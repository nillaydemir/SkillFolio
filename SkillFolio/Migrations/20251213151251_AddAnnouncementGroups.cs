using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillFolio.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnouncementGroupId",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AnnouncementGroups",
                columns: table => new
                {
                    AnnouncementGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GroupType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementGroups", x => x.AnnouncementGroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AnnouncementGroupId",
                table: "Announcements",
                column: "AnnouncementGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_AnnouncementGroups_AnnouncementGroupId",
                table: "Announcements",
                column: "AnnouncementGroupId",
                principalTable: "AnnouncementGroups",
                principalColumn: "AnnouncementGroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_AnnouncementGroups_AnnouncementGroupId",
                table: "Announcements");

            migrationBuilder.DropTable(
                name: "AnnouncementGroups");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_AnnouncementGroupId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "AnnouncementGroupId",
                table: "Announcements");
        }
    }
}
