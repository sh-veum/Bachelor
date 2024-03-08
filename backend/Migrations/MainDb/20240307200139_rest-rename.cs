using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class restrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKeys_ApiKeyID",
                table: "Theme");

            migrationBuilder.RenameColumn(
                name: "ApiKeyID",
                table: "Theme",
                newName: "RestApiKeyID");

            migrationBuilder.RenameIndex(
                name: "IX_Theme_ApiKeyID",
                table: "Theme",
                newName: "IX_Theme_RestApiKeyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKeys_RestApiKeyID",
                table: "Theme",
                column: "RestApiKeyID",
                principalTable: "ApiKeys",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKeys_RestApiKeyID",
                table: "Theme");

            migrationBuilder.RenameColumn(
                name: "RestApiKeyID",
                table: "Theme",
                newName: "ApiKeyID");

            migrationBuilder.RenameIndex(
                name: "IX_Theme_RestApiKeyID",
                table: "Theme",
                newName: "IX_Theme_ApiKeyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKeys_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKeys",
                principalColumn: "Id");
        }
    }
}
