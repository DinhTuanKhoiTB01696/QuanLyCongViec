using System;

namespace TaskManagement.Domain.Entities
{
    public class SiteAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty; // "Team", "Goal", "Project", "Profile"

        public string Action { get; set; } = string.Empty; // e.g. "Create", "Update", "StatusChange", "AddMember"
        
        public string? OldValue { get; set; } // JSON or text
        public string? NewValue { get; set; } // JSON or text

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
