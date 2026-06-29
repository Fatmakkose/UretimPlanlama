using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddCariAndStokModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CariHesaplar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HesapKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HesapAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HesapTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VergiDairesi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VergiNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHesaplar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokKartlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StokAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MevcutMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Depo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tedarikci = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokKartlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CariHareketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CariHesapId = table.Column<int>(type: "int", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    BelgeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariHareketler_CariHesaplar_CariHesapId",
                        column: x => x.CariHesapId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CariHareketler_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StokHareketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartiId = table.Column<int>(type: "int", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HareketTipi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    BelgeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokHareketler_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StokHareketler_StokKartlari_StokKartiId",
                        column: x => x.StokKartiId,
                        principalTable: "StokKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariHesapId",
                table: "CariHareketler",
                column: "CariHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_OrderId",
                table: "CariHareketler",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_HesapKodu",
                table: "CariHesaplar",
                column: "HesapKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_OrderId",
                table: "StokHareketler",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_StokKartiId",
                table: "StokHareketler",
                column: "StokKartiId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartlari_StokKodu",
                table: "StokKartlari",
                column: "StokKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CariHareketler");

            migrationBuilder.DropTable(
                name: "StokHareketler");

            migrationBuilder.DropTable(
                name: "CariHesaplar");

            migrationBuilder.DropTable(
                name: "StokKartlari");
        }
    }
}
