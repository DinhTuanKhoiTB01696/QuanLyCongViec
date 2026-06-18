using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class GoalService : IGoalService
    {
        private static readonly string[] GoalStatuses =
        {
            "ĐANG CHỜ XỬ LÝ",
            "ĐÚNG TIẾN ĐỘ",
            "CÓ RỦI RO",
            "KHÔNG ĐÚNG TIẾN ĐỘ",
            "ĐÃ HOÀN TẤT",
            "ĐÃ TẠM DỪNG",
            "ĐÃ HỦY"
        };

        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(Guid workspaceId)
        {
            var goals = await _context.Goals
                .AsNoTracking()
                .Include(g => g.Owner)
                .Include(g => g.Department)
                .Where(g => g.WorkspaceId == workspaceId && !g.IsArchived)
                .ToListAsync();

            return goals.Select(ToGoalSummary);
        }

        public Task<object> GetStatusesAsync()
        {
            return Task.FromResult<object>(GoalStatuses.Select(s => new { value = s, label = s }));
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            var g = await _context.Goals
                .AsNoTracking()
                .Include(g => g.Owner)
                .Include(g => g.Department)
                .Include(g => g.Updates)
                    .ThenInclude(u => u.User)
                .Include(g => g.Lessons)
                .Include(g => g.Risks)
                .Include(g => g.Decisions)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (g == null) return null;

            return ToGoalDetail(g);
        }

        public async Task<object> CreateAsync(Guid creatorId, Guid workspaceId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            
            var title = data.TryGetProperty("title", out var t) ? t.GetString() : null;
            var status = data.TryGetProperty("status", out var s) ? s.GetString() : null;

            var ownerId = creatorId;
            if (data.TryGetProperty("ownerId", out var oid) && Guid.TryParse(oid.GetString(), out var parsedOwnerId))
            {
                ownerId = parsedOwnerId;
            }

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                WorkspaceId = workspaceId,
                Title = string.IsNullOrEmpty(title) ? "New Goal" : title,
                Status = string.IsNullOrEmpty(status) ? "ĐANG CHỜ XỬ LÝ" : status,
                CreatedAt = DateTime.UtcNow
            };
            
            if (data.TryGetProperty("startDate", out var sDateObj))
            {
                var dateStr = sDateObj.GetString();
                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var d))
                {
                    goal.StartDate = d;
                }
            }

            if (data.TryGetProperty("dueDate", out var dDateObj) || data.TryGetProperty("date", out dDateObj))
            {
                var dateStr = dDateObj.GetString();
                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var d))
                {
                    goal.DueDate = d;
                }
            }

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return ToGoalDetail(goal);
        }

        public async Task<object> UpdateAsync(Guid id, object dto)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));

                if (TryGetProperty(data, "parentGoalId", out var pid))
                {
                    var pIdStr = pid.GetString();
                    if (string.IsNullOrEmpty(pIdStr)) goal.ParentGoalId = null;
                    else if (Guid.TryParse(pIdStr, out var parsed)) goal.ParentGoalId = parsed;
                }

                if (TryGetProperty(data, "departmentId", out var did) || TryGetProperty(data, "teamId", out did))
                {
                    var dIdStr = did.GetString();
                    if (string.IsNullOrEmpty(dIdStr)) goal.DepartmentId = null;
                    else if (Guid.TryParse(dIdStr, out var parsed)) goal.DepartmentId = parsed;
                }

                if (TryGetProperty(data, "description", out var description))
                {
                    goal.Description = description.GetString();
                }

                if (TryGetProperty(data, "status", out var status))
                {
                    var statusText = status.GetString();
                    if (!string.IsNullOrWhiteSpace(statusText)) goal.Status = statusText;
                }

                if (TryGetProperty(data, "startDate", out var sdate))
                {
                    var dateStr = sdate.GetString();
                    if (string.IsNullOrEmpty(dateStr)) goal.StartDate = null;
                    else if (DateTime.TryParse(dateStr, out var parsed)) goal.StartDate = parsed;
                }

                if (TryGetProperty(data, "dueDate", out var ddate) || TryGetProperty(data, "endDate", out ddate))
                {
                    var dateStr = ddate.GetString();
                    if (string.IsNullOrEmpty(dateStr)) goal.DueDate = null;
                    else if (DateTime.TryParse(dateStr, out var parsed)) goal.DueDate = parsed;
                }

                if (TryGetProperty(data, "progress", out var progressValue) && progressValue.TryGetInt32(out var progress))
                {
                    goal.Progress = Math.Clamp(progress, 0, 100);
                }

                goal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _context.Entry(goal).Reference(g => g.Owner).LoadAsync();
                await _context.Entry(goal).Reference(g => g.Department).LoadAsync();
            }
            return goal == null ? null! : ToGoalDetail(goal);
        }

        public async Task ArchiveAsync(Guid id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                goal.IsArchived = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> AddUpdateAsync(Guid goalId, Guid userId, object dto)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null) throw new InvalidOperationException("Goal not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var contentText = TryGetProperty(data, "content", out var content) ? content.GetString() ?? "" : "";
            var previousStatus = goal.Status;
            var previousProgress = goal.Progress;
            var nextStatus = TryGetProperty(data, "status", out var status) ? status.GetString() : null;
            int? nextProgress = null;
            DateTime? targetDate = null;

            if (TryGetProperty(data, "progress", out var progressValue) && progressValue.TryGetInt32(out var parsedProgress))
            {
                nextProgress = Math.Clamp(parsedProgress, 0, 100);
            }

            if (!string.IsNullOrWhiteSpace(nextStatus))
            {
                goal.Status = nextStatus;
            }

            if (nextProgress.HasValue)
            {
                goal.Progress = nextProgress.Value;
            }

            if (TryGetProperty(data, "targetDate", out var targetDateValue) || TryGetProperty(data, "startDate", out targetDateValue))
            {
                var dateStr = targetDateValue.GetString();
                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedTargetDate))
                {
                    targetDate = parsedTargetDate;
                    goal.StartDate = parsedTargetDate;
                }
            }

            goal.UpdatedAt = DateTime.UtcNow;

            var update = new GoalUpdate
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                UserId = userId,
                Content = contentText,
                Status = goal.Status,
                PreviousStatus = previousStatus,
                NewStatus = goal.Status,
                PreviousProgress = previousProgress,
                NewProgress = goal.Progress,
                TargetDate = targetDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.GoalUpdates.Add(update);
            await _context.SaveChangesAsync();
            await _context.Entry(update).Reference(u => u.User).LoadAsync();
            return ToGoalUpdateDto(update);
        }

        public async Task<object> AddLessonAsync(Guid goalId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var lesson = new GoalLesson
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                CreatorId = userId,
                Text = data.TryGetProperty("text", out var text) ? text.GetString() ?? "" : "",
                CreatedAt = DateTime.UtcNow
            };
            _context.GoalLessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<object> AddRiskAsync(Guid goalId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var risk = new GoalRisk
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                CreatorId = userId,
                Text = data.TryGetProperty("text", out var text) ? text.GetString() ?? "" : "",
                Severity = data.TryGetProperty("severity", out var severity) ? severity.GetString() ?? "" : "",
                CreatedAt = DateTime.UtcNow
            };
            _context.GoalRisks.Add(risk);
            await _context.SaveChangesAsync();
            return risk;
        }

        public async Task<object> AddDecisionAsync(Guid goalId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var decision = new GoalDecision
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                CreatorId = userId,
                Text = data.TryGetProperty("text", out var text) ? text.GetString() ?? "" : "",
                CreatedAt = DateTime.UtcNow
            };
            _context.GoalDecisions.Add(decision);
            await _context.SaveChangesAsync();
            return decision;
        }

        private static bool TryGetProperty(System.Text.Json.JsonElement data, string propertyName, out System.Text.Json.JsonElement value)
        {
            if (data.TryGetProperty(propertyName, out value))
            {
                return true;
            }

            foreach (var property in data.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static object ToGoalUpdateDto(GoalUpdate u)
        {
            return new
            {
                id = u.Id,
                goalId = u.GoalId,
                userId = u.UserId,
                content = u.Content,
                status = u.Status,
                previousStatus = u.PreviousStatus,
                newStatus = u.NewStatus,
                previousProgress = u.PreviousProgress,
                newProgress = u.NewProgress,
                targetDate = u.TargetDate,
                createdAt = u.CreatedAt,
                user = u.User != null ? new
                {
                    id = u.User.Id,
                    fullName = u.User.FullName,
                    email = u.User.Email,
                    avatarUrl = u.User.AvatarUrl
                } : null
            };
        }

        private static object ToGoalSummary(Goal g)
        {
            return new
            {
                id = g.Id,
                title = g.Title,
                description = g.Description,
                status = g.Status,
                progress = g.Progress,
                startDate = g.StartDate,
                dueDate = g.DueDate,
                createdAt = g.CreatedAt,
                updatedAt = g.UpdatedAt,
                ownerId = g.OwnerId,
                departmentId = g.DepartmentId,
                parentGoalId = g.ParentGoalId,
                isArchived = g.IsArchived,
                owner = g.Owner != null ? new
                {
                    id = g.Owner.Id,
                    fullName = g.Owner.FullName,
                    email = g.Owner.Email,
                    avatarUrl = g.Owner.AvatarUrl
                } : null,
                department = g.Department != null ? new
                {
                    id = g.Department.Id,
                    name = g.Department.Name
                } : null
            };
        }

        private static object ToGoalDetail(Goal g)
        {
            return new
            {
                id = g.Id,
                title = g.Title,
                description = g.Description,
                status = g.Status,
                progress = g.Progress,
                startDate = g.StartDate,
                dueDate = g.DueDate,
                createdAt = g.CreatedAt,
                updatedAt = g.UpdatedAt,
                ownerId = g.OwnerId,
                departmentId = g.DepartmentId,
                parentGoalId = g.ParentGoalId,
                isArchived = g.IsArchived,
                owner = g.Owner != null ? new
                {
                    id = g.Owner.Id,
                    fullName = g.Owner.FullName,
                    email = g.Owner.Email,
                    avatarUrl = g.Owner.AvatarUrl
                } : null,
                department = g.Department != null ? new
                {
                    id = g.Department.Id,
                    name = g.Department.Name
                } : null,
                updates = g.Updates
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(ToGoalUpdateDto),
                lessons = g.Lessons.Select(l => new
                {
                    id = l.Id,
                    goalId = l.GoalId,
                    creatorId = l.CreatorId,
                    text = l.Text,
                    createdAt = l.CreatedAt
                }),
                risks = g.Risks.Select(r => new
                {
                    id = r.Id,
                    goalId = r.GoalId,
                    creatorId = r.CreatorId,
                    text = r.Text,
                    severity = r.Severity,
                    createdAt = r.CreatedAt
                }),
                decisions = g.Decisions.Select(d => new
                {
                    id = d.Id,
                    goalId = d.GoalId,
                    creatorId = d.CreatorId,
                    text = d.Text,
                    decisionDate = d.DecisionDate,
                    createdAt = d.CreatedAt
                })
            };
        }
    }
}
