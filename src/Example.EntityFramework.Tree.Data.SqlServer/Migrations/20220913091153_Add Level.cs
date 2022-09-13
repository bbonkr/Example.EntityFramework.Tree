using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.EntityFramework.Tree.Data.SqlServer.Migrations
{
    public partial class AddLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Item",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Item");
        }
    }
}
