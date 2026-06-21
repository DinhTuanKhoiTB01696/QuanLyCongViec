using System;

namespace TaskManagement.Domain.Entities
{
    public class GoalUpdateAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GoalUpdateId { get; set; }
        public GoalUpdate GoalUpdate { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
