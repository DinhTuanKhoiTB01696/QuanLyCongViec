namespace TaskManagement.Domain.Entities;

/// <summary>Backlog schema for per-user notification delivery preferences.</summary>
public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User User { get; set; } = null!;
}
