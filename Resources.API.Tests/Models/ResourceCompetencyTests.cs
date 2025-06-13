using Resources.API.Models;
using Xunit;

namespace Resources.API.Tests.Models;

public class ResourceCompetencyTests
{
    [Fact]
    public void ResourceCompetency_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var resource = new Resource(
            id: 1,
            name: "Test Resource",
            birthDate: DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            yearsOfExperience: 5,
            competencies: new List<Competency>()
        );

        var competency = new Competency
        {
            Id = 1,
            Name = "Test Competency"
        };

        // Act
        var resourceCompetency = new ResourceCompetency
        {
            ResourceId = resource.Id,
            Resource = resource,
            CompetencyId = competency.Id,
            Competency = competency
        };

        // Assert
        Assert.Equal(1, resourceCompetency.ResourceId);
        Assert.Equal(resource, resourceCompetency.Resource);
        Assert.Equal(1, resourceCompetency.CompetencyId);
        Assert.Equal(competency, resourceCompetency.Competency);
    }

    [Fact]
    public void ResourceCompetency_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var resourceCompetency = new ResourceCompetency();

        // Assert
        Assert.Equal(0, resourceCompetency.ResourceId);
        Assert.Equal(0, resourceCompetency.CompetencyId);
        Assert.Null(resourceCompetency.Resource);
        Assert.Null(resourceCompetency.Competency);
    }
} 