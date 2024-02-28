using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class themefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme");

            migrationBuilder.AlterColumn<int>(
                name: "ApiKeyID",
                table: "Theme",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKey",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme");

            migrationBuilder.AlterColumn<int>(
                name: "ApiKeyID",
                table: "Theme",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Theme_ApiKey_ApiKeyID",
                table: "Theme",
                column: "ApiKeyID",
                principalTable: "ApiKey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
