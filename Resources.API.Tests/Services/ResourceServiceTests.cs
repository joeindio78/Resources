using Microsoft.EntityFrameworkCore;
using Resources.API.Data;
using Resources.API.Models;
using Resources.API.Services;
using Xunit;

namespace Resources.API.Tests.Services;

public class ResourceServiceTests : IDisposable
{
    private readonly ResourcesDbContext _context;
    private readonly ResourceService _resourceService;
    private int _nextCompetencyId = 1000;  // Start with a high number to avoid conflicts
    private int _nextResourceId = 2000;    // Start with a high number to avoid conflicts

    public ResourceServiceTests()
    {
        var options = new DbContextOptionsBuilder<ResourcesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ResourcesDbContext(options);
        _resourceService = new ResourceService(_context);
        SeedTestData();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void ClearContext()
    {
        _context.Resources.RemoveRange(_context.Resources);
        _context.Competencies.RemoveRange(_context.Competencies);
        _context.SaveChanges();
    }

    private Competency CreateUniqueCompetency(string name)
    {
        var competency = new Competency(_nextCompetencyId++, name);
        _context.Competencies.Add(competency);
        return competency;
    }

    private Resource CreateUniqueResource(string name, DateOnly birthDate, int yearsOfExperience, List<Competency> competencies)
    {
        return new Resource(_nextResourceId++, name, birthDate, yearsOfExperience, competencies);
    }

    private void SeedTestData()
    {
        // Create competencies first
        var csharp = CreateUniqueCompetency("C#");
        var sql = CreateUniqueCompetency("SQL");
        var javascript = CreateUniqueCompetency("JavaScript");
        _context.SaveChanges();  // Save competencies before creating resources that reference them

        // Create resources
        var resources = new List<Resource>
        {
            CreateUniqueResource("John Doe", new DateOnly(1990, 1, 1), 5, new List<Competency> { csharp, sql }),
            CreateUniqueResource("Jane Smith", new DateOnly(1985, 6, 15), 8, new List<Competency> { sql, javascript })
        };

        _context.Resources.AddRange(resources);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ListResourcesAsync_ReturnsPagedResult()
    {
        // Act
        var result = await _resourceService.ListResourcesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task ListResourcesAsync_WithFilters_ReturnsFilteredResults()
    {
        // Act
        var result = await _resourceService.ListResourcesAsync(
            pageNumber: 1,
            pageSize: 10,
            name: "John",
            minAge: 30,
            maxAge: 40,
            minYearsOfExperience: 4,
            maxYearsOfExperience: 6,
            competency: "C#"
        );

        // Assert
        Assert.NotNull(result);
        var item = Assert.Single(result.Items);
        Assert.Equal("John Doe", item.Name);
        Assert.Equal(5, item.YearsOfExperience);
        Assert.Contains(item.Competencies, c => c.Name == "C#");
    }

    [Fact]
    public async Task GetResourceByIdAsync_WithValidId_ReturnsResource()
    {
        // Act
        var resource = await _resourceService.GetResourceByIdAsync(2000);  // First seeded resource ID

        // Assert
        Assert.NotNull(resource);
        Assert.Equal("John Doe", resource.Name);
    }

    [Fact]
    public async Task CreateResourceAsync_WithValidData_CreatesResource()
    {
        // Arrange
        var competencyId = _context.Competencies.First().Id;
        var request = new CreateResourceRequest(
            "New Resource",
            new DateOnly(1995, 1, 1),
            3,
            new[] { competencyId }
        );

        // Act
        var result = await _resourceService.CreateResourceAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.BirthDate, result.BirthDate);
        Assert.Equal(request.YearsOfExperience, result.YearsOfExperience);
        Assert.Contains(result.Competencies, c => c.Id == competencyId);
    }

    [Fact]
    public async Task UpdateResourceAsync_WithValidData_UpdatesResource()
    {
        // Arrange
        var resourceId = _context.Resources.First().Id;
        var competencyId = _context.Competencies.First().Id;
        var request = new UpdateResourceRequest(
            "Updated Resource",
            new DateOnly(1995, 1, 1),
            4,
            new[] { competencyId }
        );

        // Act
        var result = await _resourceService.UpdateResourceAsync(resourceId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.BirthDate, result.BirthDate);
        Assert.Equal(request.YearsOfExperience, result.YearsOfExperience);
        Assert.Contains(result.Competencies, c => c.Id == competencyId);
    }

    [Fact]
    public async Task DeleteResourceAsync_WithValidId_DeletesResource()
    {
        // Arrange
        var resourceId = _context.Resources.First().Id;

        // Act
        var result = await _resourceService.DeleteResourceAsync(resourceId);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Resources.FindAsync(resourceId));
    }

    [Theory]
    [InlineData(1990, 1, 1, 2025, 1, 1, 35)]  // Same month and day
    [InlineData(1990, 1, 1, 2025, 2, 1, 35)]  // Later month
    [InlineData(1990, 1, 1, 2025, 1, 2, 35)]  // Later day
    [InlineData(1990, 6, 15, 2025, 6, 14, 34)] // Day before birthday
    [InlineData(1990, 6, 15, 2025, 6, 15, 35)] // On birthday
    [InlineData(1990, 6, 15, 2025, 6, 16, 35)] // Day after birthday
    [InlineData(1990, 12, 31, 2025, 1, 1, 34)] // Year boundary
    public async Task ListResourcesAsync_WithAgeFilter_CalculatesAgeCorrectly(
        int birthYear, int birthMonth, int birthDay,
        int currentYear, int currentMonth, int currentDay,
        int expectedAge)
    {
        // Arrange
        ClearContext();
        var birthDate = new DateOnly(birthYear, birthMonth, birthDay);
        var currentDate = new DateOnly(currentYear, currentMonth, currentDay);
        
        var csharpCompetency = CreateUniqueCompetency("C#");
        _context.SaveChanges();  // Save competency first

        var newResource = CreateUniqueResource("Test Person", birthDate, 5, 
            new List<Competency> { csharpCompetency });
        _context.Resources.Add(newResource);
        await _context.SaveChangesAsync();

        // Mock the current date for consistent age calculation
        var mockDateTime = new DateTime(currentYear, currentMonth, currentDay);
        _resourceService.SetCurrentDate(mockDateTime);

        // Act
        var result = await _resourceService.ListResourcesAsync(
            pageNumber: 1,
            pageSize: 10,
            minAge: expectedAge,
            maxAge: expectedAge
        );

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Items, r => r.BirthDate == birthDate);
    }

