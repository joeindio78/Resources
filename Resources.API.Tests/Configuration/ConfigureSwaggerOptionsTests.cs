using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Moq;
using Resources.API.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Resources.API.Tests.Configuration
{
    public class ConfigureSwaggerOptionsTests
    {
        private readonly Mock<IApiVersionDescriptionProvider> _apiVersionDescriptionProvider;
        private readonly ApiVersionDescription _versionDescription;
        private readonly SwaggerGenOptions _options;
        private readonly ConfigureSwaggerOptions _sut;

        public ConfigureSwaggerOptionsTests()
        {
            _apiVersionDescriptionProvider = new Mock<IApiVersionDescriptionProvider>();
            var apiVersion = new ApiVersion(1, 0);
            _versionDescription = new ApiVersionDescription(apiVersion, "v1", false);
            _apiVersionDescriptionProvider.Setup(p => p.ApiVersionDescriptions)
                .Returns(new[] { _versionDescription });

            _options = new SwaggerGenOptions();
            _sut = new ConfigureSwaggerOptions(_apiVersionDescriptionProvider.Object);
        }

        [Fact]
        public void Configure_SetsUpSwaggerDoc()
        {
            // Act
            _sut.Configure(_options);

            // Assert
            var swaggerDocs = _options.SwaggerGeneratorOptions.SwaggerDocs;
            Assert.Single(swaggerDocs);
            Assert.True(swaggerDocs.ContainsKey("v1"));

            var doc = swaggerDocs["v1"];
            Assert.Equal("Resources API v1", doc.Title);
            Assert.Equal("1.0", doc.Version);
            Assert.NotNull(doc.Description);
        }

        [Fact]
        public void Configure_SetsUpSecurityDefinition()
        {
            // Act
            _sut.Configure(_options);

            // Assert
            var securitySchemes = _options.SwaggerGeneratorOptions.SecuritySchemes;
            Assert.Single(securitySchemes);
            Assert.True(securitySchemes.ContainsKey("Bearer"));

            var scheme = securitySchemes["Bearer"];
            Assert.Equal("Bearer", scheme.Name);
            Assert.Equal(SecuritySchemeType.Http, scheme.Type);
            Assert.Equal("bearer", scheme.Scheme);
            Assert.Equal("JWT Authorization header using the Bearer scheme.", scheme.Description);
        }
    }
} 