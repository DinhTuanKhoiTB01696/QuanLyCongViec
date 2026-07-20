using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFloatingStickiesMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFloating",
                table: "StickyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PositionX",
                table: "StickyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionY",
                table: "StickyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_UserId_IsFloating",
                table: "StickyNotes",
                columns: new[] { "UserId", "IsFloating" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_UserId_IsFloating",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "IsFloating",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "StickyNotes");
        }
    }
}
