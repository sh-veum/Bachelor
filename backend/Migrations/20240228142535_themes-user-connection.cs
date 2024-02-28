using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class themesuserconnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKey_AspNetUsers_UserId",
                table: "ApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Theme",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GraphQLApiKey",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ApiKey",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Theme_UserId",
                table: "Theme",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKey_AspNetUsers_UserId",
                table: "ApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKey_AspNetUsers_UserId",
                table: "ApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_Theme_AspNetUsers_UserId",
                table: "Theme");

            migrationBuilder.DropIndex(
                name: "IX_Theme_UserId",
                table: "Theme");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Theme");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GraphQLApiKey",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ApiKey",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKey_AspNetUsers_UserId",
                table: "ApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphQLApiKey_AspNetUsers_UserId",
                table: "GraphQLApiKey",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
