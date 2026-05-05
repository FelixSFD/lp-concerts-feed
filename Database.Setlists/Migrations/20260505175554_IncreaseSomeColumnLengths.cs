using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseSomeColumnLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VariantName",
                table: "SongVariant",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SongVariant",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "SongMashup",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Song",
                type: "varchar(127)",
                maxLength: 127,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Song",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleOverride",
                table: "SetlistEntry",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExtraNotes",
                table: "SetlistEntry",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SetlistAct",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Setlist",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcertTitle",
                table: "Setlist",
                type: "varchar(127)",
                maxLength: 127,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Album",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VariantName",
                table: "SongVariant",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SongVariant",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "SongMashup",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Song",
                type: "varchar(31)",
                maxLength: 31,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Song",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleOverride",
                table: "SetlistEntry",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExtraNotes",
                table: "SetlistEntry",
                type: "varchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SetlistAct",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Setlist",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcertTitle",
                table: "Setlist",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127);

            migrationBuilder.AlterColumn<string>(
                name: "LinkinpediaUrl",
                table: "Album",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(127)",
                oldMaxLength: 127,
                oldNullable: true);
        }
    }
}
