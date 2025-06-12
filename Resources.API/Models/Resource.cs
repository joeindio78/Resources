using System.Collections.Immutable;

namespace Resources.API.Models;

public class Resource
{
    public Resource(int id, string? name, DateOnly? birthDate = null, int yearsOfExperience = 0)
    {
        Id = id;
        Name = name;
        BirthDate = birthDate;
        YearsOfExperience = yearsOfExperience;
        Competencies = new List<Competency>();
    }

    private Resource()
    {
        Competencies = new List<Competency>();
    }

    public int Id { get; set; }
    public string? Name { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int YearsOfExperience { get; set; }
    public ICollection<Competency> Competencies { get; set; }

    public Resource WithCompetencies(ICollection<Competency> competencies)
    {
        Competencies = competencies;
        return this;
    }
} 