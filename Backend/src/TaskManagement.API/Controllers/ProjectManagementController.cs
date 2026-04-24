using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Filters;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects/{projectId:guid}/management")]
    public class ProjectManagementController : ControllerBase
    {
        private const string ManagerRoles = "PROJECT_MANAGER,PROJECT_LEAD,PM,PO,SM,Admin";
        private const string BaselineSectionKey = "baselineSettings";
        private const string RewardSectionKey = "rewardRules";
        private const string CapacitySectionKey = "capacityRules";
        private const string ExecutionSectionKey = "executionRules";
        private readonly ApplicationDbContext _context;

        public ProjectManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public sealed class UpdateRewardRulesRequest
        {
            public ProjectRewardRulesDto Rules { get; set; } = new();
        }

        public sealed class UpdateCapacityRulesRequest
        {
            public ProjectCapacityRulesDto Rules { get; set; } = new();
        }

        public sealed class UpdateBaselineSettingsRequest
        {
            public ProjectBaselineSettingsDto Rules { get; set; } = new();
        }

        public sealed class SaveMilestoneRequest
        {
            public ProjectMilestoneDto Milestone { get; set; } = new();
        }

        public sealed class CreatePointAdjustmentRequest
        {
            public Guid UserId { get; set; }
            public int Amount { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string AdjustmentType { get; set; } = "Manual";
        }

        private sealed class ProjectPointLeaderboardRow
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string ProjectRole { get; set; } = string.Empty;
            public int TaskPoints { get; set; }
            public int ManualAdjustments { get; set; }
            public int TotalPoints { get; set; }
            public int TransactionCount { get; set; }
            public DateTime LastActivityAt { get; set; }
        }

        private sealed class ProjectPointHistoryRow
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public int Amount { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public Guid? WorkTaskId { get; set; }
            public string? TaskTitle { get; set; }
            public string? TaskSequenceId { get; set; }
            public string Source { get; set; } = string.Empty;
            public Guid? CreatedByUserId { get; set; }
            public string? CreatedByName { get; set; }
        }

        private sealed class ProjectPointSummaryPayload
        {
            public int TotalProjectPoints { get; set; }
            public int TotalManualAdjustments { get; set; }
            public List<ProjectPointLeaderboardRow> Leaderboard { get; set; } = new();
            public List<ProjectPointHistoryRow> History { get; set; } = new();
        }

        [HttpGet("reward-rules")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetRewardRules(Guid projectId)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            return Ok(new
            {
                statusCode = 200,
                data = ProjectManagementHelper.NormalizeRewardRules(
                    ProjectManagementHelper.ReadSection<ProjectRewardRulesDto>(project.NavigationConfig, RewardSectionKey))
            });
        }

        [HttpPut("reward-rules")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> UpdateRewardRules(Guid projectId, [FromBody] UpdateRewardRulesRequest request)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var rules = ProjectManagementHelper.NormalizeRewardRules(request?.Rules);
            project.NavigationConfig = ProjectManagementHelper.MergeSectionIntoNavigationConfig(project.NavigationConfig, RewardSectionKey, rules);
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Reward rules updated.", data = rules });
        }

        [HttpGet("capacity-rules")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetCapacityRules(Guid projectId)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            return Ok(new
            {
                statusCode = 200,
                data = ProjectManagementHelper.NormalizeCapacityRules(
                    ProjectManagementHelper.ReadSection<ProjectCapacityRulesDto>(project.NavigationConfig, CapacitySectionKey))
            });
        }

        [HttpPut("capacity-rules")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> UpdateCapacityRules(Guid projectId, [FromBody] UpdateCapacityRulesRequest request)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var rules = ProjectManagementHelper.NormalizeCapacityRules(request?.Rules);
            project.NavigationConfig = ProjectManagementHelper.MergeSectionIntoNavigationConfig(project.NavigationConfig, CapacitySectionKey, rules);
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Capacity rules updated.", data = rules });
        }

        [HttpGet("baseline-settings")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetBaselineSettings(Guid projectId)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            return Ok(new
            {
                statusCode = 200,
                data = BuildBaselineSettings(project.NavigationConfig)
            });
        }

        [HttpPut("baseline-settings")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> UpdateBaselineSettings(Guid projectId, [FromBody] UpdateBaselineSettingsRequest request)
        {
            var project = await GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var rules = ProjectManagementHelper.NormalizeBaselineSettings(request?.Rules);
            var executionRules = ProjectExecutionRuleHelper.NormalizeExecutionRules(
                ProjectManagementHelper.ReadSection<ProjectExecutionRulesDto>(project.NavigationConfig, ExecutionSectionKey));

            executionRules.DefaultBaseHours = rules.DefaultBaseHours;
            executionRules.HoursPerStoryPoint = rules.HoursPerStoryPoint;
            executionRules.RoleHourMultipliers = new Dictionary<string, double>(rules.RoleHourMultipliers, StringComparer.OrdinalIgnoreCase);

            project.NavigationConfig = ProjectManagementHelper.MergeSectionIntoNavigationConfig(project.NavigationConfig, BaselineSectionKey, rules);
            project.NavigationConfig = ProjectManagementHelper.MergeSectionIntoNavigationConfig(project.NavigationConfig, ExecutionSectionKey, executionRules);
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Baseline settings updated.", data = rules });
        }

        [HttpGet("milestones")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetMilestones(Guid projectId)
        {
            if (!await ProjectExistsAsync(projectId))
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var milestones = await LoadMilestonesAsync(projectId);
            return Ok(new { statusCode = 200, data = milestones });
        }

        [HttpPost("milestones")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> CreateMilestone(Guid projectId, [FromBody] SaveMilestoneRequest request)
        {
            if (!await ProjectExistsAsync(projectId))
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var milestone = ProjectManagementHelper.NormalizeMilestone(request?.Milestone);
            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                return BadRequest(new { statusCode = 400, message = "Milestone name is required." });
            }

            if (!await ValidateSprintLinksAsync(projectId, milestone.LinkedSprintIds))
            {
                return BadRequest(new { statusCode = 400, message = "One or more linked cycles do not belong to this project." });
            }

            var setting = new SystemSetting
            {
                Id = Guid.NewGuid(),
                SettingGroup = GetMilestoneGroup(projectId),
                Key = milestone.Id.ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(milestone),
                Description = $"Project milestone {milestone.Name}",
                LastModifiedAt = DateTime.UtcNow
            };

            _context.SystemSettings.Add(setting);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Milestone created.", data = milestone });
        }

        [HttpPut("milestones/{milestoneId:guid}")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> UpdateMilestone(Guid projectId, Guid milestoneId, [FromBody] SaveMilestoneRequest request)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(item => item.SettingGroup == GetMilestoneGroup(projectId) && item.Key == milestoneId.ToString());

            if (setting == null)
            {
                return NotFound(new { statusCode = 404, message = "Milestone not found." });
            }

            var milestone = ProjectManagementHelper.NormalizeMilestone(request?.Milestone);
            milestone.Id = milestoneId;
            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                return BadRequest(new { statusCode = 400, message = "Milestone name is required." });
            }

            if (!await ValidateSprintLinksAsync(projectId, milestone.LinkedSprintIds))
            {
                return BadRequest(new { statusCode = 400, message = "One or more linked cycles do not belong to this project." });
            }

            setting.Value = System.Text.Json.JsonSerializer.Serialize(milestone);
            setting.Description = $"Project milestone {milestone.Name}";
            setting.LastModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Milestone updated.", data = milestone });
        }

        [HttpDelete("milestones/{milestoneId:guid}")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> DeleteMilestone(Guid projectId, Guid milestoneId)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(item => item.SettingGroup == GetMilestoneGroup(projectId) && item.Key == milestoneId.ToString());

            if (setting == null)
            {
                return NotFound(new { statusCode = 404, message = "Milestone not found." });
            }

            _context.SystemSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return Ok(new { statusCode = 200, message = "Milestone deleted." });
        }

        [HttpGet("points")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetProjectPoints(Guid projectId)
        {
            if (!await ProjectExistsAsync(projectId))
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var data = await BuildProjectPointDataAsync(projectId);
            return Ok(new { statusCode = 200, data });
        }

        [HttpPost("points/adjustments")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> CreatePointAdjustment(Guid projectId, [FromBody] CreatePointAdjustmentRequest request)
        {
            if (!await ProjectExistsAsync(projectId))
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var actorUserId = GetCurrentUserId();
            if (actorUserId == null)
            {
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });
            }

            if (request == null || request.UserId == Guid.Empty || request.Amount == 0 || string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { statusCode = 400, message = "User, amount, and reason are required." });
            }

            var projectMember = await _context.ProjectMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.ProjectId == projectId && item.UserId == request.UserId && item.Status);

            if (projectMember == null)
            {
                return BadRequest(new { statusCode = 400, message = "User is not an active member of this project." });
            }

            var project = await GetProjectAsync(projectId);
            var rewardRules = ProjectManagementHelper.NormalizeRewardRules(
                ProjectManagementHelper.ReadSection<ProjectRewardRulesDto>(project?.NavigationConfig, RewardSectionKey));
            if (Math.Abs(request.Amount) > rewardRules.ManualAdjustmentLimit)
            {
                return BadRequest(new { statusCode = 400, message = $"Manual adjustment cannot exceed {rewardRules.ManualAdjustmentLimit} points." });
            }

            var wallet = await _context.UserWallets.FirstOrDefaultAsync(item => item.UserId == request.UserId);
            if (wallet == null)
            {
                wallet = new UserWallet
                {
                    UserId = request.UserId,
                    TotalPoints = 0,
                    Level = 1
                };
                _context.UserWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            wallet.TotalPoints = Math.Max(0, wallet.TotalPoints + request.Amount);
            wallet.Level = ProjectManagementHelper.CalculateWalletLevel(wallet.TotalPoints);

            _context.PointTransactions.Add(new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserWalletUserId = request.UserId,
                WorkTaskId = null,
                Amount = request.Amount,
                TransactionType = "ProjectManualAdjustment",
                Reason = $"[Project:{projectId}] {request.Reason.Trim()}",
                CreatedAt = DateTime.UtcNow
            });

            var adjustment = ProjectManagementHelper.NormalizePointAdjustment(new ProjectPointAdjustmentDto
            {
                UserId = request.UserId,
                CreatedByUserId = actorUserId.Value,
                Amount = request.Amount,
                Reason = request.Reason,
                AdjustmentType = string.IsNullOrWhiteSpace(request.AdjustmentType) ? "Manual" : request.AdjustmentType
            });

            _context.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                SettingGroup = GetPointAdjustmentGroup(projectId),
                Key = adjustment.Id.ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(adjustment),
                Description = $"Project point adjustment for {request.UserId}",
                LastModifiedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Point adjustment created.", data = adjustment });
        }

        [HttpGet("operational-dashboard")]
        [ProjectAuthorize(ManagerRoles)]
        public async Task<IActionResult> GetOperationalDashboard(Guid projectId)
        {
            if (!await ProjectExistsAsync(projectId))
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var dashboard = await BuildOperationalDashboardAsync(projectId);
            return Ok(new { statusCode = 200, data = dashboard });
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private async Task<Project?> GetProjectAsync(Guid projectId)
        {
            return await _context.Projects.FirstOrDefaultAsync(item => item.Id == projectId && !item.IsDeleted);
        }

        private async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            return await _context.Projects.AsNoTracking().AnyAsync(item => item.Id == projectId && !item.IsDeleted);
        }

        private static string GetMilestoneGroup(Guid projectId) => $"ProjectMilestones:{projectId:D}";
        private static string GetPointAdjustmentGroup(Guid projectId) => $"ProjectPointAdjustments:{projectId:D}";

        private async Task<bool> ValidateSprintLinksAsync(Guid projectId, IEnumerable<Guid> sprintIds)
        {
            var requested = sprintIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (requested.Count == 0)
            {
                return true;
            }

            var count = await _context.Sprints.CountAsync(item => item.ProjectId == projectId && requested.Contains(item.Id));
            return count == requested.Count;
        }

        private ProjectBaselineSettingsDto BuildBaselineSettings(string? navigationConfig)
        {
            var baseline = ProjectManagementHelper.NormalizeBaselineSettings(
                ProjectManagementHelper.ReadSection<ProjectBaselineSettingsDto>(navigationConfig, BaselineSectionKey));
            var executionRules = ProjectExecutionRuleHelper.NormalizeExecutionRules(
                ProjectManagementHelper.ReadSection<ProjectExecutionRulesDto>(navigationConfig, ExecutionSectionKey));

            baseline.DefaultBaseHours = executionRules.DefaultBaseHours;
            baseline.HoursPerStoryPoint = executionRules.HoursPerStoryPoint;
            baseline.RoleHourMultipliers = new Dictionary<string, double>(executionRules.RoleHourMultipliers, StringComparer.OrdinalIgnoreCase);
            return baseline;
        }

        private async Task<List<ProjectMilestoneDto>> LoadMilestonesAsync(Guid projectId)
        {
            var settings = await _context.SystemSettings
                .AsNoTracking()
                .Where(item => item.SettingGroup == GetMilestoneGroup(projectId))
                .ToListAsync();

            return settings
                .Select(item => ProjectManagementHelper.NormalizeMilestone(
                    System.Text.Json.JsonSerializer.Deserialize<ProjectMilestoneDto>(item.Value)))
                .OrderBy(item => item.TargetDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Name)
                .ToList();
        }

        private async Task<List<ProjectPointAdjustmentDto>> LoadPointAdjustmentsAsync(Guid projectId)
        {
            var settings = await _context.SystemSettings
                .AsNoTracking()
                .Where(item => item.SettingGroup == GetPointAdjustmentGroup(projectId))
                .ToListAsync();

            return settings
                .Select(item => ProjectManagementHelper.NormalizePointAdjustment(
                    System.Text.Json.JsonSerializer.Deserialize<ProjectPointAdjustmentDto>(item.Value)))
                .OrderByDescending(item => item.CreatedAt)
                .ToList();
        }

        private async Task<ProjectPointSummaryPayload> BuildProjectPointDataAsync(Guid projectId)
        {
            var members = await _context.ProjectMembers
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId && item.Status)
                .Select(item => new
                {
                    item.UserId,
                    UserName = item.User.FullName ?? item.User.Email,
                    item.ProjectRole
                })
                .ToListAsync();

            var taskTransactions = await _context.PointTransactions
                .AsNoTracking()
                .Where(item => item.WorkTaskId != null && item.WorkTask != null && item.WorkTask.ProjectId == projectId)
                .Select(item => new
                {
                    item.Id,
                    UserId = item.UserWalletUserId,
                    UserName = item.UserWallet.User.FullName ?? item.UserWallet.User.Email,
                    item.Amount,
                    item.TransactionType,
                    item.Reason,
                    item.CreatedAt,
                    item.WorkTaskId,
                    TaskTitle = item.WorkTask != null ? item.WorkTask.Title : null,
                    TaskSequenceId = item.WorkTask != null ? item.WorkTask.SequenceId : null
                })
                .ToListAsync();

            var manualAdjustments = await LoadPointAdjustmentsAsync(projectId);
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(item => members.Select(member => member.UserId).Contains(item.Id) || manualAdjustments.Select(adjustment => adjustment.CreatedByUserId).Contains(item.Id))
                .Select(item => new
                {
                    item.Id,
                    UserName = item.FullName ?? item.Email
                })
                .ToDictionaryAsync(item => item.Id, item => item.UserName);

            var leaderboard = members
                .Select(member =>
                {
                    var userTaskTransactions = taskTransactions.Where(item => item.UserId == member.UserId).ToList();
                    var userManualAdjustments = manualAdjustments.Where(item => item.UserId == member.UserId).ToList();
                    return new ProjectPointLeaderboardRow
                    {
                        UserId = member.UserId,
                        UserName = member.UserName,
                        ProjectRole = member.ProjectRole,
                        TaskPoints = userTaskTransactions.Sum(item => item.Amount),
                        ManualAdjustments = userManualAdjustments.Sum(item => item.Amount),
                        TotalPoints = userTaskTransactions.Sum(item => item.Amount) + userManualAdjustments.Sum(item => item.Amount),
                        TransactionCount = userTaskTransactions.Count + userManualAdjustments.Count,
                        LastActivityAt = userTaskTransactions.Select(item => item.CreatedAt)
                            .Concat(userManualAdjustments.Select(item => item.CreatedAt))
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()
                    };
                })
                .OrderByDescending(item => item.TotalPoints)
                .ThenBy(item => item.UserName)
                .ToList();

            var history = taskTransactions
                .Select(item => new ProjectPointHistoryRow
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Amount = item.Amount,
                    TransactionType = item.TransactionType,
                    Reason = item.Reason,
                    CreatedAt = item.CreatedAt,
                    WorkTaskId = item.WorkTaskId,
                    TaskTitle = item.TaskTitle,
                    TaskSequenceId = item.TaskSequenceId,
                    Source = "task",
                    CreatedByUserId = null,
                    CreatedByName = null
                })
                .Concat(manualAdjustments.Select(item => new ProjectPointHistoryRow
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    UserName = userLookup.GetValueOrDefault(item.UserId, "Member"),
                    Amount = item.Amount,
                    TransactionType = item.AdjustmentType,
                    Reason = item.Reason,
                    CreatedAt = item.CreatedAt,
                    WorkTaskId = null,
                    TaskTitle = null,
                    TaskSequenceId = null,
                    Source = "manual",
                    CreatedByUserId = item.CreatedByUserId,
                    CreatedByName = userLookup.GetValueOrDefault(item.CreatedByUserId, "Manager")
                }))
                .OrderByDescending(item => item.CreatedAt)
                .Take(40)
                .ToList();

            return new ProjectPointSummaryPayload
            {
                TotalProjectPoints = leaderboard.Sum(item => item.TotalPoints),
                TotalManualAdjustments = manualAdjustments.Sum(item => item.Amount),
                Leaderboard = leaderboard,
                History = history
            };
        }

        private async Task<object> BuildOperationalDashboardAsync(Guid projectId)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == projectId && !item.IsDeleted);

            var rewardRules = ProjectManagementHelper.NormalizeRewardRules(
                ProjectManagementHelper.ReadSection<ProjectRewardRulesDto>(project?.NavigationConfig, RewardSectionKey));
            var capacityRules = ProjectManagementHelper.NormalizeCapacityRules(
                ProjectManagementHelper.ReadSection<ProjectCapacityRulesDto>(project?.NavigationConfig, CapacitySectionKey));
            var baselineSettings = BuildBaselineSettings(project?.NavigationConfig);
            var milestones = await LoadMilestonesAsync(projectId);
            var pointData = await BuildProjectPointDataAsync(projectId);

            var tasks = await _context.WorkTasks
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId && !item.IsDeleted)
                .Select(item => new
                {
                    item.Id,
                    item.SprintId,
                    item.Title,
                    item.TotalEstimatedHours,
                    item.TotalActualHours,
                    item.DueDate,
                    item.StoryPoints,
                    StatusName = item.TaskStatus.Name
                })
                .ToListAsync();

            var assignments = await _context.TaskAssignments
                .AsNoTracking()
                .Where(item => item.Status && item.WorkTask.ProjectId == projectId && !item.WorkTask.IsDeleted)
                .Select(item => new
                {
                    item.UserId,
                    UserName = item.User.FullName ?? item.User.Email,
                    item.WorkTaskId,
                    item.EstimatedHours,
                    item.TotalActualHours
                })
                .ToListAsync();

            var milestoneRows = milestones.Select(milestone =>
            {
                var milestoneTasks = tasks
                    .Where(task =>
                        milestone.LinkedSprintIds.Contains(task.SprintId ?? Guid.Empty) ||
                        (milestone.StartDate.HasValue && milestone.TargetDate.HasValue &&
                         task.DueDate.HasValue &&
                         task.DueDate.Value.Date >= milestone.StartDate.Value.Date &&
                         task.DueDate.Value.Date <= milestone.TargetDate.Value.Date))
                    .ToList();

                return new
                {
                    milestone.Id,
                    milestone.Name,
                    milestone.ReleaseVersion,
                    milestone.Status,
                    milestone.TargetDate,
                    linkedCycleCount = milestone.LinkedSprintIds.Count,
                    taskCount = milestoneTasks.Count,
                    completedTaskCount = milestoneTasks.Count(task => IsDoneStatus(task.StatusName)),
                    overdueTaskCount = milestoneTasks.Count(task => !IsDoneStatus(task.StatusName) && task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.UtcNow.Date)
                };
            }).ToList();

            var memberLoad = assignments
                .GroupBy(item => new { item.UserId, item.UserName })
                .Select(group =>
                {
                    var role = _context.ProjectMembers
                        .AsNoTracking()
                        .Where(item => item.ProjectId == projectId && item.UserId == group.Key.UserId && item.Status)
                        .Select(item => item.ProjectRole)
                        .FirstOrDefault() ?? string.Empty;
                    var normalizedRole = ProjectExecutionRuleHelper.NormalizeProjectRole(role);
                    var allowedHours = capacityRules.RoleWeeklyHours.TryGetValue(normalizedRole, out var roleHours)
                        ? roleHours
                        : capacityRules.DefaultWeeklyHours;
                    var allowedActiveTasks = capacityRules.RoleActiveTaskLimits.TryGetValue(normalizedRole, out var roleTasks)
                        ? roleTasks
                        : capacityRules.MaxActiveTasksPerMember;
                    var estimated = Math.Round(group.Sum(item => item.EstimatedHours), 1);
                    var actual = Math.Round(group.Sum(item => item.TotalActualHours), 1);
                    var activeTaskCount = group.Select(item => item.WorkTaskId).Distinct().Count();
                    var loadPercent = allowedHours <= 0 ? 0 : Math.Round((estimated / allowedHours) * 100, 1);
                    var state = estimated >= allowedHours * (capacityRules.OverLimitPercent / 100d) || activeTaskCount > allowedActiveTasks
                        ? "Over capacity"
                        : estimated >= allowedHours * (capacityRules.NearLimitPercent / 100d)
                            ? "Near limit"
                            : "Healthy";

                    return new
                    {
                        group.Key.UserId,
                        group.Key.UserName,
                        ProjectRole = role,
                        EstimatedHours = estimated,
                        ActualHours = actual,
                        AllowedHours = allowedHours,
                        ActiveTaskCount = activeTaskCount,
                        AllowedActiveTasks = allowedActiveTasks,
                        LoadPercent = loadPercent,
                        State = state
                    };
                })
                .OrderByDescending(item => item.LoadPercent)
                .ThenByDescending(item => item.ActiveTaskCount)
                .ToList();

            var navigationConfig = ProjectManagementHelper.ParseNavigationConfig(project?.NavigationConfig);
            navigationConfig.TryGetValue("planningBaseline", out var planningBaseline);

            return new
            {
                overview = new
                {
                    totalTasks = tasks.Count,
                    completedTasks = tasks.Count(task => IsDoneStatus(task.StatusName)),
                    overdueTasks = tasks.Count(task => !IsDoneStatus(task.StatusName) && task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.UtcNow.Date),
                    activeMilestones = milestones.Count(item => !item.IsArchived),
                    overCapacityMembers = memberLoad.Count(item => item.State == "Over capacity"),
                    nearLimitMembers = memberLoad.Count(item => item.State == "Near limit"),
                    totalEstimatedHours = Math.Round(tasks.Sum(task => task.TotalEstimatedHours), 1),
                    totalActualHours = Math.Round(tasks.Sum(task => task.TotalActualHours), 1)
                },
                rewardHealth = new
                {
                    rewardRules,
                    projectPoints = pointData
                },
                capacityHealth = new
                {
                    rules = capacityRules,
                    rows = memberLoad.Take(12).ToList()
                },
                baselineHealth = new
                {
                    rules = baselineSettings,
                    planningBaseline
                },
                milestones = milestoneRows,
                topContributors = pointData.Leaderboard.Take(8).ToList()
            };
        }

        private static bool IsDoneStatus(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return false;
            }

            var normalized = statusName.Trim().ToUpperInvariant();
            return normalized.Contains("DONE") || normalized.Contains("COMPLETE");
        }
    }
}
