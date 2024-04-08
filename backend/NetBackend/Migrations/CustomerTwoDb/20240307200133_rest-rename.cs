using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.CustomerTwoDb
{
    /// <inheritdoc />
    public partial class restrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme");

            migrationBuilder.DropTable(
                name: "ApiKey");

            migrationBuilder.RenameColumn(
                name: "ApiKeyID",
                table: "Theme",
                newName: "RestApiKeyID");

            migrationBuilder.RenameIndex(
                name: "IX_Theme_ApiKeyID",
                table: "Theme",
                newName: "IX_Theme_RestApiKeyID");

            migrationBuilder.CreateTable(
                name: "RestApiKey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestApiKey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestApiKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestApiKey_UserId",
                table: "RestApiKey",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_RestApiKey_RestApiKeyID",
                table: "Theme",
                column: "RestApiKeyID",
                principalTable: "RestApiKey",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_RestApiKey_RestApiKeyID",
                table: "Theme");

            migrationBuilder.DropTable(
                name: "RestApiKey");

            migrationBuilder.RenameColumn(
                name: "RestApiKeyID",
                table: "Theme",
                newName: "ApiKeyID");

            migrationBuilder.RenameIndex(
                name: "IX_Theme_RestApiKeyID",
                table: "Theme",
                newName: "IX_Theme_ApiKeyID");

            migrationBuilder.CreateTable(
                name: "ApiKey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: true),
                    KeyName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKey",
                principalColumn: "Id");
        }
    }
}
