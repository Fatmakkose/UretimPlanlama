using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CuttingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CuttingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FabricArrivalActualDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FabricArrivalAgreedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FabricMeterage",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInspectionDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PackagingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PackagingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SewingEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SewingStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SewingWorkshop",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuttingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CuttingStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricArrivalActualDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricArrivalAgreedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LastInspectionDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PackagingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PackagingStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SewingEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SewingStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SewingWorkshop",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WarehouseArrivalDate",
                table: "Orders");
        }
    }
}
