using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.Entities
{
    public class WorkTask
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid? SprintId { get; set; }
        public Sprint? Sprint { get; set; }
        public Guid? ParentTaskId { get; set; }
        public WorkTask? ParentTask { get; set; }
        public Guid TaskTypeId { get; set; }
        public TaskType TaskType { get; set; } = null!;
        public Guid TaskStatusId { get; set; }
        public TaskStatus TaskStatus { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public double StoryPoints { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = null!;
        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public double TotalEstimatedHours { get; set; }
        public double TotalActualHours { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();

        // === Plane-inspired additions ===

        /// <summary>
        /// SortOrder (Double) — hỗ trợ thuật toán LexoRank cho kéo thả Kanban.
        /// Giá trị được tính trung bình giữa 2 phần tử lân cận khi kéo thả.
        /// </summary>
        public double SortOrder { get; set; }

        /// <summary>Issue ID hiển thị, VD: "CUN-42". Generated tự động.</summary>
        public string? SequenceId { get; set; }

        /// <summary>Workspace ID for tenant isolation (denormalized from Project)</summary>
        public Guid WorkspaceId { get; set; }

        // Navigation properties
        public ICollection<WorkTask> ChildTasks { get; set; } = new List<WorkTask>();
        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskDependency> PredecessorDependencies { get; set; } = new List<TaskDependency>();
        public ICollection<TaskDependency> SuccessorDependencies { get; set; } = new List<TaskDependency>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public TaskVectorEmbedding? TaskVectorEmbedding { get; set; }
        public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();
        public ICollection<IssueModule> IssueModules { get; set; } = new List<IssueModule>();
        public ICollection<TaskSubscriber> Subscribers { get; set; } = new List<TaskSubscriber>();
    }
}
