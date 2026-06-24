namespace TaskManagement.Application.DTOs.Project
{
    /// <summary>
    /// DTO for project discovery: shows all projects with membership flag.
    /// Used by Dashboard to differentiate "my projects" vs "joinable projects".
    /// </summary>
    public class ProjectDiscoveryDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Key { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Status { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int ActiveMemberCount { get; set; }
        public string? NetworkType { get; set; }
        public string? Cover { get; set; }
        public string? Icon { get; set; }
        public Guid? LeadUserId { get; set; }
        public string? LeadName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// True if the current user is an active member of this project.
        /// </summary>
        public bool IsMember { get; set; }

        /// <summary>
        /// The user's role in this project, null if not a member.
        /// </summary>
        public string? MyRole { get; set; }
    }
}
