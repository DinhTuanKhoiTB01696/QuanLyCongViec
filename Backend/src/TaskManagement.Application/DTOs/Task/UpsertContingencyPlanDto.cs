using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Task
{
    public class UpsertContingencyPlanDto
    {
        [Required]
        [MaxLength(50)]
        public string RiskLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RiskStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ActivationCondition { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string RiskDescription { get; set; } = string.Empty;

        public string? RecoveryPlan { get; set; }

        [MaxLength(500)]
        public string? ExpectedResult { get; set; }

        public Guid? BackupOwnerId { get; set; }
        
        public DateTime? FallbackDeadline { get; set; }
    }
}
