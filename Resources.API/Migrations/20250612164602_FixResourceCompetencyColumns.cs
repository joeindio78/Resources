using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Resources.API.Migrations
{
    /// <inheritdoc />
    public partial class FixResourceCompetencyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 8, 3 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 4, 4 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 9, 4 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 10, 5 });

            migrationBuilder.DeleteData(
                table: "Competencies",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Competencies",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ResourceCompetencies",
                columns: new[] { "CompetencyId", "ResourceId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 3, 2 },
                    { 4, 3 },
                    { 5, 3 },
                    { 6, 4 },
                    { 7, 4 },
                    { 1, 5 },
                    { 8, 5 }
                });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1988, 3, 20), 6 });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1992, 9, 10), 4 });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1987, 12, 5), 7 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 6, 4 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 7, 4 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "ResourceCompetencies",
                keyColumns: new[] { "CompetencyId", "ResourceId" },
                keyValues: new object[] { 8, 5 });

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resources",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Resources",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT");

            migrationBuilder.InsertData(
                table: "Competencies",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 9, "Docker" },
                    { 10, "Azure" }
                });

            migrationBuilder.InsertData(
                table: "ResourceCompetencies",
                columns: new[] { "CompetencyId", "ResourceId" },
                values: new object[,]
                {
                    { 6, 1 },
                    { 7, 2 },
                    { 3, 3 },
                    { 8, 3 },
                    { 4, 4 },
                    { 5, 5 }
                });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1995, 3, 20), 3 });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1992, 8, 10), 6 });

            migrationBuilder.UpdateData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BirthDate", "YearsOfExperience" },
                values: new object[] { new DateOnly(1988, 12, 25), 10 });

            migrationBuilder.InsertData(
                table: "ResourceCompetencies",
                columns: new[] { "CompetencyId", "ResourceId" },
                values: new object[,]
                {
                    { 9, 4 },
                    { 10, 5 }
                });
        }
    }
}
