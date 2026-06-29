using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddStokKartiToCariHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Miktar",
                table: "CariHareketler",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StokKartiId",
                table: "CariHareketler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_StokKartiId",
                table: "CariHareketler",
                column: "StokKartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_StokKartlari_StokKartiId",
                table: "CariHareketler",
                column: "StokKartiId",
                principalTable: "StokKartlari",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_StokKartlari_StokKartiId",
                table: "CariHareketler");

            migrationBuilder.DropIndex(
                name: "IX_CariHareketler_StokKartiId",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "Miktar",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "StokKartiId",
                table: "CariHareketler");
        }
    }
}
