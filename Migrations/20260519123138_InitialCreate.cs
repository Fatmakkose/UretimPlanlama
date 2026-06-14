using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComponentUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FabricPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FabricStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FabricSupplier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoodsDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InspectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductionPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmountWithVat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
