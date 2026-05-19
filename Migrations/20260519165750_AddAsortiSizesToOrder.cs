using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddAsortiSizesToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsortiCount",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSize2XL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSize3XL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSizeL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSizeM",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSizeS",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AsortiSizeXL",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsortiCount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSize2XL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSize3XL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSizeL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSizeM",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSizeS",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AsortiSizeXL",
                table: "Orders");
        }
    }
}
