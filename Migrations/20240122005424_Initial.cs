using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportRestrictionAreaLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalityNo = table.Column<long>(type: "bigint", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Week = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRestrictionAreaLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportRestrictionAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalityNo = table.Column<long>(type: "bigint", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Week = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRestrictionAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoJsonGeometry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoJsonGeometry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalityDtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalityNo = table.Column<long>(type: "bigint", nullable: true),
                    LocalityWeekId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    HasReportedLice = table.Column<bool>(type: "boolean", nullable: true),
                    IsFallow = table.Column<bool>(type: "boolean", nullable: true),
                    AvgAdultFemaleLice = table.Column<double>(type: "double precision", nullable: true),
                    HasCleanerfishDeployed = table.Column<bool>(type: "boolean", nullable: true),
                    HasMechanicalRemoval = table.Column<bool>(type: "boolean", nullable: true),
                    HasSubstanceTreatments = table.Column<bool>(type: "boolean", nullable: true),
                    HasPd = table.Column<bool>(type: "boolean", nullable: true),
                    HasIla = table.Column<bool>(type: "boolean", nullable: true),
                    MunicipalityNo = table.Column<string>(type: "text", nullable: true),
                    Municipality = table.Column<string>(type: "text", nullable: true),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lon = table.Column<double>(type: "double precision", nullable: true),
                    IsOnLand = table.Column<bool>(type: "boolean", nullable: true),
                    InFilteredSelection = table.Column<bool>(type: "boolean", nullable: true),
                    HasSalmonoids = table.Column<bool>(type: "boolean", nullable: true),
                    IsSlaughterHoldingCage = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalityDtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationNameIdDtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrgNo = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationNameIdDtos", x => x.Id);
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
                name: "CodSpawningGroundDtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaceName = table.Column<string>(type: "text", nullable: true),
                    Information = table.Column<string>(type: "text", nullable: true),
                    AreaDescription = table.Column<string>(type: "text", nullable: true),
                    Origin = table.Column<string>(type: "text", nullable: true),
                    Bmvalue = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<int>(type: "integer", nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeometryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodSpawningGroundDtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodSpawningGroundDtos_GeoJsonGeometry_GeometryId",
                        column: x => x.GeometryId,
                        principalTable: "GeoJsonGeometry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalityIlaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalityNo = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsReportingLocality = table.Column<bool>(type: "boolean", nullable: false),
                    IlaSuspected = table.Column<bool>(type: "boolean", nullable: false),
                    IlaConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PositionId = table.Column<int>(type: "integer", nullable: true),
                    ExportRestrictionAreaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalityIlaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalityIlaLinks_ExportRestrictionAreas_ExportRestrictionAr~",
                        column: x => x.ExportRestrictionAreaId,
                        principalTable: "ExportRestrictionAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalityIlaLinks_GeoJsonGeometry_PositionId",
                        column: x => x.PositionId,
                        principalTable: "GeoJsonGeometry",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "CodSpawningGroundDtos",
                columns: new[] { "Id", "AreaDescription", "Bmvalue", "GeometryId", "Information", "Origin", "PlaceName", "RegisteredDate", "Value" },
                values: new object[,]
                {
                    { 1, "Description of Area One", "BMValue1", null, "Info about Spawning Ground One", "Origin One", "Spawning Ground One", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100 },
                    { 2, "Description of Area Two", "BMValue2", null, "Info about Spawning Ground Two", "Origin Two", "Spawning Ground Two", new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 200 }
                });

            migrationBuilder.InsertData(
                table: "ExportRestrictionAreaLink",
                columns: new[] { "Id", "LocalityNo", "Week", "Year" },
                values: new object[,]
                {
                    { 1, 1L, 1, 2023 },
                    { 2, 2L, 2, 2023 }
                });

            migrationBuilder.InsertData(
                table: "ExportRestrictionAreas",
                columns: new[] { "Id", "LocalityNo", "Week", "Year" },
                values: new object[,]
                {
                    { 1, 1L, 1, 2022 },
                    { 2, 2L, 2, 2022 }
                });

            migrationBuilder.InsertData(
                table: "LocalityDtos",
                columns: new[] { "Id", "AvgAdultFemaleLice", "HasCleanerfishDeployed", "HasIla", "HasMechanicalRemoval", "HasPd", "HasReportedLice", "HasSalmonoids", "HasSubstanceTreatments", "InFilteredSelection", "IsFallow", "IsOnLand", "IsSlaughterHoldingCage", "Lat", "LocalityNo", "LocalityWeekId", "Lon", "Municipality", "MunicipalityNo", "Name" },
                values: new object[,]
                {
                    { 1, 0.29999999999999999, true, true, false, false, true, true, true, false, false, true, false, 59.914000000000001, 101L, 202301L, 10.752000000000001, "Municipality One", "M001", "Locality A" },
                    { 2, 0.69999999999999996, false, false, true, true, false, false, false, true, true, false, true, 60.390999999999998, 102L, 202302L, 5.3220000000000001, "Municipality Two", "M002", "Locality B" }
                });

            migrationBuilder.InsertData(
                table: "OrganizationNameIdDtos",
                columns: new[] { "Id", "Name", "OrgNo" },
                values: new object[,]
                {
                    { 1, "Organization One", 1 },
                    { 2, "Organization Two", 2 }
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

            migrationBuilder.CreateIndex(
                name: "IX_CodSpawningGroundDtos_GeometryId",
                table: "CodSpawningGroundDtos",
                column: "GeometryId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalityIlaLinks_ExportRestrictionAreaId",
                table: "LocalityIlaLinks",
                column: "ExportRestrictionAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalityIlaLinks_PositionId",
                table: "LocalityIlaLinks",
                column: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodSpawningGroundDtos");

            migrationBuilder.DropTable(
                name: "ExportRestrictionAreaLink");

            migrationBuilder.DropTable(
                name: "LocalityDtos");

            migrationBuilder.DropTable(
                name: "LocalityIlaLinks");

            migrationBuilder.DropTable(
                name: "OrganizationNameIdDtos");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "ExportRestrictionAreas");

            migrationBuilder.DropTable(
                name: "GeoJsonGeometry");
        }
    }
}
