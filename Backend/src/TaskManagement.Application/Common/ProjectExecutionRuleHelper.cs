using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Common
{
    public static class ProjectExecutionRuleHelper
    {
        private static readonly Dictionary<string, double> DefaultRoleHourMultipliers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pm"] = 0.9,
            ["po"] = 0.8,
            ["sm"] = 0.8,
            ["project_manager"] = 1.0,
            ["project_lead"] = 1.05,
            ["scrum_master"] = 0.8,
            ["techlead"] = 1.1,
            ["dev"] = 1.0,
            ["developer"] = 1.0,
            ["qa"] = 0.75,
            ["designer"] = 0.9,
            ["devops"] = 1.15,
            ["guest"] = 0.5,
            ["stakeholder"] = 0.5
        };

        public static string NormalizeProjectRole(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value
                .Trim()
                .Replace(" ", "_", StringComparison.Ordinal)
                .Replace("-", "_", StringComparison.Ordinal)
                .ToLowerInvariant();

            return normalized switch
            {
                "project_manager" => "pm",
                "product_owner" => "po",
                "scrum_master" => "sm",
                "dev" => "developer",
                "tester" => "qa",
                "project_admin" => "admin",
                _ => normalized
            };
        }

        public static string NormalizeVisibilityMode(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "assigned" or "private" => "assigned",
                "role" or "roles" or "role_scoped" => "role",
                _ => "project"
            };
        }

        public static TaskVisibilityDto NormalizeTaskVisibility(TaskVisibilityDto? input)
        {
            var result = new TaskVisibilityDto
            {
                Mode = NormalizeVisibilityMode(input?.Mode)
            };

            result.Roles = (input?.Roles ?? new List<string>())
                .Select(NormalizeProjectRole)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (result.Mode != "role")
            {
                result.Roles = new List<string>();
            }

            return result;
        }

        public static string BuildTaskVisibilityPayload(TaskVisibilityDto visibility)
        {
            var normalized = NormalizeTaskVisibility(visibility);
            return JsonSerializer.Serialize(normalized);
        }

        public static TaskVisibilityDto ParseTaskVisibility(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return NormalizeTaskVisibility(new TaskVisibilityDto());
            }

            try
            {
                return NormalizeTaskVisibility(JsonSerializer.Deserialize<TaskVisibilityDto>(raw));
            }
            catch
            {
                return NormalizeTaskVisibility(new TaskVisibilityDto());
            }
        }

        public static ProjectExecutionRulesDto NormalizeExecutionRules(ProjectExecutionRulesDto? input)
        {
            var result = new ProjectExecutionRulesDto
            {
                EnableRoleBasedTaskVisibility = input?.EnableRoleBasedTaskVisibility ?? false,
                ManagerAlwaysSeeAllTasks = input?.ManagerAlwaysSeeAllTasks ?? true,
                DefaultTaskVisibilityMode = NormalizeVisibilityMode(input?.DefaultTaskVisibilityMode),
                DefaultBaseHours = Clamp(input?.DefaultBaseHours ?? 4, 1, 40),
                HoursPerStoryPoint = Clamp(input?.HoursPerStoryPoint ?? 2, 0.5, 12),
                EstimateBaselineMode = string.IsNullOrWhiteSpace(input?.EstimateBaselineMode) ? "role_then_project" : input!.EstimateBaselineMode.Trim().ToLowerInvariant(),
                RoleHourMultipliers = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            };

            foreach (var pair in DefaultRoleHourMultipliers)
            {
                result.RoleHourMultipliers[pair.Key] = pair.Value;
            }

            if (input?.RoleHourMultipliers != null)
            {
                foreach (var pair in input.RoleHourMultipliers)
                {
                    var key = NormalizeProjectRole(pair.Key);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    result.RoleHourMultipliers[key] = Clamp(pair.Value, 0.3, 3);
                }
            }

            return result;
        }

        public static double CalculateSuggestedEstimateHours(
            ProjectExecutionRulesDto? rules,
            string? projectRole,
            double storyPoints,
            int priority,
            int assigneeCount,
            int subtaskCount,
            string? title)
        {
            var normalizedRules = NormalizeExecutionRules(rules);
            var normalizedRole = NormalizeProjectRole(projectRole);
            var normalizedStoryPoints = Math.Max(0, storyPoints);

            var hours = normalizedStoryPoints > 0
                ? normalizedStoryPoints * normalizedRules.HoursPerStoryPoint
                : normalizedRules.DefaultBaseHours;

            if (priority == 1) hours += 4;
            else if (priority == 2) hours += 2;

            if (assigneeCount > 1) hours += Math.Min(assigneeCount - 1, 3) * 0.5;
            if (subtaskCount > 0) hours += Math.Min(subtaskCount, 6) * 0.4;

            var titleText = (title ?? string.Empty).ToLowerInvariant();
            if (titleText.Contains("api") || titleText.Contains("integration") || titleText.Contains("migration") || titleText.Contains("security"))
            {
                hours += 2.5;
            }
            if (titleText.Contains("bug") || titleText.Contains("fix") || titleText.Contains("hotfix"))
            {
                hours += 1;
            }

            if (!string.IsNullOrWhiteSpace(normalizedRole) &&
                normalizedRules.RoleHourMultipliers.TryGetValue(normalizedRole, out var multiplier))
            {
                hours *= multiplier;
            }

            return Math.Round(Clamp(hours, 0.5, 80), 1);
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}
