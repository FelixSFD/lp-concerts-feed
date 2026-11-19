using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyLastGeneratedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastGeneratedAt",
                table: "CalendarSubscription",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGeneratedAt",
                table: "CalendarSubscription");
        }
    }
}
