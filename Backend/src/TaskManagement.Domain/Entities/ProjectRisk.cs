using System;

namespace TaskManagement.Domain.Entities
{
    public class ProjectRisk
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public Guid CreatorId { get; set; }
        public User Creator { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
