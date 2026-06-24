using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HomeSite_Phase1_Polymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_WorkTasks_WorkTaskId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_WorkTaskId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "WorkTaskId",
                table: "Comments",
                newName: "EntityId");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuccessCriteria",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackedLinkUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Why",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommentMentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentionedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentMentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentMentions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentMentions_Users_MentionedUserId",
                        column: x => x.MentionedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KudoReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KudoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KudoReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KudoReactions_Kudos_KudoId",
                        column: x => x.KudoId,
                        principalTable: "Kudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KudoReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_EntityType_EntityId",
                table: "Comments",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_CommentId",
                table: "CommentMentions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_MentionedUserId",
                table: "CommentMentions",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KudoReactions_KudoId",
                table: "KudoReactions",
                column: "KudoId");

            migrationBuilder.CreateIndex(
                name: "IX_KudoReactions_UserId",
                table: "KudoReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteAuditLogs_EntityType_EntityId",
                table: "SiteAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteAuditLogs_UserId",
                table: "SiteAuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentMentions");

            migrationBuilder.DropTable(
                name: "KudoReactions");

            migrationBuilder.DropTable(
                name: "SiteAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Comments_EntityType_EntityId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SuccessCriteria",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TrackedLinkUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Why",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "Comments",
                newName: "WorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkTaskId",
                table: "Comments",
                column: "WorkTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_WorkTasks_WorkTaskId",
                table: "Comments",
                column: "WorkTaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
