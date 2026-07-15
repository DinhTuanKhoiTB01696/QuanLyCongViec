using System;

namespace TaskManagement.Application.DTOs.Task
{
    public class ContingencyPlanDto
    {
        public Guid Id { get; set; }
        public Guid WorkTaskId { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RiskStatus { get; set; } = string.Empty;
        public string ActivationCondition { get; set; } = string.Empty;
        public string RiskDescription { get; set; } = string.Empty;
        public string? RecoveryPlan { get; set; }
        public string? ExpectedResult { get; set; }
        public Guid? BackupOwnerId { get; set; }
        public string? BackupOwnerName { get; set; }
        public string? BackupOwnerAvatar { get; set; }
        public DateTime? FallbackDeadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
