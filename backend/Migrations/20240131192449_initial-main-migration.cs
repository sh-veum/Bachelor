using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class initialmainmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiseaseZoneHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseZoneHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportRestrictionAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalityNo = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRestrictionAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoJsonLineStrings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoJsonLineStrings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoJsonPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoJsonPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoJsonPolygons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoJsonPolygons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IlaControlAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForskNr = table.Column<string>(type: "text", nullable: true),
                    ForskNavn = table.Column<string>(type: "text", nullable: true),
                    ForskLink = table.Column<string>(type: "text", nullable: true),
                    SistEndret = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: true),
                    OriginalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IlaControlAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdControlAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForskNr = table.Column<string>(type: "text", nullable: true),
                    ForskNavn = table.Column<string>(type: "text", nullable: true),
                    ForskLink = table.Column<string>(type: "text", nullable: true),
                    SistEndret = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: true),
                    OriginalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdControlAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExportRestrictionAreaLinks",
                columns: table => new
                {
                    ExportRestrictionAreaId = table.Column<int>(type: "integer", nullable: false),
                    DiseaseZoneHistoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRestrictionAreaLinks", x => new { x.ExportRestrictionAreaId, x.DiseaseZoneHistoryId });
                    table.ForeignKey(
                        name: "FK_ExportRestrictionAreaLinks_DiseaseZoneHistories_DiseaseZone~",
                        column: x => x.DiseaseZoneHistoryId,
                        principalTable: "DiseaseZoneHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExportRestrictionAreaLinks_ExportRestrictionAreas_ExportRes~",
                        column: x => x.ExportRestrictionAreaId,
                        principalTable: "ExportRestrictionAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IlaProtectionAreaLinks",
                columns: table => new
                {
                    IlaControlAreaId = table.Column<int>(type: "integer", nullable: false),
                    DiseaseZoneHistoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IlaProtectionAreaLinks", x => new { x.IlaControlAreaId, x.DiseaseZoneHistoryId });
                    table.ForeignKey(
                        name: "FK_IlaProtectionAreaLinks_DiseaseZoneHistories_DiseaseZoneHist~",
                        column: x => x.DiseaseZoneHistoryId,
                        principalTable: "DiseaseZoneHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IlaProtectionAreaLinks_IlaControlAreas_IlaControlAreaId",
                        column: x => x.IlaControlAreaId,
                        principalTable: "IlaControlAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IlaSurveillanceAreaLinks",
                columns: table => new
                {
                    IlaControlAreaId = table.Column<int>(type: "integer", nullable: false),
                    DiseaseZoneHistoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IlaSurveillanceAreaLinks", x => new { x.IlaControlAreaId, x.DiseaseZoneHistoryId });
                    table.ForeignKey(
                        name: "FK_IlaSurveillanceAreaLinks_DiseaseZoneHistories_DiseaseZoneHi~",
                        column: x => x.DiseaseZoneHistoryId,
                        principalTable: "DiseaseZoneHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IlaSurveillanceAreaLinks_IlaControlAreas_IlaControlAreaId",
                        column: x => x.IlaControlAreaId,
                        principalTable: "IlaControlAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PdProtectionAreaLinks",
                columns: table => new
                {
                    PdControlAreaId = table.Column<int>(type: "integer", nullable: false),
                    DiseaseZoneHistoryId = table.Column<int>(type: "integer", nullable: false),
                    IlaControlAreaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdProtectionAreaLinks", x => new { x.PdControlAreaId, x.DiseaseZoneHistoryId });
                    table.ForeignKey(
                        name: "FK_PdProtectionAreaLinks_DiseaseZoneHistories_DiseaseZoneHisto~",
                        column: x => x.DiseaseZoneHistoryId,
                        principalTable: "DiseaseZoneHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PdProtectionAreaLinks_IlaControlAreas_IlaControlAreaId",
                        column: x => x.IlaControlAreaId,
                        principalTable: "IlaControlAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PdProtectionAreaLinks_PdControlAreas_PdControlAreaId",
                        column: x => x.PdControlAreaId,
                        principalTable: "PdControlAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PdSurveillanceAreaLinks",
                columns: table => new
                {
                    PdControlAreaId = table.Column<int>(type: "integer", nullable: false),
                    DiseaseZoneHistoryId = table.Column<int>(type: "integer", nullable: false),
                    IlaControlAreaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdSurveillanceAreaLinks", x => new { x.PdControlAreaId, x.DiseaseZoneHistoryId });
                    table.ForeignKey(
                        name: "FK_PdSurveillanceAreaLinks_DiseaseZoneHistories_DiseaseZoneHis~",
                        column: x => x.DiseaseZoneHistoryId,
                        principalTable: "DiseaseZoneHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PdSurveillanceAreaLinks_IlaControlAreas_IlaControlAreaId",
                        column: x => x.IlaControlAreaId,
                        principalTable: "IlaControlAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PdSurveillanceAreaLinks_PdControlAreas_PdControlAreaId",
                        column: x => x.PdControlAreaId,
                        principalTable: "PdControlAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DiseaseZoneHistories",
                column: "Id",
                values: new object[]
                {
                    1,
                    2,
                    3
                });

            migrationBuilder.InsertData(
                table: "ExportRestrictionAreas",
                columns: new[] { "Id", "LocalityNo", "Week", "Year" },
                values: new object[,]
                {
                    { 1, 1, 1, 2022 },
                    { 2, 2, 2, 2022 }
                });

            migrationBuilder.InsertData(
                table: "IlaControlAreas",
                columns: new[] { "Id", "ForskLink", "ForskNavn", "ForskNr", "FromDate", "OriginalDate", "SistEndret", "ToDate", "Version" },
                values: new object[,]
                {
                    { 1, "http://example.com/controlarea1", "Control Area 1", "ILA001", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, "http://example.com/controlarea2", "Control Area 2", "ILA002", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "PdControlAreas",
                columns: new[] { "Id", "ForskLink", "ForskNavn", "ForskNr", "FromDate", "OriginalDate", "SistEndret", "ToDate", "Version" },
                values: new object[,]
                {
                    { 1, "http://example.com/area1", "Area 1", "001", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, "http://example.com/area2", "Area 2", "002", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Liten kantnål" },
                    { 2, "Torsk" },
                    { 3, "Sei" },
                    { 4, "Laks" },
                    { 5, "Ørret" }
                });

            migrationBuilder.InsertData(
                table: "ExportRestrictionAreaLinks",
                columns: new[] { "DiseaseZoneHistoryId", "ExportRestrictionAreaId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "IlaProtectionAreaLinks",
                columns: new[] { "DiseaseZoneHistoryId", "IlaControlAreaId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "IlaSurveillanceAreaLinks",
                columns: new[] { "DiseaseZoneHistoryId", "IlaControlAreaId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "PdProtectionAreaLinks",
                columns: new[] { "DiseaseZoneHistoryId", "PdControlAreaId", "IlaControlAreaId" },
                values: new object[,]
                {
                    { 2, 1, null },
                    { 3, 2, null }
                });

            migrationBuilder.InsertData(
                table: "PdSurveillanceAreaLinks",
                columns: new[] { "DiseaseZoneHistoryId", "PdControlAreaId", "IlaControlAreaId" },
                values: new object[,]
                {
                    { 1, 1, null },
                    { 2, 2, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportRestrictionAreaLinks_DiseaseZoneHistoryId",
                table: "ExportRestrictionAreaLinks",
                column: "DiseaseZoneHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IlaProtectionAreaLinks_DiseaseZoneHistoryId",
                table: "IlaProtectionAreaLinks",
                column: "DiseaseZoneHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IlaSurveillanceAreaLinks_DiseaseZoneHistoryId",
                table: "IlaSurveillanceAreaLinks",
                column: "DiseaseZoneHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PdProtectionAreaLinks_DiseaseZoneHistoryId",
                table: "PdProtectionAreaLinks",
                column: "DiseaseZoneHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PdProtectionAreaLinks_IlaControlAreaId",
                table: "PdProtectionAreaLinks",
                column: "IlaControlAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PdSurveillanceAreaLinks_DiseaseZoneHistoryId",
                table: "PdSurveillanceAreaLinks",
                column: "DiseaseZoneHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PdSurveillanceAreaLinks_IlaControlAreaId",
                table: "PdSurveillanceAreaLinks",
                column: "IlaControlAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ExportRestrictionAreaLinks");

            migrationBuilder.DropTable(
                name: "GeoJsonLineStrings");

            migrationBuilder.DropTable(
                name: "GeoJsonPoints");

            migrationBuilder.DropTable(
                name: "GeoJsonPolygons");

            migrationBuilder.DropTable(
                name: "IlaProtectionAreaLinks");

            migrationBuilder.DropTable(
                name: "IlaSurveillanceAreaLinks");

            migrationBuilder.DropTable(
                name: "PdProtectionAreaLinks");

            migrationBuilder.DropTable(
                name: "PdSurveillanceAreaLinks");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ExportRestrictionAreas");

            migrationBuilder.DropTable(
                name: "DiseaseZoneHistories");

            migrationBuilder.DropTable(
                name: "IlaControlAreas");

            migrationBuilder.DropTable(
                name: "PdControlAreas");
        }
    }
}
