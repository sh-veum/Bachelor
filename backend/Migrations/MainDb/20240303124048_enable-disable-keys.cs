using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class enabledisablekeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "GraphQLApiKeys",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "GraphQLApiKeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKeys",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ApiKeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "GraphQLApiKeys");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ApiKeys");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "GraphQLApiKeys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKeys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
