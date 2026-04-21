using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    public partial class TaskDraftProjectIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "TaskDrafts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE td
SET ProjectId = TRY_CONVERT(uniqueidentifier, JSON_VALUE(td.PayloadJson, '$.projectId'))
FROM TaskDrafts td
WHERE td.ProjectId IS NULL
  AND ISJSON(td.PayloadJson) = 1
  AND JSON_VALUE(td.PayloadJson, '$.projectId') IS NOT NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDrafts_UserId_ProjectId_UpdatedAt",
                table: "TaskDrafts",
                columns: new[] { "UserId", "ProjectId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDrafts_UserId_UpdatedAt",
                table: "TaskDrafts",
                columns: new[] { "UserId", "UpdatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskDrafts_UserId_ProjectId_UpdatedAt",
                table: "TaskDrafts");

            migrationBuilder.DropIndex(
                name: "IX_TaskDrafts_UserId_UpdatedAt",
                table: "TaskDrafts");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TaskDrafts");
        }
    }
}
