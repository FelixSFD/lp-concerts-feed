using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Users.Migrations
{
    /// <inheritdoc />
    public partial class CreateCalendarSubTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarSubscription",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    UserId = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true),
                    IncludeDoorsTime = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeLpStageTime = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarSubscription_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSubscription_UserId",
                table: "CalendarSubscription",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarSubscription");
        }
    }
}
