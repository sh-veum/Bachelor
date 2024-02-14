using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NetBackend.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class graphqlupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keys_AspNetUsers_UserId",
                table: "Keys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Keys",
                table: "Keys");

            migrationBuilder.RenameTable(
                name: "Keys",
                newName: "ApiKeys");

            migrationBuilder.RenameIndex(
                name: "IX_Keys_UserId",
                table: "ApiKeys",
                newName: "IX_ApiKeys_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GraphQLApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    AllowedQueries = table.Column<List<string>>(type: "text[]", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphQLApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphQLApiKeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GraphQLApiKeys_UserId",
                table: "GraphQLApiKeys",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_AspNetUsers_UserId",
                table: "ApiKeys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_AspNetUsers_UserId",
                table: "ApiKeys");

            migrationBuilder.DropTable(
                name: "GraphQLApiKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                newName: "Keys");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeys_UserId",
                table: "Keys",
                newName: "IX_Keys_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Keys",
                table: "Keys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Keys_AspNetUsers_UserId",
                table: "Keys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
