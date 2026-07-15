using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Domain.Entities
{
    public class ContingencyPlan
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid WorkTaskId { get; set; }
        public WorkTask WorkTask { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical

        [MaxLength(500)]
        public string? RiskDescription { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public ICollection<ContingencyPlanTask> ContingencyPlanTasks { get; set; } = new List<ContingencyPlanTask>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
