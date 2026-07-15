using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContingencyTaskSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "WorkTaskId",
                table: "ContingencyPlanTasks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeId",
                table: "ContingencyPlanTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ContingencyPlanTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ContingencyPlanTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "ContingencyPlanTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ContingencyPlanTasks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlanTasks_AssigneeId",
                table: "ContingencyPlanTasks",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContingencyPlanTasks_Users_AssigneeId",
                table: "ContingencyPlanTasks",
                column: "AssigneeId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContingencyPlanTasks_Users_AssigneeId",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropIndex(
                name: "IX_ContingencyPlanTasks_AssigneeId",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "ContingencyPlanTasks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ContingencyPlanTasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkTaskId",
                table: "ContingencyPlanTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
