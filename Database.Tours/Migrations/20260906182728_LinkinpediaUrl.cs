using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class LinkinpediaUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkinpediaUrl",
                table: "Concert",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true);

            migrationBuilder.InsertData(
                table: "ConcertType",
                columns: new[] { "Id", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1u, "Linkin Park Show", null },
                    { 2u, "Festival", null },
                    { 3u, "Other", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConcertType",
                keyColumn: "Id",
                keyValue: 1u);

            migrationBuilder.DeleteData(
                table: "ConcertType",
                keyColumn: "Id",
                keyValue: 2u);

            migrationBuilder.DeleteData(
                table: "ConcertType",
                keyColumn: "Id",
                keyValue: 3u);

            migrationBuilder.DropColumn(
                name: "LinkinpediaUrl",
                table: "Concert");
        }
    }
}
