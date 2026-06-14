using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedDatesToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedCuttingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedCuttingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedLastInspectionDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedPackagingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedPackagingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedSewingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedSewingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedCuttingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedCuttingStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedLastInspectionDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedPackagingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedPackagingStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedSewingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedSewingStartDate",
                table: "Orders");
        }
    }
}
