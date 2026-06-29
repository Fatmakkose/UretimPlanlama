using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddTelaTipiAndPat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BossTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KapakTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KusakTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MansetTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YakaTelaTipi",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BossTelaTipi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KapakTelaTipi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KusakTelaTipi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MansetTelaTipi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PatAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PatTelaRenk",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PatTelaTipi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "YakaTelaTipi",
                table: "Orders");
        }
    }
}
