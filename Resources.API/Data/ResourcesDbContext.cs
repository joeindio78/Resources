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

    public virtual DbSet<Resource> Resources => Set<Resource>();
    public virtual DbSet<Competency> Competencies => Set<Competency>();
    public virtual DbSet<ResourceCompetency> ResourceCompetencies => Set<ResourceCompetency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            
            entity.HasMany(e => e.Competencies)
                  .WithMany(e => e.Resources)
                  .UsingEntity<ResourceCompetency>();
        });

        modelBuilder.Entity<Competency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<ResourceCompetency>(entity =>
        {
            entity.HasKey(e => new { e.ResourceId, e.CompetencyId });

            entity.HasOne(rc => rc.Resource)
                  .WithMany()
                  .HasForeignKey(rc => rc.ResourceId);

            entity.HasOne(rc => rc.Competency)
                  .WithMany()
                  .HasForeignKey(rc => rc.CompetencyId);
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
            new Competency(8, "Node.js"),
            new Competency(9, "Docker"),
            new Competency(10, "Azure")
        };
        modelBuilder.Entity<Competency>().HasData(competencies);

        // Seed resources
        modelBuilder.Entity<Resource>().HasData(
            new Resource(1, "John Doe", new DateOnly(1990, 1, 1), 5),
            new Resource(2, "Jane Smith", new DateOnly(1985, 6, 15), 8),
            new Resource(3, "Bob Johnson", new DateOnly(1995, 3, 20), 3),
            new Resource(4, "Alice Brown", new DateOnly(1992, 8, 10), 6),
            new Resource(5, "Charlie Wilson", new DateOnly(1988, 12, 25), 10)
        );

        // Seed resource-competency relationships
        modelBuilder.Entity<ResourceCompetency>().HasData(
            new ResourceCompetency { ResourceId = 1, CompetencyId = 1 },  // John: C#
            new ResourceCompetency { ResourceId = 1, CompetencyId = 6 },  // John: React
            new ResourceCompetency { ResourceId = 2, CompetencyId = 2 },  // Jane: JavaScript
            new ResourceCompetency { ResourceId = 2, CompetencyId = 7 },  // Jane: Angular
            new ResourceCompetency { ResourceId = 3, CompetencyId = 3 },  // Bob: Python
            new ResourceCompetency { ResourceId = 3, CompetencyId = 8 },  // Bob: Node.js
            new ResourceCompetency { ResourceId = 4, CompetencyId = 4 },  // Alice: Java
            new ResourceCompetency { ResourceId = 4, CompetencyId = 9 },  // Alice: Docker
            new ResourceCompetency { ResourceId = 5, CompetencyId = 5 },  // Charlie: SQL
            new ResourceCompetency { ResourceId = 5, CompetencyId = 10 }  // Charlie: Azure
        );
    }
}