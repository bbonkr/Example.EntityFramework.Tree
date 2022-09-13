using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.EntityFramework.Tree.Data.SqlServer.Migrations
{
    public partial class AddLanguageCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "Item",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Item",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Item");

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "Item",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }
    }
}
