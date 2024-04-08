using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetBackend.Migrations
{
    /// <inheritdoc />
    public partial class updateWaterQualityLogtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key_Timestamp",
                table: "WaterQualityLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "WaterQualityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "WaterQualityLogs");

            migrationBuilder.AddColumn<string>(
                name: "Key_Timestamp",
                table: "WaterQualityLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
