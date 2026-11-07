using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Venue",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Country",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "City",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "City");
        }
    }
}
