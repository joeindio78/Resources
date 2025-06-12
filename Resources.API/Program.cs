using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Resources.API.Configuration;
using Resources.API.Data;
using Resources.API.Health;
using Resources.API.Models;
using Resources.API.Services;
using Resources.API.Settings;
using Resources.API.Telemetry;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Resources.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        // API Versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen();

        // Rate Limiting
        builder.Services.Configure<RateLimitSettings>(
            builder.Configuration.GetSection("RateLimitSettings"));

        var rateLimitSettings = builder.Configuration
            .GetSection("RateLimitSettings")
            .Get<RateLimitSettings>() ?? new RateLimitSettings();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("fixed", options =>
            {
                options.PermitLimit = rateLimitSettings.PermitLimit;
                options.Window = TimeSpan.FromSeconds(rateLimitSettings.Window);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = rateLimitSettings.QueueLimit;
            });
        });

        // Health Checks
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "ready" });

        builder.Services.AddScoped<DatabaseHealthCheck>(sp =>
        {
            var context = sp.GetRequiredService<ResourcesDbContext>();
            return new DatabaseHealthCheck(() => context.Database.GetDbConnection());
        });

        // OpenTelemetry
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ActivitySources.ServiceName))
            .WithTracing(tracing => tracing
                .AddSource(ActivitySources.ServiceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddConsoleExporter());

        // Configuration
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings?.Key == null)
        {
            throw new InvalidOperationException("JWT Key is not configured");
        }

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.Configure<AdminUserSettings>(builder.Configuration.GetSection("AdminUser"));

        // JWT Configuration
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        // Add SQLite and configure the context
        builder.Services.AddDbContext<ResourcesDbContext>(options =>
            options.UseSqlite("Data Source=resources.db"));

        // Register services
        builder.Services.AddScoped<IResourceService, ResourceService>();

        var app = builder.Build();

        // Configure middleware
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();
            
            // Build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, $"Resources API {name}");
            }
        });

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        // Health check endpoint
        app.MapHealthChecks("/health");

        // Create and migrate the database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ResourcesDbContext>();
            context.Database.Migrate();
        }

        var v1 = app.NewVersionedApi("v1");
        // Auth endpoint
        v1.MapPost("/v{version:apiVersion}/login", (IAuthService authService, LoginRequest request) =>
        {
            var response = authService.Login(request);
            if (response is null)
                return Results.Unauthorized();
            return Results.Ok(response);
        })
        .HasApiVersion(1.0)
        .WithName("Login")
        .WithTags("Auth")
        .WithDescription("Authenticate and receive a JWT token");

        var resourcesApi = v1.MapGroup("/v{version:apiVersion}/resources")
            .HasApiVersion(1.0)
            .RequireAuthorization()
            .WithTags("Resources")
            .RequireRateLimiting("fixed");

        resourcesApi.MapGet("/", async (
            IResourceService resourceService,
            int? page,
            int? pageSize,
            string? name,
            int? minAge,
            int? maxAge,
            int? minYearsOfExperience,
            int? maxYearsOfExperience,
            string? competency,
            string? sortBy,
            string? sortDirection) =>
        {
            page = page is null or <= 0 ? 1 : page.Value;
            pageSize = pageSize is null or <= 0 ? 10 : pageSize.Value;

            return await resourceService.ListResourcesAsync(
                page.Value,
                pageSize.Value,
                name,
                minAge,
                maxAge,
                minYearsOfExperience,
                maxYearsOfExperience,
                competency,
                sortBy,
                sortDirection);
        });

        resourcesApi.MapGet("/{id}", async (IResourceService resourceService, int id) =>
        {
            var resource = await resourceService.GetResourceByIdAsync(id);
            return resource is null ? Results.NotFound() : Results.Ok(resource);
        });

        resourcesApi.MapPost("/", async (IResourceService resourceService, CreateResourceRequest request) =>
        {
            try
            {
                var resource = await resourceService.CreateResourceAsync(request);
                return Results.Created($"/resources/{resource.Id}", resource);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        resourcesApi.MapPut("/{id}", async (IResourceService resourceService, int id, UpdateResourceRequest request) =>
        {
            try
            {
                var resource = await resourceService.UpdateResourceAsync(id, request);
                return resource is null ? Results.NotFound() : Results.Ok(resource);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        resourcesApi.MapDelete("/{id}", async (IResourceService resourceService, int id) =>
        {
            try
            {
                var success = await resourceService.DeleteResourceAsync(id);
                return success ? Results.NoContent() : Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        var competenciesApi = v1.MapGroup("/v{version:apiVersion}/competencies")
            .HasApiVersion(1.0)
            .RequireAuthorization()
            .WithTags("Competencies")
            .RequireRateLimiting("fixed");

        competenciesApi.MapGet("/", async (ResourcesDbContext db) => 
            await db.Competencies.ToListAsync());

        app.Run();
    }
}

public record CreateResourceRequest(
    string Name,
    DateOnly? BirthDate,
    int YearsOfExperience,
    int[] CompetencyIds
);

public record UpdateResourceRequest(
    string? Name,
    DateOnly? BirthDate,
    int? YearsOfExperience,
    int[] CompetencyIds
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int CurrentPage,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
};

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token);

[JsonSerializable(typeof(Models.Resource[]))]
[JsonSerializable(typeof(Competency[]))]
[JsonSerializable(typeof(PagedResult<Models.Resource>))]
[JsonSerializable(typeof(CreateResourceRequest))]
[JsonSerializable(typeof(UpdateResourceRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
