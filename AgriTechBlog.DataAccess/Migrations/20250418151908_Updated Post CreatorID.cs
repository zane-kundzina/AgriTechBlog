using Microsoft.EntityFrameworkCore.Migrations;

namespace AgriTechBlog.Data.Migrations
{
    public partial class UpdatedPostCreatorID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_CreatorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatorIr",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Posts",
                newName: "CreatorID");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts",
                newName: "IX_Posts_CreatorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_CreatorID",
                table: "Posts",
                column: "CreatorID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_CreatorID",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "CreatorID",
                table: "Posts",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_CreatorID",
                table: "Posts",
                newName: "IX_Posts_CreatorId");

            migrationBuilder.AddColumn<string>(
                name: "CreatorIr",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
