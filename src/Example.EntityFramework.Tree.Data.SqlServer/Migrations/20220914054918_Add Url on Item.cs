using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.EntityFramework.Tree.Data.SqlServer.Migrations
{
    public partial class AddUrlonItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Item",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Item");
        }
    }
}
