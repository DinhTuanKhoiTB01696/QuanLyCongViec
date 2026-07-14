namespace TaskManagement.Domain.Entities;

public class TaskContingencyPlan
{
    public Guid Id { get; set; }
    public Guid WorkTaskId { get; set; }
    public string Risk { get; set; } = string.Empty;
    public string? Cause { get; set; }
    public string ResponsePlan { get; set; } = string.Empty;
    public Guid? SupportPersonId { get; set; }
    public DateTime? ReplacementDeadline { get; set; }
    public string ImpactLevel { get; set; } = "Medium";
    public string? TriggerCondition { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public Guid UpdatedById { get; set; }
    public WorkTask WorkTask { get; set; } = null!;
    public User? SupportPerson { get; set; }
    public User CreatedBy { get; set; } = null!;
    public User UpdatedBy { get; set; } = null!;
}
