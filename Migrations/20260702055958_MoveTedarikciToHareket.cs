using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class MoveTedarikciToHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tedarikci",
                table: "StokKartlari");

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "StokKartlari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tedarikci",
                table: "StokHareketler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "StokKartlari");

            migrationBuilder.DropColumn(
                name: "Tedarikci",
                table: "StokHareketler");

            migrationBuilder.AddColumn<string>(
                name: "Tedarikci",
                table: "StokKartlari",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
