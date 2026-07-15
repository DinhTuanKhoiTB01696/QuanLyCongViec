using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContingencyPlanSchemaV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContingencyPlans_Users_ActivatedById",
                table: "ContingencyPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_ContingencyPlans_WorkTasks_ContingencyTaskId",
                table: "ContingencyPlans");

            migrationBuilder.DropIndex(
                name: "IX_ContingencyPlans_ActivatedById",
                table: "ContingencyPlans");

            migrationBuilder.DropIndex(
                name: "IX_ContingencyPlans_ContingencyTaskId",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "ActivatedById",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "ActivationCondition",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "ContingencyTaskId",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "IsActivated",
                table: "ContingencyPlans");

            migrationBuilder.DropColumn(
                name: "RiskStatus",
                table: "ContingencyPlans");

            migrationBuilder.CreateTable(
                name: "ContingencyPlanTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContingencyPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContingencyPlanTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContingencyPlanTasks_ContingencyPlans_ContingencyPlanId",
                        column: x => x.ContingencyPlanId,
                        principalTable: "ContingencyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContingencyPlanTasks_Users_ActivatedById",
                        column: x => x.ActivatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContingencyPlanTasks_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlanTasks_ActivatedById",
                table: "ContingencyPlanTasks",
                column: "ActivatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlanTasks_ContingencyPlanId",
                table: "ContingencyPlanTasks",
                column: "ContingencyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlanTasks_WorkTaskId",
                table: "ContingencyPlanTasks",
                column: "WorkTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContingencyPlanTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "ContingencyPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivatedById",
                table: "ContingencyPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationCondition",
                table: "ContingencyPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ContingencyTaskId",
                table: "ContingencyPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                table: "ContingencyPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RiskStatus",
                table: "ContingencyPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlans_ActivatedById",
                table: "ContingencyPlans",
                column: "ActivatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContingencyPlans_ContingencyTaskId",
                table: "ContingencyPlans",
                column: "ContingencyTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContingencyPlans_Users_ActivatedById",
                table: "ContingencyPlans",
                column: "ActivatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContingencyPlans_WorkTasks_ContingencyTaskId",
                table: "ContingencyPlans",
                column: "ContingencyTaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
