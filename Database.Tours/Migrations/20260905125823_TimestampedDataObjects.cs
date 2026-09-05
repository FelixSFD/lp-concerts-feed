using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class TimestampedDataObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Venue",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Venue",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "TourLeg",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "TourLeg",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tour",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tour",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "State",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "State",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PreviousVenueName",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PreviousVenueName",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Country",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Country",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ConcertType",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ConcertType",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Concert",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Concert",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "City",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "City",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TourLeg");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TourLeg");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "State");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "State");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PreviousVenueName");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PreviousVenueName");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConcertType");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConcertType");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Concert");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Concert");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "City");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "City");
        }
    }
}
