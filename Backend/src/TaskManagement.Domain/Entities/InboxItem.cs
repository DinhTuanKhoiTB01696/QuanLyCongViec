using System;

namespace TaskManagement.Domain.Entities
{
    public class InboxItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? IntegrationAccountId { get; set; }
        public IntegrationAccount? IntegrationAccount { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Location { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsRead { get; set; }
        public Guid? CreatedTaskId { get; set; }
        public WorkTask? CreatedTask { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
