using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NetBackend.Migrations.CustomerTwoDb
{
    /// <inheritdoc />
    public partial class reduceddatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportRestrictionAreaLinks");

            migrationBuilder.DropTable(
                name: "IlaProtectionAreaLinks");

            migrationBuilder.DropTable(
                name: "IlaSurveillanceAreaLinks");

            migrationBuilder.DropTable(
                name: "PdProtectionAreaLinks");

            migrationBuilder.DropTable(
                name: "PdSurveillanceAreaLinks");

            migrationBuilder.DropTable(
                name: "ExportRestrictionAreas");

            migrationBuilder.DropTable(
                name: "DiseaseZoneHistories");

            migrationBuilder.DropTable(
                name: "IlaControlAreas");

            migrationBuilder.DropTable(
                name: "PdControlAreas");

            migrationBuilder.AddColumn<string>(
                name: "KeyName",
                table: "ApiKey",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrgNo = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Organization",
                columns: new[] { "Id", "Address", "City", "Name", "OrgNo", "PostalCode" },
                values: new object[,]
                {
                    { 1, "Storvegen 303", "Ålesund", "Kompni AS", 199, "6000" },
                    { 2, "Storvegen 304", "Ålesund", "Kompni 2 AS", 200, "6001" },
                    { 3, "Storvegen 305", "Ålesund", "Kompni 3 AS", 201, "6002" }
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 6, "Makrell" },
                    { 7, "Sild" },
                    { 8, "Kveite" },
                    { 9, "Blåkveite" },
                    { 10, "Hyse" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DeleteData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "KeyName",
                table: "ApiKey");

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
                    Week = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRestrictionAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IlaControlAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForskLink = table.Column<string>(type: "text", nullable: true),
                    ForskNavn = table.Column<string>(type: "text", nullable: true),
                    ForskNr = table.Column<string>(type: "text", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SistEndret = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: true)
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
                    ForskLink = table.Column<string>(type: "text", nullable: true),
                    ForskNavn = table.Column<string>(type: "text", nullable: true),
                    ForskNr = table.Column<string>(type: "text", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SistEndret = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdControlAreas", x => x.Id);
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
    }
}
