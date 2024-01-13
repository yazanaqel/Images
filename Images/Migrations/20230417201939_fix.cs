using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Images.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullScreenlContent",
                table: "ImagesData",
                newName: "FullScreenContent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullScreenContent",
                table: "ImagesData",
                newName: "FullScreenlContent");
        }
    }
}
