using System;

namespace TaskManagement.Domain.Entities
{
    public class ProjectUpdateReaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectUpdateId { get; set; }
        public ProjectUpdate ProjectUpdate { get; set; } = null!;
        public string ReactionType { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
