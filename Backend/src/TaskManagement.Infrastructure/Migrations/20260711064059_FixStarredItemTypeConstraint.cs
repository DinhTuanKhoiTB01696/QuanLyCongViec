using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStarredItemTypeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [StarredItems] WHERE [ItemType] NOT IN ('Goal', 'Project', 'Team', 'User');");

            migrationBuilder.AlterColumn<string>(
                name: "ItemType",
                table: "StarredItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StarredItems_UserId_WorkspaceId_ItemType_ItemId",
                table: "StarredItems",
                columns: new[] { "UserId", "WorkspaceId", "ItemType", "ItemId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StarredItems_ItemType",
                table: "StarredItems",
                sql: "[ItemType] IN ('Goal', 'Project', 'Team', 'User')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StarredItems_UserId_WorkspaceId_ItemType_ItemId",
                table: "StarredItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StarredItems_ItemType",
                table: "StarredItems");

            migrationBuilder.AlterColumn<string>(
                name: "ItemType",
                table: "StarredItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
