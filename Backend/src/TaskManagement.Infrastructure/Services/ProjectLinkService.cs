using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectLinkService : IProjectLinkService
    {
        private readonly ApplicationDbContext _context;

        public ProjectLinkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllLinksAsync(Guid projectId)
        {
            var links = await _context.ProjectLinks
                .AsNoTracking()
                .Where(p => p.ProjectId == projectId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return await MapLinksAsync(links);
        }

        public async Task<object> CreateLinkAsync(Guid creatorId, Guid projectId, object dto)
        {
            var payload = ProjectLinkPayload.From(dto);
            if (string.IsNullOrWhiteSpace(payload.LinkedType))
                throw new ArgumentException("LinkedType is required.");

            var linkedType = NormalizeLinkedType(payload.LinkedType);
            if (linkedType != "Goal" && linkedType != "Project" && linkedType != "Task" && linkedType != "TrackedLink")
                throw new ArgumentException("Unsupported linked type.");

            if (linkedType == "TrackedLink" && string.IsNullOrWhiteSpace(payload.TrackedUrl))
                throw new ArgumentException("TrackedUrl is required for tracked links.");

            if (linkedType != "TrackedLink" && !payload.LinkedId.HasValue)
                throw new ArgumentException("LinkedId is required.");

            await EnsureTargetExistsAsync(linkedType, payload.LinkedId);

            var exists = await _context.ProjectLinks.AnyAsync(link =>
                link.ProjectId == projectId &&
                link.LinkedType == linkedType &&
                link.LinkedId == payload.LinkedId &&
                link.TrackedUrl == payload.TrackedUrl);
            if (exists)
                throw new InvalidOperationException("Link already exists.");

            var link = new ProjectLink
            {
                Id = Guid.NewGuid(),
                CreatorId = creatorId,
                ProjectId = projectId,
                LinkedType = linkedType,
                LinkedId = payload.LinkedId,
                TrackedUrl = payload.TrackedUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectLinks.Add(link);
            await _context.SaveChangesAsync();

            return (await MapLinksAsync(new[] { link })).First();
        }

        public async Task DeleteLinkAsync(Guid id)
        {
            var link = await _context.ProjectLinks.FindAsync(id);
            if (link != null)
            {
                _context.ProjectLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureTargetExistsAsync(string linkedType, Guid? linkedId)
        {
            if (linkedType == "TrackedLink")
                return;

            var exists = linkedType switch
            {
                "Goal" => await _context.Goals.AnyAsync(g => g.Id == linkedId && !g.IsArchived),
                "Project" => await _context.Projects.AnyAsync(p => p.Id == linkedId && !p.IsDeleted),
                "Task" => await _context.WorkTasks.AnyAsync(t => t.Id == linkedId && !t.IsDeleted),
                _ => false
            };

            if (!exists)
                throw new ArgumentException("Linked target does not exist.");
        }

        private async Task<List<object>> MapLinksAsync(IEnumerable<ProjectLink> links)
        {
            var list = links.ToList();
            var goalIds = list.Where(l => l.LinkedType == "Goal" && l.LinkedId.HasValue).Select(l => l.LinkedId!.Value).ToList();
            var projectIds = list.Where(l => l.LinkedType == "Project" && l.LinkedId.HasValue).Select(l => l.LinkedId!.Value).ToList();
            var taskIds = list.Where(l => l.LinkedType == "Task" && l.LinkedId.HasValue).Select(l => l.LinkedId!.Value).ToList();

            var goalNames = await _context.Goals.Where(g => goalIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Title);
            var projectNames = await _context.Projects.Where(p => projectIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);
            var taskNames = await _context.WorkTasks.Where(t => taskIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Title);

            return list.Select(link => (object)new
            {
                link.Id,
                link.ProjectId,
                link.LinkedType,
                link.LinkedId,
                link.TrackedUrl,
                link.CreatorId,
                link.CreatedAt,
                linkedName = link.LinkedType switch
                {
                    "Goal" when link.LinkedId.HasValue => goalNames.GetValueOrDefault(link.LinkedId.Value),
                    "Project" when link.LinkedId.HasValue => projectNames.GetValueOrDefault(link.LinkedId.Value),
                    "Task" when link.LinkedId.HasValue => taskNames.GetValueOrDefault(link.LinkedId.Value),
                    "TrackedLink" => link.TrackedUrl,
                    _ => null
                }
            }).ToList();
        }

        private static string NormalizeLinkedType(string value)
        {
            return value.Trim().ToLowerInvariant() switch
            {
                "goal" => "Goal",
                "project" => "Project",
                "task" => "Task",
                "worktask" => "Task",
                "trackedlink" => "TrackedLink",
                "tracked_link" => "TrackedLink",
                "link" => "TrackedLink",
                _ => value.Trim()
            };
        }

        private sealed class ProjectLinkPayload
        {
            public string LinkedType { get; set; } = string.Empty;
            public Guid? LinkedId { get; set; }
            public string? TrackedUrl { get; set; }

            public static ProjectLinkPayload From(object dto)
            {
                if (dto is JsonElement element)
                    return FromJson(element);

                var json = JsonSerializer.Serialize(dto);
                return JsonSerializer.Deserialize<ProjectLinkPayload>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ProjectLinkPayload();
            }

            private static ProjectLinkPayload FromJson(JsonElement element)
            {
                var payload = new ProjectLinkPayload();
                if (element.TryGetProperty("linkedType", out var linkedType) || element.TryGetProperty("LinkedType", out linkedType))
                    payload.LinkedType = linkedType.GetString() ?? string.Empty;
                if (element.TryGetProperty("linkedId", out var linkedId) || element.TryGetProperty("LinkedId", out linkedId))
                {
                    if (linkedId.ValueKind == JsonValueKind.String && Guid.TryParse(linkedId.GetString(), out var parsed))
                        payload.LinkedId = parsed;
                    else if (linkedId.ValueKind == JsonValueKind.Null)
                        payload.LinkedId = null;
                }
                if (element.TryGetProperty("trackedUrl", out var trackedUrl) || element.TryGetProperty("TrackedUrl", out trackedUrl))
                    payload.TrackedUrl = trackedUrl.GetString();
                return payload;
            }
        }
    }
}
