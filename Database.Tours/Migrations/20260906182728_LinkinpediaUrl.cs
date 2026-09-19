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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkinpediaUrl",
                table: "Concert");
        }
    }
}
