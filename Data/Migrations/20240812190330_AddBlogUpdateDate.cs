using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTales.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogUpdateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogEntityCategoryEntity_Categories_CatogoriesId",
                table: "BlogEntityCategoryEntity");

            migrationBuilder.RenameColumn(
                name: "CatogoriesId",
                table: "BlogEntityCategoryEntity",
                newName: "CategoriesId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogEntityCategoryEntity_CatogoriesId",
                table: "BlogEntityCategoryEntity",
                newName: "IX_BlogEntityCategoryEntity_CategoriesId");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Blogs",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_BlogEntityCategoryEntity_Categories_CategoriesId",
                table: "BlogEntityCategoryEntity",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogEntityCategoryEntity_Categories_CategoriesId",
                table: "BlogEntityCategoryEntity");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Blogs");

            migrationBuilder.RenameColumn(
                name: "CategoriesId",
                table: "BlogEntityCategoryEntity",
                newName: "CatogoriesId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogEntityCategoryEntity_CategoriesId",
                table: "BlogEntityCategoryEntity",
                newName: "IX_BlogEntityCategoryEntity_CatogoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogEntityCategoryEntity_Categories_CatogoriesId",
                table: "BlogEntityCategoryEntity",
                column: "CatogoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
