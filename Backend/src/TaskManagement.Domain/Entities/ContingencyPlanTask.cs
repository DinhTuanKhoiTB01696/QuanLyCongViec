using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Domain.Entities
{
    public class ContingencyPlanTask
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ContingencyPlanId { get; set; }
        public ContingencyPlan ContingencyPlan { get; set; } = null!;

        public Guid? WorkTaskId { get; set; }
        public WorkTask? WorkTask { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public int Priority { get; set; } = 3;
        
        public Guid? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public string StatusName { get; set; } = "Đang chờ xử lý";

        public bool IsActivated { get; set; } = false;
        
        public Guid? ActivatedById { get; set; }
        public User? ActivatedBy { get; set; }
        
        public DateTime? ActivatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
