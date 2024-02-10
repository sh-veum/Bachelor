using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class apikeyexpirationdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Keys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ExpiresIn",
                table: "Keys",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Keys");

            migrationBuilder.DropColumn(
                name: "ExpiresIn",
                table: "Keys");
        }
    }
}
