using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTaskFields_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "WorkTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "WorkTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkTasks",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_AssignedUserId",
                table: "WorkTasks",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_Users_AssignedUserId",
                table: "WorkTasks",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_Users_AssignedUserId",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_AssignedUserId",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkTasks");
        }
    }
}
