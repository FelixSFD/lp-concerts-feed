using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class AddAppleMusicIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "appleMusicIdOverride",
                table: "SongVariant",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "appleMusicId",
                table: "Song",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "appleMusicIdOverride",
                table: "SongVariant");

            migrationBuilder.DropColumn(
                name: "appleMusicId",
                table: "Song");
        }
    }
}
