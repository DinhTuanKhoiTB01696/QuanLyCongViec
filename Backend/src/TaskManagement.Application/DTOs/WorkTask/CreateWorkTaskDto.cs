using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class CreateWorkTaskDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid ProjectId { get; set; }

        public Guid? SprintId { get; set; }
        
        public Guid? ParentTaskId { get; set; }
        
        // Support both Guid-based and Name-based lookup
        public Guid? TaskTypeId { get; set; }
        public string? TypeName { get; set; }

        public string? StatusName { get; set; }

        public int Priority { get; set; } = 3;
        public double StoryPoints { get; set; }

        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid? AssignedUserId { get; set; }
        public List<Guid>? AssigneeIds { get; set; }
        public Guid? ModuleId { get; set; }
        public List<Guid>? LabelIds { get; set; }
        public double TotalEstimatedHours { get; set; }
        public string? VisibilityMode { get; set; }
        public List<string>? VisibleToRoles { get; set; }
    }
}
