using System;

namespace TaskManagement.Domain.Entities
{
    public static class StarredItemTypes
    {
        public const string Goal = "Goal";
        public const string Project = "Project";
        public const string Team = "Team";
        public const string User = "User";

        public static readonly string[] Allowed = [Goal, Project, Team, User];
    }

    public class StarredItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        
        public string ItemType { get; set; } = string.Empty;
        public Guid ItemId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
