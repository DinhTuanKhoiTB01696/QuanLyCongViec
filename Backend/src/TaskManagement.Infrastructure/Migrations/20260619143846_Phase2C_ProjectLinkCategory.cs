using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2C_ProjectLinkCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectLinks_ProjectId",
                table: "ProjectLinks");

            migrationBuilder.AlterColumn<string>(
                name: "LinkedType",
                table: "ProjectLinks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "LinkCategory",
                table: "ProjectLinks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "GoalUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkTaskId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "GoalId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GoalUpdateId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GoalUpdateAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalUpdateAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalUpdateAttachments_GoalUpdates_GoalUpdateId",
                        column: x => x.GoalUpdateId,
                        principalTable: "GoalUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalUpdateAttachments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalUpdateReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalUpdateReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalUpdateReactions_GoalUpdates_GoalUpdateId",
                        column: x => x.GoalUpdateId,
                        principalTable: "GoalUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalUpdateReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLinks_ProjectId_LinkedType_LinkedId_LinkCategory",
                table: "ProjectLinks",
                columns: new[] { "ProjectId", "LinkedType", "LinkedId", "LinkCategory" },
                unique: true,
                filter: "[LinkedId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_GoalId",
                table: "Comments",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_GoalUpdateId",
                table: "Comments",
                column: "GoalUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalUpdateAttachments_GoalUpdateId",
                table: "GoalUpdateAttachments",
                column: "GoalUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalUpdateAttachments_UserId",
                table: "GoalUpdateAttachments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalUpdateReactions_GoalUpdateId_UserId_ReactionType",
                table: "GoalUpdateReactions",
                columns: new[] { "GoalUpdateId", "UserId", "ReactionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalUpdateReactions_UserId",
                table: "GoalUpdateReactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_GoalUpdates_GoalUpdateId",
                table: "Comments",
                column: "GoalUpdateId",
                principalTable: "GoalUpdates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Goals_GoalId",
                table: "Comments",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_GoalUpdates_GoalUpdateId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Goals_GoalId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "GoalUpdateAttachments");

            migrationBuilder.DropTable(
                name: "GoalUpdateReactions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectLinks_ProjectId_LinkedType_LinkedId_LinkCategory",
                table: "ProjectLinks");

            migrationBuilder.DropIndex(
                name: "IX_Comments_GoalId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_GoalUpdateId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LinkCategory",
                table: "ProjectLinks");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "GoalUpdates");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "GoalUpdateId",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "LinkedType",
                table: "ProjectLinks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkTaskId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLinks_ProjectId",
                table: "ProjectLinks",
                column: "ProjectId");
        }
    }
}
