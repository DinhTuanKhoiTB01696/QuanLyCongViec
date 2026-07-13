using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTerraAiUsagePricingAndContingency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverAltText",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiCreditRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EstimatedCredits = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Disclaimer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiCreditRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiPricingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MonthlyPriceVnd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerUser = table.Column<bool>(type: "bit", nullable: false),
                    IncludedUsers = table.Column<int>(type: "int", nullable: true),
                    IncludedAiCredits = table.Column<int>(type: "int", nullable: false),
                    ExtraAiCreditsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PricingStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPricingPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiUsageLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditsConsumed = table.Column<int>(type: "int", nullable: false),
                    ProviderTokens = table.Column<long>(type: "bigint", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsageLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiUsageLedgerEntries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiUsageLedgerEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AiUsageLedgerEntries_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskContingencyPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Risk = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplacementDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImpactLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TriggerCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskContingencyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskContingencyPlans_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskContingencyPlans_Users_SupportPersonId",
                        column: x => x.SupportPersonId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskContingencyPlans_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskContingencyPlans_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiCreditRules_ActionType",
                table: "AiCreditRules",
                column: "ActionType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPricingPlans_Code",
                table: "AiPricingPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageLedgerEntries_IdempotencyKey",
                table: "AiUsageLedgerEntries",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageLedgerEntries_ProjectId",
                table: "AiUsageLedgerEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageLedgerEntries_UserId_OccurredAt",
                table: "AiUsageLedgerEntries",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageLedgerEntries_WorkspaceId_OccurredAt",
                table: "AiUsageLedgerEntries",
                columns: new[] { "WorkspaceId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_Category",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskContingencyPlans_CreatedById",
                table: "TaskContingencyPlans",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskContingencyPlans_SupportPersonId",
                table: "TaskContingencyPlans",
                column: "SupportPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskContingencyPlans_UpdatedById",
                table: "TaskContingencyPlans",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskContingencyPlans_WorkTaskId_Status",
                table: "TaskContingencyPlans",
                columns: new[] { "WorkTaskId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiCreditRules");

            migrationBuilder.DropTable(
                name: "AiPricingPlans");

            migrationBuilder.DropTable(
                name: "AiUsageLedgerEntries");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "TaskContingencyPlans");

            migrationBuilder.DropColumn(
                name: "CoverAltText",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Projects");
        }
    }
}
