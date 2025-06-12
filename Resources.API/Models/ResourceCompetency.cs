namespace Resources.API.Models;

public class ResourceCompetency
{
    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    public int CompetencyId { get; set; }
    public Competency Competency { get; set; } = null!;
} 