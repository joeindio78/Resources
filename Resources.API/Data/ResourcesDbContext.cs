using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Resources.API.Models;

namespace Resources.API.Data;

public class ResourcesDbContext : DbContext
{
    public ResourcesDbContext(DbContextOptions<ResourcesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Competency> Competencies => Set<Competency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.BirthDate).IsRequired();
            entity.Property(e => e.YearsOfExperience).IsRequired();
            
            entity.HasMany(e => e.Competencies)
                .WithMany(e => e.Resources)
                .UsingEntity<ResourceCompetency>(
                    j => j
                        .HasOne(rc => rc.Competency)
                        .WithMany()
                        .HasForeignKey(rc => rc.CompetencyId),
                    j => j
                        .HasOne(rc => rc.Resource)
                        .WithMany()
                        .HasForeignKey(rc => rc.ResourceId),
                    j =>
                    {
                        j.ToTable("ResourceCompetencies");
                        j.HasKey(t => new { t.ResourceId, t.CompetencyId });
                    }
                );
        });

        modelBuilder.Entity<Competency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        // Seed competencies
        var competencies = new[]
        {
            new Competency(1, "C#"),
            new Competency(2, "JavaScript"),
            new Competency(3, "Python"),
            new Competency(4, "Java"),
            new Competency(5, "SQL"),
            new Competency(6, "React"),
            new Competency(7, "Angular"),
            new Competency(8, "Node.js")
        };

        modelBuilder.Entity<Competency>().HasData(competencies);

        // Seed resources
        var resources = new[]
        {
            new Resource(1, "John Doe", new DateOnly(1990, 1, 1), 5, new List<Competency> { competencies[0], competencies[1] }),
            new Resource(2, "Jane Smith", new DateOnly(1985, 6, 15), 8, new List<Competency> { competencies[1], competencies[2] }),
            new Resource(3, "Bob Johnson", new DateOnly(1988, 3, 20), 6, new List<Competency> { competencies[3], competencies[4] }),
            new Resource(4, "Alice Brown", new DateOnly(1992, 9, 10), 4, new List<Competency> { competencies[5], competencies[6] }),
            new Resource(5, "Charlie Wilson", new DateOnly(1987, 12, 5), 7, new List<Competency> { competencies[7], competencies[0] })
        };

        modelBuilder.Entity<Resource>().HasData(
            resources.Select(r => new
            {
                r.Id,
                r.Name,
                r.BirthDate,
                r.YearsOfExperience
            })
        );

        // Seed resource-competency relationships
        var resourceCompetencies = resources.SelectMany(r => r.Competencies.Select(c => new
        {
            ResourceId = r.Id,
            CompetencyId = c.Id
        }));

        modelBuilder.Entity<ResourceCompetency>().HasData(resourceCompetencies);
    }
}