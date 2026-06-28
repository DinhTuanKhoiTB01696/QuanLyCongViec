using System;

namespace TaskManagement.Domain.Entities
{
    public class RecentView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
