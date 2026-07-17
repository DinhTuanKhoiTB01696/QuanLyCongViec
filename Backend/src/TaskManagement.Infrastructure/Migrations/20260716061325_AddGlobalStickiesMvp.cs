using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalStickiesMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_UserId",
                table: "StickyNotes");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "StickyNotes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "GoalId",
                table: "StickyNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StickyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "StickyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "StickyNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRoute",
                table: "StickyNotes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "StickyNotes",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkTaskId",
                table: "StickyNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "StickyNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_GoalId",
                table: "StickyNotes",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_ProjectId",
                table: "StickyNotes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_UserId_IsDeleted_IsPinned_UpdatedAt",
                table: "StickyNotes",
                columns: new[] { "UserId", "IsDeleted", "IsPinned", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_WorkspaceId",
                table: "StickyNotes",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_WorkTaskId",
                table: "StickyNotes",
                column: "WorkTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_GoalId",
                table: "StickyNotes");

            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_ProjectId",
                table: "StickyNotes");

            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_UserId_IsDeleted_IsPinned_UpdatedAt",
                table: "StickyNotes");

            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_WorkspaceId",
                table: "StickyNotes");

            migrationBuilder.DropIndex(
                name: "IX_StickyNotes_WorkTaskId",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "SourceRoute",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "WorkTaskId",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "StickyNotes");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "StickyNotes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_UserId",
                table: "StickyNotes",
                column: "UserId");
        }
    }
}
