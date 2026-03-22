using System;

namespace TaskManagement.Application.DTOs.Project
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
}
