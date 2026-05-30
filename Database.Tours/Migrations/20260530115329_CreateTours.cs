using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class CreateTours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConcertType",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcertType", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    IsoCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    NativeName = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.IsoCode);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tour",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tour", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    CountryCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    NativeName = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => new { x.CountryCode, x.Code });
                    table.ForeignKey(
                        name: "FK_State_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "IsoCode",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourLeg",
                columns: table => new
                {
                    TourId = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    Id = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourLeg", x => new { x.TourId, x.Id });
                    table.UniqueConstraint("AK_TourLeg_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourLeg_Tour_TourId",
                        column: x => x.TourId,
                        principalTable: "Tour",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CountryCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    StateCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Id = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    NativeName = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => new { x.CountryCode, x.StateCode, x.Id });
                    table.ForeignKey(
                        name: "FK_City_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "IsoCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_City_State_CountryCode_StateCode",
                        columns: x => new { x.CountryCode, x.StateCode },
                        principalTable: "State",
                        principalColumns: new[] { "CountryCode", "Code" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Venue",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    StateCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    CityId = table.Column<uint>(type: "int unsigned", nullable: false),
                    CurrentName = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false),
                    TimeZone = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venue_City_CountryCode_StateCode_CityId",
                        columns: x => new { x.CountryCode, x.StateCode, x.CityId },
                        principalTable: "City",
                        principalColumns: new[] { "CountryCode", "StateCode", "Id" });
                    table.ForeignKey(
                        name: "FK_Venue_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "IsoCode");
                    table.ForeignKey(
                        name: "FK_Venue_State_CountryCode_StateCode",
                        columns: x => new { x.CountryCode, x.StateCode },
                        principalTable: "State",
                        principalColumns: new[] { "CountryCode", "Code" });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Concert",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    ConcertTypeId = table.Column<uint>(type: "int unsigned", nullable: false),
                    TourId = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: true),
                    TourLegId = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: true),
                    CustomTitle = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: true),
                    VenueId = table.Column<uint>(type: "int unsigned", nullable: false),
                    PostedStartTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    MainStageTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DoorsTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LpuEarlyEntryTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LpuEarlyEntryConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExpectedSetDurationMinutes = table.Column<uint>(type: "int unsigned", nullable: false),
                    ScheduleImageFile = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concert_ConcertType_ConcertTypeId",
                        column: x => x.ConcertTypeId,
                        principalTable: "ConcertType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Concert_TourLeg_TourLegId",
                        column: x => x.TourLegId,
                        principalTable: "TourLeg",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Concert_Tour_TourId",
                        column: x => x.TourId,
                        principalTable: "Tour",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Concert_Venue_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PreviousVenueName",
                columns: table => new
                {
                    VenueId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Id = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false),
                    From = table.Column<DateOnly>(type: "date", nullable: false),
                    To = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousVenueName", x => new { x.VenueId, x.Id });
                    table.ForeignKey(
                        name: "FK_PreviousVenueName_Venue_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Concert_ConcertTypeId",
                table: "Concert",
                column: "ConcertTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Concert_TourId",
                table: "Concert",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Concert_TourLegId",
                table: "Concert",
                column: "TourLegId");

            migrationBuilder.CreateIndex(
                name: "IX_Concert_VenueId",
                table: "Concert",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Venue_CountryCode_StateCode_CityId",
                table: "Venue",
                columns: new[] { "CountryCode", "StateCode", "CityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Concert");

            migrationBuilder.DropTable(
                name: "PreviousVenueName");

            migrationBuilder.DropTable(
                name: "ConcertType");

            migrationBuilder.DropTable(
                name: "TourLeg");

            migrationBuilder.DropTable(
                name: "Venue");

            migrationBuilder.DropTable(
                name: "Tour");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
