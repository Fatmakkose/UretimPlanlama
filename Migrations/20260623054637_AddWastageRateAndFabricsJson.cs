using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddWastageRateAndFabricsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FabricsJson",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WastageRate",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FabricsJson",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WastageRate",
                table: "Orders");
        }
    }
}
