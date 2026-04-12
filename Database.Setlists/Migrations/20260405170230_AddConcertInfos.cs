using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class AddConcertInfos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConcertDate",
                table: "Setlist",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ConcertTourName",
                table: "Setlist",
                type: "varchar(63)",
                maxLength: 63,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcertType",
                table: "Setlist",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcertDate",
                table: "Setlist");

            migrationBuilder.DropColumn(
                name: "ConcertTourName",
                table: "Setlist");

            migrationBuilder.DropColumn(
                name: "ConcertType",
                table: "Setlist");
        }
    }
}
