using System;

namespace TaskManagement.Application.DTOs.Project
{
    public class ProjectMemberDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProjectRole { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
