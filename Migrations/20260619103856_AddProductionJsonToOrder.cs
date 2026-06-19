using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionJsonToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductionJson",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionJson",
                table: "Orders");
        }
    }
}
