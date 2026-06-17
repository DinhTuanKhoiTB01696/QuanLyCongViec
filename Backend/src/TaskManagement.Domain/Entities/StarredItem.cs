using System;

namespace TaskManagement.Domain.Entities
{
    public class StarredItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        
        public string ItemType { get; set; } = string.Empty; // "Goal", "Project", "Team", "User"
        public Guid ItemId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
