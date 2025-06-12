using System.Text.Json.Serialization;

namespace Resources.API.Models;

public record Competency(int Id, string Name)
{
    public Competency() : this(0, string.Empty)
    {
        Resources = new List<Resource>();
    }

    [JsonIgnore]
    public ICollection<Resource> Resources { get; set; }
} 