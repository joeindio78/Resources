using System.Text.Json.Serialization;

namespace Resources.API.Models;

public class Competency
{
    public Competency(int id, string name)
    {
        Id = id;
        Name = name;
        Resources = new List<Resource>();
    }

    private Competency()
    {
        Resources = new List<Resource>();
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonIgnore]
    public ICollection<Resource> Resources { get; set; }
} 