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
        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(Guid workspaceId)
        {
            return await _context.Goals
                .AsNoTracking()
                .Where(g => g.WorkspaceId == workspaceId && !g.IsArchived)
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.Goals
                .AsNoTracking()
                .Include(g => g.Updates)
                .Include(g => g.Lessons)
                .Include(g => g.Risks)
                .Include(g => g.Decisions)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<object> CreateAsync(Guid creatorId, Guid workspaceId, object dto)
        {
            // Placeholder for real mapping
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                OwnerId = creatorId,
                WorkspaceId = workspaceId,
                Title = "New Goal",
                CreatedAt = DateTime.UtcNow
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<object> UpdateAsync(Guid id, object dto)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                goal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return goal;
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
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var update = new GoalUpdate
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                UserId = userId,
                Content = data.TryGetProperty("content", out var content) ? content.GetString() ?? "" : "",
                Status = data.TryGetProperty("status", out var status) ? status.GetString() ?? "" : "",
                CreatedAt = DateTime.UtcNow
            };
            _context.GoalUpdates.Add(update);
            await _context.SaveChangesAsync();
            return update;
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
    }
}
