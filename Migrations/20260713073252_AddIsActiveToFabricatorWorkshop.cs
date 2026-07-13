using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToFabricatorWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Workshops",
                type: "bit",
                nullable: false,
                defaultValue: false);


            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Fabricators",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Workshops");


            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Fabricators");
        }
    }
}
