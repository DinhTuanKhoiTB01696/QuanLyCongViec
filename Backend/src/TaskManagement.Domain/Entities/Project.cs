using System;
using System.Collections.Generic;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Mã định danh dự án, VD: "CUN", "PRJ". Dùng cho Issue ID (CUN-1, CUN-2)</summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>Bộ đếm Issue tự tăng trong project (CUN-1, CUN-2,...)</summary>
        public int IssueSequence { get; set; } = 0;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Overview Info
        public string? Why { get; set; }
        public string? SuccessCriteria { get; set; }
        public DateTime? CloseDate { get; set; }
        public string? TrackedLinkUrl { get; set; }

        public bool Status { get; set; } = true;
        public Guid CreatorId { get; set; }
        public User Creator { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // === Multi-Tenant: Workspace ===
        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public Guid? ProjectTemplateId { get; set; }
        public ProjectTemplate? ProjectTemplate { get; set; }
        public string? TemplateType { get; set; }
        public string? NavigationConfig { get; set; }

        /// <summary>Mạng lưới dự án: Public (ai trong workspace cũng thấy) / Private (chỉ members)</summary>
        public string NetworkType { get; set; } = "Public";

        // Navigation properties
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<ProjectDepartmentRole> ProjectDepartmentRoles { get; set; } = new List<ProjectDepartmentRole>();
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();
        public ICollection<TaskStatus> TaskStatuses { get; set; } = new List<TaskStatus>();
        public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
        public ICollection<Label> Labels { get; set; } = new List<Label>();
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<Intake> Intakes { get; set; } = new List<Intake>();
        public ICollection<Page> Pages { get; set; } = new List<Page>();
        public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
        public ICollection<ProjectLesson> Lessons { get; set; } = new List<ProjectLesson>();
        public ICollection<ProjectRisk> Risks { get; set; } = new List<ProjectRisk>();
        public ICollection<ProjectDecision> Decisions { get; set; } = new List<ProjectDecision>();

    }
}
