using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Users.Migrations
{
    /// <inheritdoc />
    public partial class UserOriginCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginCountryCode",
                table: "User",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginCountryCode",
                table: "User");
        }
    }
}
