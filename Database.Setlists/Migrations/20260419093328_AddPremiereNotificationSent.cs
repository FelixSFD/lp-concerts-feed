using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiereNotificationSent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PremiereNotificationSent",
                table: "SetlistEntry",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PremiereNotificationSent",
                table: "SetlistEntry");
        }
    }
}
