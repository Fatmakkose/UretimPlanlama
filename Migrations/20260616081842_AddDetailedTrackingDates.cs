using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedTrackingDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualButtonArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualButtonColorQualityApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualButtonSewingThreadDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualButtonTestDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualChestCardDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualInnerLabelArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualLabelArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualPPApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualPriceCardArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualSampleFabricTestDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualTouchColorApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualWashingInstructionArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedButtonArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedButtonColorQualityApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedButtonSewingThreadDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedButtonTestDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedChestCardDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedInnerLabelArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedLabelArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedPPApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedPriceCardArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedSampleFabricTestDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedTouchColorApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedWashingInstructionArrivalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualButtonArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualButtonColorQualityApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualButtonSewingThreadDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualButtonTestDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualChestCardDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualInnerLabelArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualLabelArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualPPApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualPriceCardArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualSampleFabricTestDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualTouchColorApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualWashingInstructionArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedButtonArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedButtonColorQualityApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedButtonSewingThreadDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedButtonTestDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedChestCardDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedInnerLabelArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedLabelArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedPPApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedPriceCardArrivalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedSampleFabricTestDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedTouchColorApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlannedWashingInstructionArrivalDate",
                table: "Orders");
        }
    }
}