    [Fact]
    public async Task ListResourcesAsync_WithAllFilters_ReturnsFilteredResults()
    {
        // Arrange
        ClearContext();
        var csharp = CreateUniqueCompetency("C#");
        var sql = CreateUniqueCompetency("SQL");
        var javascript = CreateUniqueCompetency("JavaScript");
        _context.SaveChanges();  // Save competencies first

        var resources = new List<Resource>
        {
            CreateUniqueResource("John Developer", new DateOnly(1990, 1, 1), 5,
                new List<Competency> { csharp, sql }),
            CreateUniqueResource("Jane Engineer", new DateOnly(1995, 6, 15), 3,
                new List<Competency> { csharp, javascript }),
            CreateUniqueResource("Bob Architect", new DateOnly(1985, 12, 31), 8,
                new List<Competency> { sql, javascript })
        };
        
        _context.Resources.AddRange(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.ListResourcesAsync(
            pageNumber: 1,
            pageSize: 10,
            name: "developer",
            minAge: 30,
            maxAge: 35,
            minYearsOfExperience: 4,
            maxYearsOfExperience: 6,
            competency: "C#",
            sortBy: "yearsofexperience",
            sortDirection: "desc"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var resource = result.Items.First();
        Assert.Equal("John Developer", resource.Name);
        Assert.Equal(5, resource.YearsOfExperience);
        Assert.Contains(resource.Competencies, c => c.Name == "C#");
    }

    [Theory]
    [InlineData("yearsofexperience", "asc")]
    [InlineData("yearsofexperience", "desc")]
    [InlineData("birthdate", "asc")]
    [InlineData("birthdate", "desc")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("invalid", "asc")]  // Should default to Id sorting
    public async Task ListResourcesAsync_WithVariousSortOptions_SortsCorrectly(string sortBy, string sortDirection)
    {
        // Arrange
        ClearContext();
        var csharp = CreateUniqueCompetency("C#");
        _context.SaveChanges();  // Save competency first

        var resources = new List<Resource>
        {
            CreateUniqueResource("Charlie", new DateOnly(1990, 1, 1), 5,
                new List<Competency> { csharp }),
            CreateUniqueResource("Alice", new DateOnly(1995, 6, 15), 3,
                new List<Competency> { csharp }),
            CreateUniqueResource("Bob", new DateOnly(1985, 12, 31), 8,
                new List<Competency> { csharp })
        };
        
        _context.Resources.AddRange(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.ListResourcesAsync(
            pageNumber: 1,
            pageSize: 10,
            sortBy: sortBy,
            sortDirection: sortDirection
        );

        // Assert
        Assert.NotNull(result);
        var items = result.Items.ToList();
        Assert.Equal(3, items.Count);

        // Verify sorting based on the specified field
        switch (sortBy.ToLower())
        {
            case "yearsofexperience":
                if (sortDirection == "desc")
                    Assert.True(items[0].YearsOfExperience >= items[1].YearsOfExperience);
                else
                    Assert.True(items[0].YearsOfExperience <= items[1].YearsOfExperience);
                break;

            case "birthdate":
                if (sortDirection == "desc")
                    Assert.True(items[0].BirthDate >= items[1].BirthDate);
                else
                    Assert.True(items[0].BirthDate <= items[1].BirthDate);
                break;

            case "name":
                if (sortDirection == "desc")
                    Assert.True(string.Compare(items[0].Name, items[1].Name) >= 0);
                else
                    Assert.True(string.Compare(items[0].Name, items[1].Name) <= 0);
                break;

            default:
                // For invalid sort field, should sort by Id
                Assert.True(items[0].Id <= items[1].Id);
                break;
        }
    }
} 