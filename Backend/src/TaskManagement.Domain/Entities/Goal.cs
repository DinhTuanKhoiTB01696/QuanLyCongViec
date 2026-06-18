using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class Goal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "On Track"; // On Track, At Risk, Off Track, Completed
        public DateTime? StartDate { get; set; } // Ngày bắt đầu
        public DateTime? DueDate { get; set; } // Ngày kết thúc
        public int Progress { get; set; } = 0; // 0-100%
        public bool IsArchived { get; set; } = false;
        
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public Guid? DepartmentId { get; set; } // Liên kết với Team (Department)
        public Department? Department { get; set; }

        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public Guid? ParentGoalId { get; set; }
        public Goal? ParentGoal { get; set; }
        public ICollection<Goal> SubGoals { get; set; } = new List<Goal>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<GoalUpdate> Updates { get; set; } = new List<GoalUpdate>();
        public ICollection<GoalLesson> Lessons { get; set; } = new List<GoalLesson>();
        public ICollection<GoalRisk> Risks { get; set; } = new List<GoalRisk>();
        public ICollection<GoalDecision> Decisions { get; set; } = new List<GoalDecision>();
        
        // Link to Projects will be handled by ProjectLink entity to support polymorphic links
    }
}
