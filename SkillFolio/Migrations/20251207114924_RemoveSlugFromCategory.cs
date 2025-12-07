using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillFolio.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlugFromCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "EventCategories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "EventCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
