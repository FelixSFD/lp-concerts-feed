using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Tours.Migrations
{
    /// <inheritdoc />
    public partial class RestrictCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_State_CountryCode_StateCode",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_Concert_TourLeg_TourLegId",
                table: "Concert");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_City_CountryCode_CityId",
                table: "Venue");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_State_CountryCode_StateCode",
                table: "Venue");

            migrationBuilder.AddForeignKey(
                name: "FK_City_State_CountryCode_StateCode",
                table: "City",
                columns: new[] { "CountryCode", "StateCode" },
                principalTable: "State",
                principalColumns: new[] { "CountryCode", "Code" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Concert_TourLeg_TourLegId",
                table: "Concert",
                column: "TourLegId",
                principalTable: "TourLeg",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_City_CountryCode_CityId",
                table: "Venue",
                columns: new[] { "CountryCode", "CityId" },
                principalTable: "City",
                principalColumns: new[] { "CountryCode", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_State_CountryCode_StateCode",
                table: "Venue",
                columns: new[] { "CountryCode", "StateCode" },
                principalTable: "State",
                principalColumns: new[] { "CountryCode", "Code" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_State_CountryCode_StateCode",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_Concert_TourLeg_TourLegId",
                table: "Concert");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_City_CountryCode_CityId",
                table: "Venue");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_State_CountryCode_StateCode",
                table: "Venue");

            migrationBuilder.AddForeignKey(
                name: "FK_City_State_CountryCode_StateCode",
                table: "City",
                columns: new[] { "CountryCode", "StateCode" },
                principalTable: "State",
                principalColumns: new[] { "CountryCode", "Code" });

            migrationBuilder.AddForeignKey(
                name: "FK_Concert_TourLeg_TourLegId",
                table: "Concert",
                column: "TourLegId",
                principalTable: "TourLeg",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_City_CountryCode_CityId",
                table: "Venue",
                columns: new[] { "CountryCode", "CityId" },
                principalTable: "City",
                principalColumns: new[] { "CountryCode", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_State_CountryCode_StateCode",
                table: "Venue",
                columns: new[] { "CountryCode", "StateCode" },
                principalTable: "State",
                principalColumns: new[] { "CountryCode", "Code" });
        }
    }
}
