using System;
using System.Collections.Generic;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class UpdateWorkTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public double? StoryPoints { get; set; }
        public Guid? AssignedUserId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? SprintId { get; set; }
        public Guid TaskTypeId { get; set; }
        public double? TotalEstimatedHours { get; set; }
        public string? VisibilityMode { get; set; }
        public List<string>? VisibleToRoles { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}
