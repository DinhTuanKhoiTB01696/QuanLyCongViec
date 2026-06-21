using System;

namespace TaskManagement.Domain.Entities
{
    public class ProjectLink
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string LinkedType { get; set; } = string.Empty; // "Goal", "Project", "Task", "TrackedLink"
        public Guid? LinkedId { get; set; } // Nullable if it's just a web link
        public string? TrackedUrl { get; set; } // For external tracked links
        public string LinkCategory { get; set; } = "spaceProject"; // "spaceProject" or "siteProject"
        
        public Guid CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
