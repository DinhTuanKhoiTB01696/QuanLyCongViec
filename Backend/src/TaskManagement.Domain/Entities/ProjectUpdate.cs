using System;

namespace TaskManagement.Domain.Entities
{
    public class ProjectUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        
        public Guid CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
