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

        public async Task<object> UpdateUpdateAsync(Guid goalId, Guid updateId, Guid userId, object dto)
        {
            var update = await _context.GoalUpdates.FindAsync(updateId);
            if (update == null || update.GoalId != goalId) throw new InvalidOperationException("Update not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (TryGetProperty(data, "content", out var content)) update.Content = content.GetString() ?? update.Content;
            
            // Should not modify previousStatus or previousProgress, just the new state if allowed, but we'll stick to updating content mostly for now.
            await _context.SaveChangesAsync();
            await _context.Entry(update).Reference(u => u.User).LoadAsync();
            return ToGoalUpdateDto(update);
        }

        public async Task DeleteUpdateAsync(Guid goalId, Guid updateId, Guid userId)
        {
            var update = await _context.GoalUpdates.FindAsync(updateId);
            if (update != null && update.GoalId == goalId)
            {
                _context.GoalUpdates.Remove(update);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> UpdateLessonAsync(Guid goalId, Guid lessonId, Guid userId, object dto)
        {
            var lesson = await _context.GoalLessons.FindAsync(lessonId);
            if (lesson == null || lesson.GoalId != goalId) throw new InvalidOperationException("Lesson not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (TryGetProperty(data, "text", out var text)) lesson.Text = text.GetString() ?? lesson.Text;
            
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task DeleteLessonAsync(Guid goalId, Guid lessonId, Guid userId)
        {
            var lesson = await _context.GoalLessons.FindAsync(lessonId);
            if (lesson != null && lesson.GoalId == goalId)
            {
                _context.GoalLessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> UpdateRiskAsync(Guid goalId, Guid riskId, Guid userId, object dto)
        {
            var risk = await _context.GoalRisks.FindAsync(riskId);
            if (risk == null || risk.GoalId != goalId) throw new InvalidOperationException("Risk not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (TryGetProperty(data, "text", out var text)) risk.Text = text.GetString() ?? risk.Text;
            if (TryGetProperty(data, "severity", out var severity)) risk.Severity = severity.GetString() ?? risk.Severity;
            
            await _context.SaveChangesAsync();
            return risk;
        }

        public async Task DeleteRiskAsync(Guid goalId, Guid riskId, Guid userId)
        {
            var risk = await _context.GoalRisks.FindAsync(riskId);
            if (risk != null && risk.GoalId == goalId)
            {
                _context.GoalRisks.Remove(risk);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> UpdateDecisionAsync(Guid goalId, Guid decisionId, Guid userId, object dto)
        {
            var decision = await _context.GoalDecisions.FindAsync(decisionId);
            if (decision == null || decision.GoalId != goalId) throw new InvalidOperationException("Decision not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (TryGetProperty(data, "text", out var text)) decision.Text = text.GetString() ?? decision.Text;
            
            await _context.SaveChangesAsync();
            return decision;
        }

        public async Task DeleteDecisionAsync(Guid goalId, Guid decisionId, Guid userId)
        {
            var decision = await _context.GoalDecisions.FindAsync(decisionId);
            if (decision != null && decision.GoalId == goalId)
            {
                _context.GoalDecisions.Remove(decision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> GetCommentsAsync(Guid goalId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.GoalId == goalId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(ToCommentDto);
        }

        public async Task<object> GetUpdateCommentsAsync(Guid updateId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.GoalUpdateId == updateId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(ToCommentDto);
        }

        public async Task<object> AddCommentAsync(Guid goalId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var text = TryGetProperty(data, "content", out var content) ? content.GetString() ?? "" : "";
            
            Guid? parentId = null;
            if (TryGetProperty(data, "parentCommentId", out var pIdStr) && Guid.TryParse(pIdStr.GetString(), out var parsedPId))
            {
                parentId = parsedPId;
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                UserId = userId,
                Content = text,
                ParentCommentId = parentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return ToCommentDto(comment);
        }

        public async Task<object> AddUpdateCommentAsync(Guid goalId, Guid updateId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var text = TryGetProperty(data, "content", out var content) ? content.GetString() ?? "" : "";

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                GoalUpdateId = updateId,
                UserId = userId,
                Content = text,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return ToCommentDto(comment);
        }

        public async Task<object> UpdateCommentAsync(Guid commentId, Guid userId, object dto)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) throw new InvalidOperationException("Comment not found");

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (TryGetProperty(data, "content", out var content)) comment.Content = content.GetString() ?? comment.Content;
            
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();
            return ToCommentDto(comment);
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> GetReactionsAsync(Guid updateId)
        {
            var reactions = await _context.GoalUpdateReactions
                .Where(r => r.GoalUpdateId == updateId)
                .ToListAsync();

            return reactions.GroupBy(r => r.ReactionType).Select(g => new
            {
                reactionType = g.Key,
                count = g.Count(),
                users = g.Select(x => x.UserId)
            });
        }

        public async Task<object> ToggleReactionAsync(Guid updateId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var type = TryGetProperty(data, "reactionType", out var rt) ? rt.GetString() ?? "like" : "like";

            var existing = await _context.GoalUpdateReactions.FirstOrDefaultAsync(r => r.GoalUpdateId == updateId && r.UserId == userId && r.ReactionType == type);
            if (existing != null)
            {
                _context.GoalUpdateReactions.Remove(existing);
            }
            else
            {
                _context.GoalUpdateReactions.Add(new GoalUpdateReaction
                {
                    Id = Guid.NewGuid(),
                    GoalUpdateId = updateId,
                    UserId = userId,
                    ReactionType = type,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
            return await GetReactionsAsync(updateId);
        }

        public async Task<object> GetUpdateAttachmentsAsync(Guid updateId)
        {
            var attachments = await _context.GoalUpdateAttachments
                .Where(a => a.GoalUpdateId == updateId)
                .ToListAsync();

            return attachments.Select(a => new
            {
                id = a.Id,
                fileName = a.FileName,
                fileUrl = a.FileUrl,
                fileSize = a.FileSize,
                createdAt = a.CreatedAt
            });
        }

        public async Task<object> AddUpdateAttachmentAsync(Guid updateId, Guid userId, string fileName, string fileUrl, long fileSize)
        {
            var attachment = new GoalUpdateAttachment
            {
                Id = Guid.NewGuid(),
                GoalUpdateId = updateId,
                UserId = userId,
                FileName = fileName,
                FileUrl = fileUrl,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow
            };

            _context.GoalUpdateAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return new
            {
                id = attachment.Id,
                fileName = attachment.FileName,
                fileUrl = attachment.FileUrl,
                fileSize = attachment.FileSize,
                createdAt = attachment.CreatedAt
            };
        }

        public async Task DeleteUpdateAttachmentAsync(Guid attachmentId, Guid userId)
        {
            var attachment = await _context.GoalUpdateAttachments.FindAsync(attachmentId);
            if (attachment != null)
            {
                _context.GoalUpdateAttachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> GetProjectLinksAsync(Guid goalId, string linkCategory = null)
        {
            var query = _context.ProjectLinks
                .Include(pl => pl.Project)
                .Where(pl => pl.LinkedType == "Goal" && pl.LinkedId == goalId);
                
            if (!string.IsNullOrEmpty(linkCategory))
            {
                query = query.Where(pl => pl.LinkCategory == linkCategory);
            }

            var links = await query.ToListAsync();

            return links.Select(l => new
            {
                id = l.Id,
                projectId = l.ProjectId,
                projectName = l.Project.Name,
                projectKey = l.Project.Identifier,
                projectCover = l.Project.NavigationConfig,
                linkType = l.LinkCategory,
                linkedType = l.LinkedType,
                linkedId = l.LinkedId,
                createdAt = l.CreatedAt
            });
        }

        public async Task<object> AddProjectLinkAsync(Guid goalId, Guid userId, object dto)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            if (!TryGetProperty(data, "projectId", out var pidStr) || !Guid.TryParse(pidStr.GetString(), out var projectId))
            {
                throw new ArgumentException("Invalid projectId");
            }
            
            string linkCategory = "spaceProject";
            if (TryGetProperty(data, "linkType", out var linkTypeProp))
            {
                linkCategory = linkTypeProp.GetString() ?? "spaceProject";
            }

            var existing = await _context.ProjectLinks.FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.LinkedType == "Goal" && pl.LinkedId == goalId && pl.LinkCategory == linkCategory);
            if (existing != null) throw new InvalidOperationException("Project link already exists.");

            var link = new ProjectLink
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                LinkedType = "Goal",
                LinkedId = goalId,
                LinkCategory = linkCategory,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectLinks.Add(link);
            await _context.SaveChangesAsync();
            
            await _context.Entry(link).Reference(l => l.Project).LoadAsync();
            return new
            {
                id = link.Id,
                projectId = link.ProjectId,
                projectName = link.Project.Name,
                projectKey = link.Project.Identifier,
                linkType = link.LinkCategory,
                createdAt = link.CreatedAt
            };
        }

        public async Task DeleteProjectLinkAsync(Guid goalId, Guid linkId, Guid userId)
        {
            var link = await _context.ProjectLinks.FirstOrDefaultAsync(l => l.Id == linkId && l.LinkedType == "Goal" && l.LinkedId == goalId);
            if (link != null)
            {
                _context.ProjectLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        private static object ToCommentDto(Comment c)
        {
            return new
            {
                id = c.Id,
                content = c.Content,
                authorId = c.UserId,
                authorName = c.User?.FullName ?? c.User?.Email,
                authorEmail = c.User?.Email,
                authorAvatar = c.User?.AvatarUrl,
                createdAt = c.CreatedAt,
                updatedAt = c.UpdatedAt,
                parentCommentId = c.ParentCommentId
            };
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
