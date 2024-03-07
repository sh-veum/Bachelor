using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class accesskeymigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeyPermissions_GraphQLApiKeys_GraphQLApiKeyId",
                table: "AccessKeyPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeys_ApiKeys_ApiKeyId",
                table: "AccessKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeys_GraphQLApiKeys_GraphQLApiKeyId",
                table: "AccessKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphQLApiKeys_AspNetUsers_UserId",
                table: "GraphQLApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKeys_ApiKeyID",
                table: "Theme");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_AccessKeys_ApiKeyId",
                table: "AccessKeys");

            migrationBuilder.DropIndex(
                name: "IX_AccessKeys_GraphQLApiKeyId",
                table: "AccessKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GraphQLApiKeys",
                table: "GraphQLApiKeys");

            migrationBuilder.DropColumn(
                name: "ApiKeyId",
                table: "AccessKeys");

            migrationBuilder.DropColumn(
                name: "GraphQLApiKeyId",
                table: "AccessKeys");

            migrationBuilder.RenameTable(
                name: "GraphQLApiKeys",
                newName: "IApiKey");

            migrationBuilder.RenameIndex(
                name: "IX_GraphQLApiKeys_UserId",
                table: "IApiKey",
                newName: "IX_IApiKey_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessKeyId",
                table: "IApiKey",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "IApiKey",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IApiKey",
                table: "IApiKey",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_IApiKey_AccessKeyId",
                table: "IApiKey",
                column: "AccessKeyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeyPermissions_IApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermissions",
                column: "GraphQLApiKeyId",
                principalTable: "IApiKey",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IApiKey_AccessKeys_AccessKeyId",
                table: "IApiKey",
                column: "AccessKeyId",
                principalTable: "AccessKeys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IApiKey_AspNetUsers_UserId",
                table: "IApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_IApiKey_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "IApiKey",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeyPermissions_IApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_IApiKey_AccessKeys_AccessKeyId",
                table: "IApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_IApiKey_AspNetUsers_UserId",
                table: "IApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_Theme_IApiKey_ApiKeyID",
                table: "Theme");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IApiKey",
                table: "IApiKey");

            migrationBuilder.DropIndex(
                name: "IX_IApiKey_AccessKeyId",
                table: "IApiKey");

            migrationBuilder.DropColumn(
                name: "AccessKeyId",
                table: "IApiKey");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "IApiKey");

            migrationBuilder.RenameTable(
                name: "IApiKey",
                newName: "GraphQLApiKeys");

            migrationBuilder.RenameIndex(
                name: "IX_IApiKey_UserId",
                table: "GraphQLApiKeys",
                newName: "IX_GraphQLApiKeys_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "ApiKeyId",
                table: "AccessKeys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GraphQLApiKeyId",
                table: "AccessKeys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GraphQLApiKeys",
                table: "GraphQLApiKeys",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessKeys_ApiKeyId",
                table: "AccessKeys",
                column: "ApiKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessKeys_GraphQLApiKeyId",
                table: "AccessKeys",
                column: "GraphQLApiKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_UserId",
                table: "ApiKeys",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeyPermissions_GraphQLApiKeys_GraphQLApiKeyId",
                table: "AccessKeyPermissions",
                column: "GraphQLApiKeyId",
                principalTable: "GraphQLApiKeys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeys_ApiKeys_ApiKeyId",
                table: "AccessKeys",
                column: "ApiKeyId",
                principalTable: "ApiKeys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeys_GraphQLApiKeys_GraphQLApiKeyId",
                table: "AccessKeys",
                column: "GraphQLApiKeyId",
                principalTable: "GraphQLApiKeys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphQLApiKeys_AspNetUsers_UserId",
                table: "GraphQLApiKeys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKeys_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKeys",
                principalColumn: "Id");
        }
    }
}
