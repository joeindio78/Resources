using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Resources.API.Data;
using Resources.API.Models;
using Resources.API.Telemetry;

namespace Resources.API.Services;

public interface IResourceService
{
    Task<PagedResult<Resource>> ListResourcesAsync(
        int pageNumber,
        int pageSize,
        string? name = null,
        int? minAge = null,
        int? maxAge = null,
        int? minYearsOfExperience = null,
        int? maxYearsOfExperience = null,
        string? competency = null,
        string? sortBy = null,
        string? sortDirection = null);
    Task<Resource?> GetResourceByIdAsync(int id);
    Task<Resource> CreateResourceAsync(CreateResourceRequest request);
    Task<Resource?> UpdateResourceAsync(int id, UpdateResourceRequest request);
    Task<bool> DeleteResourceAsync(int id);
}

public class ResourceService : IResourceService
{
    private readonly ResourcesDbContext _context;
    private static readonly ActivitySource _activitySource = ActivitySources.ResourcesApi;

    public ResourceService(ResourcesDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Resource>> ListResourcesAsync(
        int pageNumber,
        int pageSize,
        string? name = null,
        int? minAge = null,
        int? maxAge = null,
        int? minYearsOfExperience = null,
        int? maxYearsOfExperience = null,
        string? competency = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        using var activity = _activitySource.StartActivity("ListResources");
        activity?.SetTag("pageNumber", pageNumber);
        activity?.SetTag("pageSize", pageSize);
        activity?.SetTag("filters.name", name);
        activity?.SetTag("filters.minAge", minAge);
        activity?.SetTag("filters.maxAge", maxAge);
        activity?.SetTag("filters.minYearsOfExperience", minYearsOfExperience);
        activity?.SetTag("filters.maxYearsOfExperience", maxYearsOfExperience);
        activity?.SetTag("filters.competency", competency);
        activity?.SetTag("sort.by", sortBy);
        activity?.SetTag("sort.direction", sortDirection);

        var query = _context.Resources
            .Include(r => r.Competencies)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(r => r.Name != null && 
                r.Name.ToLower().Contains(name.ToLower()));
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (minAge.HasValue || maxAge.HasValue)
        {
            query = query.Where(r => r.BirthDate.HasValue &&
                (!minAge.HasValue || today.Year - r.BirthDate.Value.Year >= minAge.Value) &&
                (!maxAge.HasValue || today.Year - r.BirthDate.Value.Year <= maxAge.Value));
        }

        if (minYearsOfExperience.HasValue)
        {
            query = query.Where(r => r.YearsOfExperience >= minYearsOfExperience.Value);
        }

        if (maxYearsOfExperience.HasValue)
        {
            query = query.Where(r => r.YearsOfExperience <= maxYearsOfExperience.Value);
        }

        if (!string.IsNullOrWhiteSpace(competency))
        {
            query = query.Where(r => r.Competencies.Any(c => c.Name.Contains(competency)));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var isDescending = sortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
            
            query = sortBy.ToLowerInvariant() switch
            {
                "name" => isDescending 
                    ? query.OrderByDescending(r => r.Name)
                    : query.OrderBy(r => r.Name),
                "yearsofexperience" => isDescending 
                    ? query.OrderByDescending(r => r.YearsOfExperience)
                    : query.OrderBy(r => r.YearsOfExperience),
                "birthdate" => isDescending 
                    ? query.OrderByDescending(r => r.BirthDate)
                    : query.OrderBy(r => r.BirthDate),
                _ => query.OrderBy(r => r.Id) // Default sorting
            };
        }
        else
        {
            query = query.OrderBy(r => r.Id); // Default sorting
        }

        var totalCount = await query.CountAsync();
        activity?.SetTag("totalCount", totalCount);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        activity?.SetTag("resultCount", items.Count);

        return new PagedResult<Resource>(
            items,
            totalCount,
            pageNumber,
            pageSize
        );
    }

    public async Task<Resource?> GetResourceByIdAsync(int id)
    {
        using var activity = _activitySource.StartActivity("GetResourceById");
        activity?.SetTag("resourceId", id);

        var resource = await _context.Resources
            .Include(r => r.Competencies)
            .FirstOrDefaultAsync(r => r.Id == id);

        activity?.SetTag("found", resource != null);
        return resource;
    }

    public async Task<Resource> CreateResourceAsync(CreateResourceRequest request)
    {
        using var activity = _activitySource.StartActivity("CreateResource");
        activity?.SetTag("resourceName", request.Name);
        activity?.SetTag("competencyCount", request.CompetencyIds.Length);

        try
        {
            // Validate and get competencies
            var selectedCompetencies = await _context.Competencies
                .Where(c => request.CompetencyIds.Contains(c.Id))
                .ToListAsync();

            if (!selectedCompetencies.Any())
            {
                throw new ArgumentException("At least one valid competency must be specified");
            }

            var resource = new Resource(
                0,
                request.Name,
                request.BirthDate,
                request.YearsOfExperience
            ).WithCompetencies(selectedCompetencies);

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            activity?.SetTag("success", true);
            activity?.SetTag("resourceId", resource.Id);
            return resource;
        }
        catch (Exception ex)
        {
            activity?.SetTag("success", false);
            activity?.SetTag("error", ex.Message);
            throw;
        }
    }

    public async Task<Resource?> UpdateResourceAsync(int id, UpdateResourceRequest request)
    {
        using var activity = _activitySource.StartActivity("UpdateResource");
        activity?.SetTag("resourceId", id);
        activity?.SetTag("competencyCount", request.CompetencyIds.Length);

        try
        {
            var existingResource = await _context.Resources
                .Include(r => r.Competencies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingResource == null)
            {
                activity?.SetTag("found", false);
                return null;
            }

            activity?.SetTag("found", true);

            // Validate and get competencies
            var selectedCompetencies = await _context.Competencies
                .Where(c => request.CompetencyIds.Contains(c.Id))
                .ToListAsync();

            if (!selectedCompetencies.Any())
            {
                throw new ArgumentException("At least one valid competency must be specified");
            }

            existingResource.Name = request.Name ?? existingResource.Name;
            existingResource.BirthDate = request.BirthDate ?? existingResource.BirthDate;
            existingResource.YearsOfExperience = request.YearsOfExperience ?? existingResource.YearsOfExperience;
            
            existingResource.Competencies.Clear();
            foreach (var competency in selectedCompetencies)
            {
                existingResource.Competencies.Add(competency);
            }
            
            await _context.SaveChangesAsync();

            activity?.SetTag("success", true);
            return existingResource;
        }
        catch (Exception ex)
        {
            activity?.SetTag("success", false);
            activity?.SetTag("error", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteResourceAsync(int id)
    {
        using var activity = _activitySource.StartActivity("DeleteResource");
        activity?.SetTag("resourceId", id);

        try
        {
            var resource = await _context.Resources
                .Include(r => r.Competencies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                activity?.SetTag("found", false);
                return false;
            }

            activity?.SetTag("found", true);

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            activity?.SetTag("success", true);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetTag("success", false);
            activity?.SetTag("error", ex.Message);
            throw;
        }
    }
} 