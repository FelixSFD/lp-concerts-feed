using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Database.Setlists.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    LinkinpediaUrl = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Setlist",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ConcertId = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    ConcertTitle = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    SetName = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true),
                    LinkinpediaUrl = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setlist", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SongInMashup",
                columns: table => new
                {
                    SongId = table.Column<uint>(type: "int unsigned", nullable: false),
                    MashupId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongInMashup", x => new { x.MashupId, x.SongId });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SongMashup",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false),
                    LinkinpediaUrl = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongMashup", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SetlistAct",
                columns: table => new
                {
                    SetlistId = table.Column<uint>(type: "int unsigned", nullable: false),
                    ActNumber = table.Column<uint>(type: "int unsigned", nullable: false),
                    Title = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetlistAct", x => new { x.SetlistId, x.ActNumber });
                    table.ForeignKey(
                        name: "FK_SetlistAct_Setlist_SetlistId",
                        column: x => x.SetlistId,
                        principalTable: "Setlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Song",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AlbumId = table.Column<uint>(type: "int unsigned", nullable: true),
                    Title = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: false),
                    Isrc = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    LinkinpediaUrl = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true),
                    SongMashupDoId = table.Column<uint>(type: "int unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Song", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Song_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Song_SongMashup_SongMashupDoId",
                        column: x => x.SongMashupDoId,
                        principalTable: "SongMashup",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SongVariant",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SongId = table.Column<uint>(type: "int unsigned", nullable: false),
                    IsrcOverride = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    VariantName = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: true),
                    Description = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongVariant_Song_SongId",
                        column: x => x.SongId,
                        principalTable: "Song",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SetlistEntry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    SetlistId = table.Column<uint>(type: "int unsigned", nullable: false),
                    ActNumber = table.Column<uint>(type: "int unsigned", nullable: true),
                    SortNumber = table.Column<uint>(type: "int unsigned", nullable: false),
                    SongNumber = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TitleOverride = table.Column<string>(type: "varchar(31)", maxLength: 31, nullable: true),
                    ExtraNotes = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: true),
                    PlayedSongId = table.Column<uint>(type: "int unsigned", nullable: true),
                    PlayedSongVariantId = table.Column<uint>(type: "int unsigned", nullable: true),
                    PlayedMashupId = table.Column<uint>(type: "int unsigned", nullable: true),
                    IsRotationSong = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPlayedFromRecording = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWorldPremiere = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsLivePremiere = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetlistEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetlistEntry_SetlistAct_SetlistId_ActNumber",
                        columns: x => new { x.SetlistId, x.ActNumber },
                        principalTable: "SetlistAct",
                        principalColumns: new[] { "SetlistId", "ActNumber" });
                    table.ForeignKey(
                        name: "FK_SetlistEntry_Setlist_SetlistId",
                        column: x => x.SetlistId,
                        principalTable: "Setlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetlistEntry_SongMashup_PlayedMashupId",
                        column: x => x.PlayedMashupId,
                        principalTable: "SongMashup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetlistEntry_SongVariant_PlayedSongVariantId",
                        column: x => x.PlayedSongVariantId,
                        principalTable: "SongVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetlistEntry_Song_PlayedSongId",
                        column: x => x.PlayedSongId,
                        principalTable: "Song",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SetlistEntrySongExtra",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    SetlistEntryId = table.Column<string>(type: "varchar(255)", nullable: true),
                    SongId = table.Column<uint>(type: "int unsigned", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetlistEntrySongExtra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetlistEntrySongExtra_SetlistEntry_SetlistEntryId",
                        column: x => x.SetlistEntryId,
                        principalTable: "SetlistEntry",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetlistEntrySongExtra_Song_SongId",
                        column: x => x.SongId,
                        principalTable: "Song",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntry_PlayedMashupId",
                table: "SetlistEntry",
                column: "PlayedMashupId");

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntry_PlayedSongId",
                table: "SetlistEntry",
                column: "PlayedSongId");

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntry_PlayedSongVariantId",
                table: "SetlistEntry",
                column: "PlayedSongVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntry_SetlistId_ActNumber",
                table: "SetlistEntry",
                columns: new[] { "SetlistId", "ActNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntrySongExtra_SetlistEntryId",
                table: "SetlistEntrySongExtra",
                column: "SetlistEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SetlistEntrySongExtra_SongId",
                table: "SetlistEntrySongExtra",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Song_AlbumId",
                table: "Song",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Song_SongMashupDoId",
                table: "Song",
                column: "SongMashupDoId");

            migrationBuilder.CreateIndex(
                name: "IX_SongVariant_SongId",
                table: "SongVariant",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetlistEntrySongExtra");

            migrationBuilder.DropTable(
                name: "SongInMashup");

            migrationBuilder.DropTable(
                name: "SetlistEntry");

            migrationBuilder.DropTable(
                name: "SetlistAct");

            migrationBuilder.DropTable(
                name: "SongVariant");

            migrationBuilder.DropTable(
                name: "Setlist");

            migrationBuilder.DropTable(
                name: "Song");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "SongMashup");
        }
    }
}
