namespace TaskManagement.Domain.Entities;

public class AiUsageLedger
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public int CreditsConsumed { get; set; }
    public long? ProviderTokens { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime OccurredAt { get; set; }
    public User User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public Project? Project { get; set; }
}
