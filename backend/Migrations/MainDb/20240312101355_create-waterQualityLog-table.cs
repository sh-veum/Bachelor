using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class createwaterQualityLogtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestApiKeyThemes_Theme_ThemesId",
                table: "RestApiKeyThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_Theme_AspNetUsers_UserId",
                table: "Theme");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Theme",
                table: "Theme");

            migrationBuilder.RenameTable(
                name: "Theme",
                newName: "Themes");

            migrationBuilder.RenameIndex(
                name: "IX_Theme_UserId",
                table: "Themes",
                newName: "IX_Themes_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Themes",
                table: "Themes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestApiKeyThemes_Themes_ThemesId",
                table: "RestApiKeyThemes",
                column: "ThemesId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_AspNetUsers_UserId",
                table: "Themes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestApiKeyThemes_Themes_ThemesId",
                table: "RestApiKeyThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_Themes_AspNetUsers_UserId",
                table: "Themes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Themes",
                table: "Themes");

            migrationBuilder.RenameTable(
                name: "Themes",
                newName: "Theme");

            migrationBuilder.RenameIndex(
                name: "IX_Themes_UserId",
                table: "Theme",
                newName: "IX_Theme_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Theme",
                table: "Theme",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestApiKeyThemes_Theme_ThemesId",
                table: "RestApiKeyThemes",
                column: "ThemesId",
                principalTable: "Theme",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_AspNetUsers_UserId",
                table: "Theme",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
