using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillFolio.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedAndRegisteredAtToUserEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "UserEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAt",
                table: "UserEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "UserEvents");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "UserEvents");
        }
    }
}
