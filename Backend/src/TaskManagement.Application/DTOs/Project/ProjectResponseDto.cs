namespace TaskManagement.Application.DTOs.Project
{
    public class ProjectResponseDto
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
    }
}
