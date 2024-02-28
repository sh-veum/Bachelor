using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NetBackend.Migrations.CustomerTwoDb
{
    /// <inheritdoc />
    public partial class inittest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessKeys", x => x.Id);
                });

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
                    DatabaseName = table.Column<string>(type: "text", nullable: true),
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
                name: "Organizations",
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
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SuperSecretNumber = table.Column<int>(type: "integer", nullable: true)
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
                name: "ApiKey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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
                name: "GraphQLApiKey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphQLApiKey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeName = table.Column<string>(type: "text", nullable: false),
                    AccessibleEndpoints = table.Column<List<string>>(type: "text[]", nullable: false),
                    ApiKeyID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Theme_ApiKey_ApiKeyID",
                        column: x => x.ApiKeyID,
                        principalTable: "ApiKey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessKeyPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueryName = table.Column<string>(type: "text", nullable: false),
                    AllowedFields = table.Column<List<string>>(type: "text[]", nullable: true),
                    GraphQLApiKeyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessKeyPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessKeyPermission_GraphQLApiKey_GraphQLApiKeyId",
                        column: x => x.GraphQLApiKeyId,
                        principalTable: "GraphQLApiKey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "Address", "City", "Name", "OrgNo", "PostalCode" },
                values: new object[,]
                {
                    { 101, "Address 1", "City 1", "CustomerTwo: Organization 1", 101, "PostalCode 1" },
                    { 102, "Address 2", "City 2", "CustomerTwo: Organization 2", 102, "PostalCode 2" },
                    { 103, "Address 3", "City 3", "CustomerTwo: Organization 3", 103, "PostalCode 3" },
                    { 104, "Address 4", "City 4", "CustomerTwo: Organization 4", 104, "PostalCode 4" },
                    { 105, "Address 5", "City 5", "CustomerTwo: Organization 5", 105, "PostalCode 5" },
                    { 106, "Address 6", "City 6", "CustomerTwo: Organization 6", 106, "PostalCode 6" },
                    { 107, "Address 7", "City 7", "CustomerTwo: Organization 7", 107, "PostalCode 7" },
                    { 108, "Address 8", "City 8", "CustomerTwo: Organization 8", 108, "PostalCode 8" },
                    { 109, "Address 9", "City 9", "CustomerTwo: Organization 9", 109, "PostalCode 9" },
                    { 110, "Address 10", "City 10", "CustomerTwo: Organization 10", 110, "PostalCode 10" },
                    { 111, "Address 11", "City 11", "CustomerTwo: Organization 11", 111, "PostalCode 11" },
                    { 112, "Address 12", "City 12", "CustomerTwo: Organization 12", 112, "PostalCode 12" },
                    { 113, "Address 13", "City 13", "CustomerTwo: Organization 13", 113, "PostalCode 13" },
                    { 114, "Address 14", "City 14", "CustomerTwo: Organization 14", 114, "PostalCode 14" },
                    { 115, "Address 15", "City 15", "CustomerTwo: Organization 15", 115, "PostalCode 15" },
                    { 116, "Address 16", "City 16", "CustomerTwo: Organization 16", 116, "PostalCode 16" },
                    { 117, "Address 17", "City 17", "CustomerTwo: Organization 17", 117, "PostalCode 17" },
                    { 118, "Address 18", "City 18", "CustomerTwo: Organization 18", 118, "PostalCode 18" },
                    { 119, "Address 19", "City 19", "CustomerTwo: Organization 19", 119, "PostalCode 19" },
                    { 120, "Address 20", "City 20", "CustomerTwo: Organization 20", 120, "PostalCode 20" },
                    { 121, "Address 21", "City 21", "CustomerTwo: Organization 21", 121, "PostalCode 21" },
                    { 122, "Address 22", "City 22", "CustomerTwo: Organization 22", 122, "PostalCode 22" },
                    { 123, "Address 23", "City 23", "CustomerTwo: Organization 23", 123, "PostalCode 23" },
                    { 124, "Address 24", "City 24", "CustomerTwo: Organization 24", 124, "PostalCode 24" },
                    { 125, "Address 25", "City 25", "CustomerTwo: Organization 25", 125, "PostalCode 25" },
                    { 126, "Address 26", "City 26", "CustomerTwo: Organization 26", 126, "PostalCode 26" },
                    { 127, "Address 27", "City 27", "CustomerTwo: Organization 27", 127, "PostalCode 27" },
                    { 128, "Address 28", "City 28", "CustomerTwo: Organization 28", 128, "PostalCode 28" },
                    { 129, "Address 29", "City 29", "CustomerTwo: Organization 29", 129, "PostalCode 29" },
                    { 130, "Address 30", "City 30", "CustomerTwo: Organization 30", 130, "PostalCode 30" },
                    { 131, "Address 31", "City 31", "CustomerTwo: Organization 31", 131, "PostalCode 31" },
                    { 132, "Address 32", "City 32", "CustomerTwo: Organization 32", 132, "PostalCode 32" },
                    { 133, "Address 33", "City 33", "CustomerTwo: Organization 33", 133, "PostalCode 33" },
                    { 134, "Address 34", "City 34", "CustomerTwo: Organization 34", 134, "PostalCode 34" },
                    { 135, "Address 35", "City 35", "CustomerTwo: Organization 35", 135, "PostalCode 35" },
                    { 136, "Address 36", "City 36", "CustomerTwo: Organization 36", 136, "PostalCode 36" },
                    { 137, "Address 37", "City 37", "CustomerTwo: Organization 37", 137, "PostalCode 37" },
                    { 138, "Address 38", "City 38", "CustomerTwo: Organization 38", 138, "PostalCode 38" },
                    { 139, "Address 39", "City 39", "CustomerTwo: Organization 39", 139, "PostalCode 39" },
                    { 140, "Address 40", "City 40", "CustomerTwo: Organization 40", 140, "PostalCode 40" },
                    { 141, "Address 41", "City 41", "CustomerTwo: Organization 41", 141, "PostalCode 41" },
                    { 142, "Address 42", "City 42", "CustomerTwo: Organization 42", 142, "PostalCode 42" },
                    { 143, "Address 43", "City 43", "CustomerTwo: Organization 43", 143, "PostalCode 43" },
                    { 144, "Address 44", "City 44", "CustomerTwo: Organization 44", 144, "PostalCode 44" },
                    { 145, "Address 45", "City 45", "CustomerTwo: Organization 45", 145, "PostalCode 45" },
                    { 146, "Address 46", "City 46", "CustomerTwo: Organization 46", 146, "PostalCode 46" },
                    { 147, "Address 47", "City 47", "CustomerTwo: Organization 47", 147, "PostalCode 47" },
                    { 148, "Address 48", "City 48", "CustomerTwo: Organization 48", 148, "PostalCode 48" },
                    { 149, "Address 49", "City 49", "CustomerTwo: Organization 49", 149, "PostalCode 49" },
                    { 150, "Address 50", "City 50", "CustomerTwo: Organization 50", 150, "PostalCode 50" },
                    { 151, "Address 51", "City 51", "CustomerTwo: Organization 51", 151, "PostalCode 51" },
                    { 152, "Address 52", "City 52", "CustomerTwo: Organization 52", 152, "PostalCode 52" },
                    { 153, "Address 53", "City 53", "CustomerTwo: Organization 53", 153, "PostalCode 53" },
                    { 154, "Address 54", "City 54", "CustomerTwo: Organization 54", 154, "PostalCode 54" },
                    { 155, "Address 55", "City 55", "CustomerTwo: Organization 55", 155, "PostalCode 55" },
                    { 156, "Address 56", "City 56", "CustomerTwo: Organization 56", 156, "PostalCode 56" },
                    { 157, "Address 57", "City 57", "CustomerTwo: Organization 57", 157, "PostalCode 57" },
                    { 158, "Address 58", "City 58", "CustomerTwo: Organization 58", 158, "PostalCode 58" },
                    { 159, "Address 59", "City 59", "CustomerTwo: Organization 59", 159, "PostalCode 59" },
                    { 160, "Address 60", "City 60", "CustomerTwo: Organization 60", 160, "PostalCode 60" },
                    { 161, "Address 61", "City 61", "CustomerTwo: Organization 61", 161, "PostalCode 61" },
                    { 162, "Address 62", "City 62", "CustomerTwo: Organization 62", 162, "PostalCode 62" },
                    { 163, "Address 63", "City 63", "CustomerTwo: Organization 63", 163, "PostalCode 63" },
                    { 164, "Address 64", "City 64", "CustomerTwo: Organization 64", 164, "PostalCode 64" },
                    { 165, "Address 65", "City 65", "CustomerTwo: Organization 65", 165, "PostalCode 65" },
                    { 166, "Address 66", "City 66", "CustomerTwo: Organization 66", 166, "PostalCode 66" },
                    { 167, "Address 67", "City 67", "CustomerTwo: Organization 67", 167, "PostalCode 67" },
                    { 168, "Address 68", "City 68", "CustomerTwo: Organization 68", 168, "PostalCode 68" },
                    { 169, "Address 69", "City 69", "CustomerTwo: Organization 69", 169, "PostalCode 69" },
                    { 170, "Address 70", "City 70", "CustomerTwo: Organization 70", 170, "PostalCode 70" },
                    { 171, "Address 71", "City 71", "CustomerTwo: Organization 71", 171, "PostalCode 71" },
                    { 172, "Address 72", "City 72", "CustomerTwo: Organization 72", 172, "PostalCode 72" },
                    { 173, "Address 73", "City 73", "CustomerTwo: Organization 73", 173, "PostalCode 73" },
                    { 174, "Address 74", "City 74", "CustomerTwo: Organization 74", 174, "PostalCode 74" },
                    { 175, "Address 75", "City 75", "CustomerTwo: Organization 75", 175, "PostalCode 75" },
                    { 176, "Address 76", "City 76", "CustomerTwo: Organization 76", 176, "PostalCode 76" },
                    { 177, "Address 77", "City 77", "CustomerTwo: Organization 77", 177, "PostalCode 77" },
                    { 178, "Address 78", "City 78", "CustomerTwo: Organization 78", 178, "PostalCode 78" },
                    { 179, "Address 79", "City 79", "CustomerTwo: Organization 79", 179, "PostalCode 79" },
                    { 180, "Address 80", "City 80", "CustomerTwo: Organization 80", 180, "PostalCode 80" },
                    { 181, "Address 81", "City 81", "CustomerTwo: Organization 81", 181, "PostalCode 81" },
                    { 182, "Address 82", "City 82", "CustomerTwo: Organization 82", 182, "PostalCode 82" },
                    { 183, "Address 83", "City 83", "CustomerTwo: Organization 83", 183, "PostalCode 83" },
                    { 184, "Address 84", "City 84", "CustomerTwo: Organization 84", 184, "PostalCode 84" },
                    { 185, "Address 85", "City 85", "CustomerTwo: Organization 85", 185, "PostalCode 85" },
                    { 186, "Address 86", "City 86", "CustomerTwo: Organization 86", 186, "PostalCode 86" },
                    { 187, "Address 87", "City 87", "CustomerTwo: Organization 87", 187, "PostalCode 87" },
                    { 188, "Address 88", "City 88", "CustomerTwo: Organization 88", 188, "PostalCode 88" },
                    { 189, "Address 89", "City 89", "CustomerTwo: Organization 89", 189, "PostalCode 89" },
                    { 190, "Address 90", "City 90", "CustomerTwo: Organization 90", 190, "PostalCode 90" },
                    { 191, "Address 91", "City 91", "CustomerTwo: Organization 91", 191, "PostalCode 91" },
                    { 192, "Address 92", "City 92", "CustomerTwo: Organization 92", 192, "PostalCode 92" },
                    { 193, "Address 93", "City 93", "CustomerTwo: Organization 93", 193, "PostalCode 93" },
                    { 194, "Address 94", "City 94", "CustomerTwo: Organization 94", 194, "PostalCode 94" },
                    { 195, "Address 95", "City 95", "CustomerTwo: Organization 95", 195, "PostalCode 95" },
                    { 196, "Address 96", "City 96", "CustomerTwo: Organization 96", 196, "PostalCode 96" },
                    { 197, "Address 97", "City 97", "CustomerTwo: Organization 97", 197, "PostalCode 97" },
                    { 198, "Address 98", "City 98", "CustomerTwo: Organization 98", 198, "PostalCode 98" },
                    { 199, "Address 99", "City 99", "CustomerTwo: Organization 99", 199, "PostalCode 99" },
                    { 200, "Address 100", "City 100", "CustomerTwo: Organization 100", 200, "PostalCode 100" }
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "Name", "SuperSecretNumber" },
                values: new object[,]
                {
                    { 101, "CustomerTwo: Species 1", 1 },
                    { 102, "CustomerTwo: Species 2", 2 },
                    { 103, "CustomerTwo: Species 3", 3 },
                    { 104, "CustomerTwo: Species 4", 4 },
                    { 105, "CustomerTwo: Species 5", 5 },
                    { 106, "CustomerTwo: Species 6", 6 },
                    { 107, "CustomerTwo: Species 7", 7 },
                    { 108, "CustomerTwo: Species 8", 8 },
                    { 109, "CustomerTwo: Species 9", 9 },
                    { 110, "CustomerTwo: Species 10", 10 },
                    { 111, "CustomerTwo: Species 11", 11 },
                    { 112, "CustomerTwo: Species 12", 12 },
                    { 113, "CustomerTwo: Species 13", 13 },
                    { 114, "CustomerTwo: Species 14", 14 },
                    { 115, "CustomerTwo: Species 15", 15 },
                    { 116, "CustomerTwo: Species 16", 16 },
                    { 117, "CustomerTwo: Species 17", 17 },
                    { 118, "CustomerTwo: Species 18", 18 },
                    { 119, "CustomerTwo: Species 19", 19 },
                    { 120, "CustomerTwo: Species 20", 20 },
                    { 121, "CustomerTwo: Species 21", 21 },
                    { 122, "CustomerTwo: Species 22", 22 },
                    { 123, "CustomerTwo: Species 23", 23 },
                    { 124, "CustomerTwo: Species 24", 24 },
                    { 125, "CustomerTwo: Species 25", 25 },
                    { 126, "CustomerTwo: Species 26", 26 },
                    { 127, "CustomerTwo: Species 27", 27 },
                    { 128, "CustomerTwo: Species 28", 28 },
                    { 129, "CustomerTwo: Species 29", 29 },
                    { 130, "CustomerTwo: Species 30", 30 },
                    { 131, "CustomerTwo: Species 31", 31 },
                    { 132, "CustomerTwo: Species 32", 32 },
                    { 133, "CustomerTwo: Species 33", 33 },
                    { 134, "CustomerTwo: Species 34", 34 },
                    { 135, "CustomerTwo: Species 35", 35 },
                    { 136, "CustomerTwo: Species 36", 36 },
                    { 137, "CustomerTwo: Species 37", 37 },
                    { 138, "CustomerTwo: Species 38", 38 },
                    { 139, "CustomerTwo: Species 39", 39 },
                    { 140, "CustomerTwo: Species 40", 40 },
                    { 141, "CustomerTwo: Species 41", 41 },
                    { 142, "CustomerTwo: Species 42", 42 },
                    { 143, "CustomerTwo: Species 43", 43 },
                    { 144, "CustomerTwo: Species 44", 44 },
                    { 145, "CustomerTwo: Species 45", 45 },
                    { 146, "CustomerTwo: Species 46", 46 },
                    { 147, "CustomerTwo: Species 47", 47 },
                    { 148, "CustomerTwo: Species 48", 48 },
                    { 149, "CustomerTwo: Species 49", 49 },
                    { 150, "CustomerTwo: Species 50", 50 },
                    { 151, "CustomerTwo: Species 51", 51 },
                    { 152, "CustomerTwo: Species 52", 52 },
                    { 153, "CustomerTwo: Species 53", 53 },
                    { 154, "CustomerTwo: Species 54", 54 },
                    { 155, "CustomerTwo: Species 55", 55 },
                    { 156, "CustomerTwo: Species 56", 56 },
                    { 157, "CustomerTwo: Species 57", 57 },
                    { 158, "CustomerTwo: Species 58", 58 },
                    { 159, "CustomerTwo: Species 59", 59 },
                    { 160, "CustomerTwo: Species 60", 60 },
                    { 161, "CustomerTwo: Species 61", 61 },
                    { 162, "CustomerTwo: Species 62", 62 },
                    { 163, "CustomerTwo: Species 63", 63 },
                    { 164, "CustomerTwo: Species 64", 64 },
                    { 165, "CustomerTwo: Species 65", 65 },
                    { 166, "CustomerTwo: Species 66", 66 },
                    { 167, "CustomerTwo: Species 67", 67 },
                    { 168, "CustomerTwo: Species 68", 68 },
                    { 169, "CustomerTwo: Species 69", 69 },
                    { 170, "CustomerTwo: Species 70", 70 },
                    { 171, "CustomerTwo: Species 71", 71 },
                    { 172, "CustomerTwo: Species 72", 72 },
                    { 173, "CustomerTwo: Species 73", 73 },
                    { 174, "CustomerTwo: Species 74", 74 },
                    { 175, "CustomerTwo: Species 75", 75 },
                    { 176, "CustomerTwo: Species 76", 76 },
                    { 177, "CustomerTwo: Species 77", 77 },
                    { 178, "CustomerTwo: Species 78", 78 },
                    { 179, "CustomerTwo: Species 79", 79 },
                    { 180, "CustomerTwo: Species 80", 80 },
                    { 181, "CustomerTwo: Species 81", 81 },
                    { 182, "CustomerTwo: Species 82", 82 },
                    { 183, "CustomerTwo: Species 83", 83 },
                    { 184, "CustomerTwo: Species 84", 84 },
                    { 185, "CustomerTwo: Species 85", 85 },
                    { 186, "CustomerTwo: Species 86", 86 },
                    { 187, "CustomerTwo: Species 87", 87 },
                    { 188, "CustomerTwo: Species 88", 88 },
                    { 189, "CustomerTwo: Species 89", 89 },
                    { 190, "CustomerTwo: Species 90", 90 },
                    { 191, "CustomerTwo: Species 91", 91 },
                    { 192, "CustomerTwo: Species 92", 92 },
                    { 193, "CustomerTwo: Species 93", 93 },
                    { 194, "CustomerTwo: Species 94", 94 },
                    { 195, "CustomerTwo: Species 95", 95 },
                    { 196, "CustomerTwo: Species 96", 96 },
                    { 197, "CustomerTwo: Species 97", 97 },
                    { 198, "CustomerTwo: Species 98", 98 },
                    { 199, "CustomerTwo: Species 99", 99 },
                    { 200, "CustomerTwo: Species 100", 100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessKeyPermission_GraphQLApiKeyId",
                table: "AccessKeyPermission",
                column: "GraphQLApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey",
                column: "UserId");

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
                name: "IX_GraphQLApiKey_UserId",
                table: "GraphQLApiKey",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Theme_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessKeyPermission");

            migrationBuilder.DropTable(
                name: "AccessKeys");

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
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Theme");

            migrationBuilder.DropTable(
                name: "GraphQLApiKey");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ApiKey");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
