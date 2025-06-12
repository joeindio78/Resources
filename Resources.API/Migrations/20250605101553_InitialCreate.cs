using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Resources.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCompetencies",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetencyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCompetencies", x => new { x.ResourceId, x.CompetencyId });
                    table.ForeignKey(
                        name: "FK_ResourceCompetencies_Competencies_CompetencyId",
                        column: x => x.CompetencyId,
                        principalTable: "Competencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceCompetencies_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Competencies",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "C#" },
                    { 2, "JavaScript" },
                    { 3, "Python" },
                    { 4, "Java" },
                    { 5, "SQL" },
                    { 6, "React" },
                    { 7, "Angular" },
                    { 8, "Node.js" },
                    { 9, "Docker" },
                    { 10, "Azure" }
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "BirthDate", "Name", "YearsOfExperience" },
                values: new object[,]
                {
                    { 1, new DateOnly(1990, 1, 1), "John Doe", 5 },
                    { 2, new DateOnly(1985, 6, 15), "Jane Smith", 8 },
                    { 3, new DateOnly(1995, 3, 20), "Bob Johnson", 3 },
                    { 4, new DateOnly(1992, 8, 10), "Alice Brown", 6 },
                    { 5, new DateOnly(1988, 12, 25), "Charlie Wilson", 10 }
                });

            migrationBuilder.InsertData(
                table: "ResourceCompetencies",
                columns: new[] { "CompetencyId", "ResourceId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 6, 1 },
                    { 2, 2 },
                    { 7, 2 },
                    { 3, 3 },
                    { 8, 3 },
                    { 4, 4 },
                    { 9, 4 },
                    { 5, 5 },
                    { 10, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCompetencies_CompetencyId",
                table: "ResourceCompetencies",
                column: "CompetencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceCompetencies");

            migrationBuilder.DropTable(
                name: "Competencies");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
