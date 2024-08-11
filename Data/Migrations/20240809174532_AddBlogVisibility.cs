using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTales.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Visibility",
                table: "Blogs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Blogs");
        }
    }
}
