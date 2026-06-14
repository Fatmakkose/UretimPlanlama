using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CollarInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuffInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFifLabel",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInnerBarcode",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasOtherCard",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPriceCard",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWashingInstruction",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasYokeLabel",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LargeButtonCount",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlacketInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PocketFlapInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmallButtonCount",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitFabricMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollarInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CuffInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasFifLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasInnerBarcode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasOtherCard",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasPriceCard",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasWashingInstruction",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasYokeLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LargeButtonCount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlacketInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PocketFlapInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SmallButtonCount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitFabricMeterage",
                table: "Orders");
        }
    }
}
