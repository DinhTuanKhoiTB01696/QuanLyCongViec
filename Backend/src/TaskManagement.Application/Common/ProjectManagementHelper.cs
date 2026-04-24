using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TaskManagement.Application.DTOs.Project;

namespace TaskManagement.Application.Common
{
    public static class ProjectManagementHelper
    {
        private static readonly Dictionary<string, double> DefaultRoleWeeklyHours = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pm"] = 40,
            ["po"] = 32,
            ["sm"] = 32,
            ["project_lead"] = 40,
            ["developer"] = 40,
            ["qa"] = 36,
            ["designer"] = 32,
            ["devops"] = 36,
            ["admin"] = 40,
            ["accountant"] = 32
        };

        private static readonly Dictionary<string, int> DefaultRoleActiveTaskLimits = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pm"] = 10,
            ["po"] = 10,
            ["sm"] = 10,
            ["project_lead"] = 8,
            ["developer"] = 7,
            ["qa"] = 7,
            ["designer"] = 6,
            ["devops"] = 6,
            ["admin"] = 10,
            ["accountant"] = 6
        };

        private static readonly Dictionary<string, double> DefaultRoleBaseHours = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pm"] = 3.5,
            ["po"] = 3,
            ["sm"] = 3,
            ["project_lead"] = 4.5,
            ["developer"] = 4,
            ["qa"] = 3,
            ["designer"] = 3.5,
            ["devops"] = 4.5,
            ["admin"] = 3.5,
            ["accountant"] = 3
        };

        public static Dictionary<string, object?> ParseNavigationConfig(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(raw)
                    ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public static string MergeSectionIntoNavigationConfig(string? raw, string key, object value)
        {
            var config = ParseNavigationConfig(raw);
            config[key] = value;
            return JsonSerializer.Serialize(config);
        }

        public static T? ReadSection<T>(string? raw, string key)
        {
            var config = ParseNavigationConfig(raw);
            if (!config.TryGetValue(key, out var value) || value == null)
            {
                return default;
            }

            try
            {
                if (value is JsonElement json)
                {
                    return json.Deserialize<T>();
                }

                if (value is T typed)
                {
                    return typed;
                }

                return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
            }
            catch
            {
                return default;
            }
        }

        public static ProjectRewardRulesDto NormalizeRewardRules(ProjectRewardRulesDto? input)
        {
            return new ProjectRewardRulesDto
            {
                BasePointMultiplier = Clamp(input?.BasePointMultiplier ?? 1, 0.2, 5),
                EarlyBonusPercent = Clamp(input?.EarlyBonusPercent ?? 10, 0, 100),
                AccuracyBonusPercent = Clamp(input?.AccuracyBonusPercent ?? 5, 0, 100),
                LatePenaltyPercent = Clamp(input?.LatePenaltyPercent ?? 10, 0, 100),
                CollaborationBonusPoints = (int)Math.Round(Clamp(input?.CollaborationBonusPoints ?? 2, 0, 100), MidpointRounding.AwayFromZero),
                ManualAdjustmentLimit = (int)Math.Round(Clamp(input?.ManualAdjustmentLimit ?? 200, 10, 5000), MidpointRounding.AwayFromZero)
            };
        }

        public static ProjectCapacityRulesDto NormalizeCapacityRules(ProjectCapacityRulesDto? input)
        {
            var result = new ProjectCapacityRulesDto
            {
                DefaultWeeklyHours = Clamp(input?.DefaultWeeklyHours ?? 40, 8, 80),
                NearLimitPercent = Clamp(input?.NearLimitPercent ?? 80, 40, 100),
                OverLimitPercent = Clamp(input?.OverLimitPercent ?? 100, 60, 200),
                MaxActiveTasksPerMember = (int)Math.Round(Clamp(input?.MaxActiveTasksPerMember ?? 8, 1, 30), MidpointRounding.AwayFromZero),
                RoleWeeklyHours = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase),
                RoleActiveTaskLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            };

            foreach (var pair in DefaultRoleWeeklyHours)
            {
                result.RoleWeeklyHours[pair.Key] = pair.Value;
            }

            foreach (var pair in DefaultRoleActiveTaskLimits)
            {
                result.RoleActiveTaskLimits[pair.Key] = pair.Value;
            }

            if (input?.RoleWeeklyHours != null)
            {
                foreach (var pair in input.RoleWeeklyHours)
                {
                    var key = ProjectExecutionRuleHelper.NormalizeProjectRole(pair.Key);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        result.RoleWeeklyHours[key] = Clamp(pair.Value, 4, 80);
                    }
                }
            }

            if (input?.RoleActiveTaskLimits != null)
            {
                foreach (var pair in input.RoleActiveTaskLimits)
                {
                    var key = ProjectExecutionRuleHelper.NormalizeProjectRole(pair.Key);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        result.RoleActiveTaskLimits[key] = (int)Math.Round(Clamp(pair.Value, 1, 30), MidpointRounding.AwayFromZero);
                    }
                }
            }

            if (result.OverLimitPercent < result.NearLimitPercent)
            {
                result.OverLimitPercent = result.NearLimitPercent;
            }

            return result;
        }

        public static ProjectBaselineSettingsDto NormalizeBaselineSettings(ProjectBaselineSettingsDto? input)
        {
            var result = new ProjectBaselineSettingsDto
            {
                UsePlanningBaseline = input?.UsePlanningBaseline ?? true,
                DefaultBaseHours = Clamp(input?.DefaultBaseHours ?? 4, 0.5, 40),
                HoursPerStoryPoint = Clamp(input?.HoursPerStoryPoint ?? 2, 0.5, 16),
                MinimumSuggestedHours = Clamp(input?.MinimumSuggestedHours ?? 0.5, 0.1, 24),
                MaximumSuggestedHours = Clamp(input?.MaximumSuggestedHours ?? 80, 1, 200),
                RoleHourMultipliers = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase),
                RoleBaseHours = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            };

            if (result.MaximumSuggestedHours < result.MinimumSuggestedHours)
            {
                result.MaximumSuggestedHours = result.MinimumSuggestedHours;
            }

            foreach (var pair in DefaultRoleBaseHours)
            {
                result.RoleBaseHours[pair.Key] = pair.Value;
            }

            foreach (var pair in ProjectExecutionRuleHelper.NormalizeExecutionRules(null).RoleHourMultipliers)
            {
                result.RoleHourMultipliers[pair.Key] = pair.Value;
            }

            if (input?.RoleHourMultipliers != null)
            {
                foreach (var pair in input.RoleHourMultipliers)
                {
                    var key = ProjectExecutionRuleHelper.NormalizeProjectRole(pair.Key);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        result.RoleHourMultipliers[key] = Clamp(pair.Value, 0.3, 3);
                    }
                }
            }

            if (input?.RoleBaseHours != null)
            {
                foreach (var pair in input.RoleBaseHours)
                {
                    var key = ProjectExecutionRuleHelper.NormalizeProjectRole(pair.Key);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        result.RoleBaseHours[key] = Clamp(pair.Value, 0.5, 40);
                    }
                }
            }

            return result;
        }

        public static ProjectMilestoneDto NormalizeMilestone(ProjectMilestoneDto? input)
        {
            var startDate = input?.StartDate?.Date;
            var targetDate = input?.TargetDate?.Date;
            if (targetDate.HasValue && startDate.HasValue && targetDate.Value < startDate.Value)
            {
                targetDate = startDate;
            }

            return new ProjectMilestoneDto
            {
                Id = input?.Id == Guid.Empty ? Guid.NewGuid() : input?.Id ?? Guid.NewGuid(),
                Name = (input?.Name ?? string.Empty).Trim(),
                Description = string.IsNullOrWhiteSpace(input?.Description) ? null : input!.Description!.Trim(),
                ReleaseVersion = string.IsNullOrWhiteSpace(input?.ReleaseVersion) ? null : input!.ReleaseVersion!.Trim(),
                StartDate = startDate,
                TargetDate = targetDate,
                Status = string.IsNullOrWhiteSpace(input?.Status) ? "Planned" : input!.Status.Trim(),
                IsArchived = input?.IsArchived ?? false,
                LinkedSprintIds = (input?.LinkedSprintIds ?? new List<Guid>())
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList()
            };
        }

        public static ProjectPointAdjustmentDto NormalizePointAdjustment(ProjectPointAdjustmentDto? input)
        {
            return new ProjectPointAdjustmentDto
            {
                Id = input?.Id == Guid.Empty ? Guid.NewGuid() : input?.Id ?? Guid.NewGuid(),
                UserId = input?.UserId ?? Guid.Empty,
                CreatedByUserId = input?.CreatedByUserId ?? Guid.Empty,
                Amount = input?.Amount ?? 0,
                Reason = (input?.Reason ?? string.Empty).Trim(),
                AdjustmentType = string.IsNullOrWhiteSpace(input?.AdjustmentType) ? "Manual" : input?.AdjustmentType?.Trim() ?? "Manual",
                CreatedAt = input?.CreatedAt == default ? DateTime.UtcNow : input?.CreatedAt ?? DateTime.UtcNow
            };
        }

        public static int CalculateWalletLevel(int totalPoints)
        {
            var level = 1;
            var nextThreshold = PointsForCareerLevel(level + 1);

            while (totalPoints >= nextThreshold)
            {
                level++;
                nextThreshold = PointsForCareerLevel(level + 1);
            }

            return level;
        }

        private static int PointsForCareerLevel(int level)
        {
            var normalizedLevel = Math.Max(1, level);
            return 250 * normalizedLevel * (normalizedLevel + 1);
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}
