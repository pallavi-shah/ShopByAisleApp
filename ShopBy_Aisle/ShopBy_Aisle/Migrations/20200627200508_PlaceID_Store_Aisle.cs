using Microsoft.EntityFrameworkCore.Migrations;

namespace ShopBy_Aisle.Migrations
{
    public partial class PlaceID_Store_Aisle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceID",
                table: "Stores",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "MasterItems",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "PlaceID",
                table: "Aisles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceID",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "PlaceID",
                table: "Aisles");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "MasterItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
