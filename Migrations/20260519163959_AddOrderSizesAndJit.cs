using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSizesAndJit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJIT",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SalesRegion",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Size2XL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Size3XL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeM",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeS",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeXL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsJIT",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SalesRegion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Size2XL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Size3XL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SizeL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SizeM",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SizeS",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SizeXL",
                table: "Orders");
        }
    }
}
