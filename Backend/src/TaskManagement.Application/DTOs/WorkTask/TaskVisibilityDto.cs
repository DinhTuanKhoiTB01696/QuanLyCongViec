using System.Collections.Generic;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class TaskVisibilityDto
    {
        public string Mode { get; set; } = "project";
        public List<string> Roles { get; set; } = new();
    }
}
