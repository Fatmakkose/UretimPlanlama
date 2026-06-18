using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningPhaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessoryCompletionDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ActualFabricMeterage",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ButtonStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProductionCompleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPurchasingCompleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSampleTestCompleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MainLabelStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedFabricMeterage",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchasingMaterialsJson",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleTestJson",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WashingInstructionStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessoryCompletionDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualFabricMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ButtonStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsProductionCompleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsPurchasingCompleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSampleTestCompleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MainLabelStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedFabricMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PurchasingMaterialsJson",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SampleTestJson",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WashingInstructionStatus",
                table: "Orders");
        }
    }
}
