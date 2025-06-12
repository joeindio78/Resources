using System.Collections.Immutable;

namespace Resources.API.Models;

public class Resource
{
    public Resource(int id, string name, DateOnly birthDate, int yearsOfExperience, List<Competency> competencies)
    {
        Id = id;
        Name = name;
        BirthDate = birthDate;
        YearsOfExperience = yearsOfExperience;
        Competencies = competencies;
    }

    private Resource()
    {
        Name = string.Empty;
        Competencies = new List<Competency>();
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateOnly BirthDate { get; set; }
    public int YearsOfExperience { get; set; }
    public ICollection<Competency> Competencies { get; set; }
} 