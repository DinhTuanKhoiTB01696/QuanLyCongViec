using System;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class ContingencyPlanDto
    {
        public Guid Id { get; set; }
        public Guid WorkTaskId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string? RiskDescription { get; set; }
        public string? Notes { get; set; }
        
        public List<ContingencyTaskDto> ContingencyTasks { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ContingencyTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public Guid? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public bool IsActivated { get; set; }
    }

    public class CreateContingencyTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; } = 3;
        public Guid? AssigneeId { get; set; }
    }
}
