using System;
using System.Collections.Generic;

namespace TaskManagement.Application.DTOs.Project
{
    public class ProjectRewardRulesDto
    {
        public double BasePointMultiplier { get; set; } = 1;
        public double EarlyBonusPercent { get; set; } = 10;
        public double AccuracyBonusPercent { get; set; } = 5;
        public double LatePenaltyPercent { get; set; } = 10;
        public int CollaborationBonusPoints { get; set; } = 2;
        public int ManualAdjustmentLimit { get; set; } = 200;
    }

    public class ProjectCapacityRulesDto
    {
        public double DefaultWeeklyHours { get; set; } = 40;
        public double NearLimitPercent { get; set; } = 80;
        public double OverLimitPercent { get; set; } = 100;
        public int MaxActiveTasksPerMember { get; set; } = 8;
        public Dictionary<string, double> RoleWeeklyHours { get; set; } = new();
        public Dictionary<string, int> RoleActiveTaskLimits { get; set; } = new();
    }

    public class ProjectBaselineSettingsDto
    {
        public bool UsePlanningBaseline { get; set; } = true;
        public double DefaultBaseHours { get; set; } = 4;
        public double HoursPerStoryPoint { get; set; } = 2;
        public double MinimumSuggestedHours { get; set; } = 0.5;
        public double MaximumSuggestedHours { get; set; } = 80;
        public Dictionary<string, double> RoleHourMultipliers { get; set; } = new();
        public Dictionary<string, double> RoleBaseHours { get; set; } = new();
    }

    public class ProjectMilestoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ReleaseVersion { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = "Planned";
        public bool IsArchived { get; set; }
        public List<Guid> LinkedSprintIds { get; set; } = new();
    }

    public class ProjectPointAdjustmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = "Manual";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
