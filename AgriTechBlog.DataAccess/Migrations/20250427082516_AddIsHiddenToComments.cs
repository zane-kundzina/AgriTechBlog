using Microsoft.EntityFrameworkCore.Migrations;

namespace AgriTechBlog.Data.Migrations
{
    public partial class AddIsHiddenToComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Comments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Comments");
        }
    }
}
