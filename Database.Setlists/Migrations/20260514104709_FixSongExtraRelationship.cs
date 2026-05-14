using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class FixSongExtraRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetlistEntrySongExtra_SetlistEntry_SetlistEntryId",
                table: "SetlistEntrySongExtra");

            migrationBuilder.AlterColumn<string>(
                name: "SetlistEntryId",
                table: "SetlistEntrySongExtra",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SetlistEntrySongExtra_SetlistEntry_SetlistEntryId",
                table: "SetlistEntrySongExtra",
                column: "SetlistEntryId",
                principalTable: "SetlistEntry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetlistEntrySongExtra_SetlistEntry_SetlistEntryId",
                table: "SetlistEntrySongExtra");

            migrationBuilder.AlterColumn<string>(
                name: "SetlistEntryId",
                table: "SetlistEntrySongExtra",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_SetlistEntrySongExtra_SetlistEntry_SetlistEntryId",
                table: "SetlistEntrySongExtra",
                column: "SetlistEntryId",
                principalTable: "SetlistEntry",
                principalColumn: "Id");
        }
    }
}
