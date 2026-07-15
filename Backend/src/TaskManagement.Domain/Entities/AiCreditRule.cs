namespace TaskManagement.Domain.Entities;

public class AiCreditRule
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public int EstimatedCredits { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Disclaimer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
