using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddOzelliklerJsonToStokKarti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boyut",
                table: "StokKartlari");

            migrationBuilder.DropColumn(
                name: "Renk",
                table: "StokKartlari");

            migrationBuilder.AddColumn<string>(
                name: "OzelliklerJson",
                table: "StokKartlari",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OzelliklerJson",
                table: "StokKartlari");

            migrationBuilder.AddColumn<string>(
                name: "Boyut",
                table: "StokKartlari",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Renk",
                table: "StokKartlari",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
