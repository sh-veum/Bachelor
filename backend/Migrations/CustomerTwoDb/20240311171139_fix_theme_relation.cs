using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.CustomerTwoDb
{
    /// <inheritdoc />
    public partial class fix_theme_relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_RestApiKey_RestApiKeyID",
                table: "Theme");

            migrationBuilder.DropIndex(
                name: "IX_Theme_RestApiKeyID",
                table: "Theme");

            migrationBuilder.DropColumn(
                name: "RestApiKeyID",
                table: "Theme");

            migrationBuilder.CreateTable(
                name: "RestApiKeyTheme",
                columns: table => new
                {
                    RestApiKeysId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestApiKeyTheme", x => new { x.RestApiKeysId, x.ThemesId });
                    table.ForeignKey(
                        name: "FK_RestApiKeyTheme_RestApiKey_RestApiKeysId",
                        column: x => x.RestApiKeysId,
                        principalTable: "RestApiKey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestApiKeyTheme_Theme_ThemesId",
                        column: x => x.ThemesId,
                        principalTable: "Theme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestApiKeyTheme_ThemesId",
                table: "RestApiKeyTheme",
                column: "ThemesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestApiKeyTheme");

            migrationBuilder.AddColumn<Guid>(
                name: "RestApiKeyID",
                table: "Theme",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Theme_RestApiKeyID",
                table: "Theme",
                column: "RestApiKeyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_RestApiKey_RestApiKeyID",
                table: "Theme",
                column: "RestApiKeyID",
                principalTable: "RestApiKey",
                principalColumn: "Id");
        }
    }
}
