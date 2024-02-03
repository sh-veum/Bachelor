using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class keydtoapiendpointfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyName",
                table: "Keys",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyName",
                table: "Keys");
        }
    }
}
