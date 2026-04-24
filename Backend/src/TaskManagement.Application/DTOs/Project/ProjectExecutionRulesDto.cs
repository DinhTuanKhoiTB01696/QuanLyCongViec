using System.Collections.Generic;

namespace TaskManagement.Application.DTOs.Project
{
    public class ProjectExecutionRulesDto
    {
        public bool EnableRoleBasedTaskVisibility { get; set; }
        public bool ManagerAlwaysSeeAllTasks { get; set; } = true;
        public string DefaultTaskVisibilityMode { get; set; } = "project";
        public double DefaultBaseHours { get; set; } = 4;
        public double HoursPerStoryPoint { get; set; } = 2;
        public string EstimateBaselineMode { get; set; } = "role_then_project";
        public Dictionary<string, double> RoleHourMultipliers { get; set; } = new();
    }
}
