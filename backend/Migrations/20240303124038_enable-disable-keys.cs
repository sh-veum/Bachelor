using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class enabledisablekeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "GraphQLApiKey",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "GraphQLApiKey",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKey",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ApiKey",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "GraphQLApiKey");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ApiKey");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "GraphQLApiKey",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKey",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
