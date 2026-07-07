using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddStokVaryant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<int>(
                name: "StokVaryantId",
                table: "StokHareketler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StokVaryantId",
                table: "OrderMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StokVaryantlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartiId = table.Column<int>(type: "int", nullable: false),
                    VaryantAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MevcutMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokVaryantlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokVaryantlar_StokKartlari_StokKartiId",
                        column: x => x.StokKartiId,
                        principalTable: "StokKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_StokVaryantId",
                table: "StokHareketler",
                column: "StokVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_StokVaryantId",
                table: "OrderMaterials",
                column: "StokVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_StokVaryantlar_StokKartiId",
                table: "StokVaryantlar",
                column: "StokKartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMaterials_StokVaryantlar_StokVaryantId",
                table: "OrderMaterials",
                column: "StokVaryantId",
                principalTable: "StokVaryantlar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketler_StokVaryantlar_StokVaryantId",
                table: "StokHareketler",
                column: "StokVaryantId",
                principalTable: "StokVaryantlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderMaterials_StokVaryantlar_StokVaryantId",
                table: "OrderMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_StokVaryantlar_StokVaryantId",
                table: "StokHareketler");

            migrationBuilder.DropTable(
                name: "StokVaryantlar");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketler_StokVaryantId",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_OrderMaterials_StokVaryantId",
                table: "OrderMaterials");



            migrationBuilder.DropColumn(
                name: "StokVaryantId",
                table: "StokHareketler");

            migrationBuilder.DropColumn(
                name: "StokVaryantId",
                table: "OrderMaterials");
        }
    }
}
