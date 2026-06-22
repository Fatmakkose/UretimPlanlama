using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UretimPlanlama.Migrations
{
    /// <inheritdoc />
    public partial class AddInterliningFields3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollarInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CuffInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlacketInterliningMeterage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PocketFlapInterliningMeterage",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "BossAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BossTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KapakAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KapakTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KusakAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KusakTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MansetAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MansetTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YakaAstarGram",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YakaTelaRenk",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BossAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BossTelaRenk",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KapakAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KapakTelaRenk",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KusakAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "KusakTelaRenk",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MansetAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MansetTelaRenk",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "YakaAstarGram",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "YakaTelaRenk",
                table: "Orders");

            migrationBuilder.AddColumn<decimal>(
                name: "CollarInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuffInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlacketInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PocketFlapInterliningMeterage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
