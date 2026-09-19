using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseTourIdLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "TourLeg",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31);

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "TourLeg",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Tour",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31);

            migrationBuilder.AlterColumn<string>(
                name: "TourLegId",
                table: "Concert",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "Concert",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(31)",
                oldMaxLength: 31,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "TourLeg",
                type: "varchar(31)",
                maxLength: 31,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "TourLeg",
                type: "varchar(31)",
                maxLength: 31,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Tour",
                type: "varchar(31)",
                maxLength: 31,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "TourLegId",
                table: "Concert",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "Concert",
                type: "varchar(31)",
                maxLength: 31,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(63)",
                oldMaxLength: 63,
                oldNullable: true);
        }
    }
}
