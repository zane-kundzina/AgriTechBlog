using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.Data.Migrations
{
    [ExcludeFromCodeCoverage]

    public partial class AddingcolumnstoAppUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AboutContent",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubHeader",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutContent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubHeader",
                table: "AspNetUsers");
        }
    }
}
