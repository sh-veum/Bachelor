using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class accesskeymigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessKey_ApiKey_ApiKeyId",
                table: "AccessKey");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessKey_GraphQLApiKey_GraphQLApiKeyId",
                table: "AccessKey");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeyPermission_GraphQLApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme");

            migrationBuilder.DropTable(
                name: "ApiKey");

            migrationBuilder.DropIndex(
                name: "IX_AccessKey_ApiKeyId",
                table: "AccessKey");

            migrationBuilder.DropIndex(
                name: "IX_AccessKey_GraphQLApiKeyId",
                table: "AccessKey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GraphQLApiKey",
                table: "GraphQLApiKey");

            migrationBuilder.DropColumn(
                name: "ApiKeyId",
                table: "AccessKey");

            migrationBuilder.DropColumn(
                name: "GraphQLApiKeyId",
                table: "AccessKey");

            migrationBuilder.RenameTable(
                name: "GraphQLApiKey",
                newName: "IApiKey");

            migrationBuilder.RenameIndex(
                name: "IX_GraphQLApiKey_UserId",
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
                name: "FK_AccessKeyPermission_IApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermission",
                column: "GraphQLApiKeyId",
                principalTable: "IApiKey",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IApiKey_AccessKey_AccessKeyId",
                table: "IApiKey",
                column: "AccessKeyId",
                principalTable: "AccessKey",
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
                name: "FK_AccessKeyPermission_IApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_IApiKey_AccessKey_AccessKeyId",
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
                newName: "GraphQLApiKey");

            migrationBuilder.RenameIndex(
                name: "IX_IApiKey_UserId",
                table: "GraphQLApiKey",
                newName: "IX_GraphQLApiKey_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "ApiKeyId",
                table: "AccessKey",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GraphQLApiKeyId",
                table: "AccessKey",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GraphQLApiKey",
                table: "GraphQLApiKey",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ApiKey",
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
                    table.PrimaryKey("PK_ApiKey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessKey_ApiKeyId",
                table: "AccessKey",
                column: "ApiKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessKey_GraphQLApiKeyId",
                table: "AccessKey",
                column: "GraphQLApiKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKey_ApiKey_ApiKeyId",
                table: "AccessKey",
                column: "ApiKeyId",
                principalTable: "ApiKey",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKey_GraphQLApiKey_GraphQLApiKeyId",
                table: "AccessKey",
                column: "GraphQLApiKeyId",
                principalTable: "GraphQLApiKey",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeyPermission_GraphQLApiKey_GraphQLApiKeyId",
                table: "AccessKeyPermission",
                column: "GraphQLApiKeyId",
                principalTable: "GraphQLApiKey",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKey",
                principalColumn: "Id");
        }
    }
}
