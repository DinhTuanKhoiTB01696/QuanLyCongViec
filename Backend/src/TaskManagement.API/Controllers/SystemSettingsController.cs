using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.Json;
using TaskManagement.API.Filters;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SystemSettingsController : ControllerBase
    {
        private static readonly string[] AdminAccessRoles =
        {
            "superadmin",
            "admin",
            "system admin",
            "organization admin",
            "accessadmin",
            "access admin"
        };

        private readonly ApplicationDbContext _context;

        public SystemSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{group}")]
        public async Task<IActionResult> GetSettingsByGroup(string group)
        {
            try
            {
                if (RequiresAdminAccess(group) && !await CurrentUserHasAdminAccessAsync())
                {
                    return Forbid();
                }

                var settings = await _context.SystemSettings
                    .Where(s => s.SettingGroup == group)
                    .ToListAsync();

                var data = settings.ToDictionary(s => s.Key, s => s.Value);

                return Ok(new { statusCode = 200, data = data });
            }
            catch
            {
                return Ok(new { statusCode = 200, data = new Dictionary<string, string>() });
            }
        }

        public class UpdateSettingRequest
        {
            public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        }

        [HttpPut("{group}")]
        public async Task<IActionResult> UpdateSettingsByGroup(string group, [FromBody] UpdateSettingRequest request)
        {
            if (RequiresAdminAccess(group) && !await CurrentUserHasAdminAccessAsync())
            {
                return Forbid();
            }

            var existingSettings = await _context.SystemSettings
                .Where(s => s.SettingGroup == group)
                .ToListAsync();

            foreach (var kvp in request.Settings)
            {
                var setting = existingSettings.FirstOrDefault(s => s.Key == kvp.Key);
                if (setting != null)
                {
                    setting.Value = kvp.Value;
                    setting.LastModifiedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        Id = Guid.NewGuid(),
                        SettingGroup = group,
                        Key = kvp.Key,
                        Value = kvp.Value,
                        LastModifiedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { statusCode = 200, message = "Settings updated successfully" });
        }

        [HttpGet("admin/default-task-statuses")]
        [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
        public async Task<IActionResult> GetDefaultTaskStatuses()
        {
            var items = await GetJsonListSetting("AdminDefaults", "DefaultTaskStatuses", GetDefaultTaskStatusItems());
            return Ok(new { statusCode = 200, data = items });
        }

        [HttpPut("admin/default-task-statuses")]
        [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
        public async Task<IActionResult> UpdateDefaultTaskStatuses([FromBody] StatusListRequest request)
        {
            var items = NormalizeStatusItems(request.Items, GetDefaultTaskStatusItems());
            await SaveJsonListSetting("AdminDefaults", "DefaultTaskStatuses", items, "Default workflow statuses");
            return Ok(new { statusCode = 200, message = "Default task statuses updated successfully.", data = items });
        }

        [HttpGet("admin/project-statuses")]
        [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
        public async Task<IActionResult> GetProjectStatuses()
        {
            var items = await GetJsonListSetting("AdminDefaults", "ProjectStatuses", GetDefaultProjectStatuses());
            return Ok(new { statusCode = 200, data = items });
        }

        [HttpPut("admin/project-statuses")]
        [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
        public async Task<IActionResult> UpdateProjectStatuses([FromBody] StatusListRequest request)
        {
            var items = NormalizeStatusItems(request.Items, GetDefaultProjectStatuses());
            await SaveJsonListSetting("AdminDefaults", "ProjectStatuses", items, "Project lifecycle statuses");
            return Ok(new { statusCode = 200, message = "Project statuses updated successfully.", data = items });
        }

        [HttpGet("admin/activity-metrics")]
        [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
        public async Task<IActionResult> GetSystemMetrics()
        {
            var recentTaskAuditTimes = await _context.AuditLogs
                .OrderByDescending(log => log.CreatedAt)
                .Take(16)
                .Select(log => log.CreatedAt)
                .ToListAsync();

            var recentSystemAuditTimes = await _context.SystemAuditLogs
                .OrderByDescending(log => log.CreatedAt)
                .Take(16)
                .Select(log => log.CreatedAt)
                .ToListAsync();

            var combined = recentTaskAuditTimes
                .Concat(recentSystemAuditTimes)
                .OrderBy(time => time)
                .ToList();

            var metrics = new List<int>();
            if (combined.Count >= 2)
            {
                for (var index = 1; index < combined.Count; index += 1)
                {
                    var deltaMs = (int)Math.Clamp((combined[index] - combined[index - 1]).TotalMilliseconds, 60, 1800);
                    metrics.Add(deltaMs);
                }
            }

            if (metrics.Count == 0)
            {
                metrics = new List<int> { 220, 180, 260, 210, 190, 240, 200, 170, 230, 215, 205, 195 };
            }

            return Ok(new { statusCode = 200, data = metrics.TakeLast(12).ToList() });
        }

        public class StatusListRequest
        {
            public List<StatusItem> Items { get; set; } = new();
        }

        public class StatusItem
        {
            public string Key { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public int Position { get; set; }
            public bool IsDefault { get; set; }
        }

        private async Task<List<StatusItem>> GetJsonListSetting(string group, string key, List<StatusItem> fallback)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(item => item.SettingGroup == group && item.Key == key);
            if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
            {
                return fallback;
            }

            try
            {
                return JsonSerializer.Deserialize<List<StatusItem>>(setting.Value) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private async Task SaveJsonListSetting(string group, string key, List<StatusItem> items, string description)
        {
            var existing = await _context.SystemSettings.FirstOrDefaultAsync(item => item.SettingGroup == group && item.Key == key);
            if (existing == null)
            {
                existing = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingGroup = group,
                    Key = key,
                    Description = description
                };
                _context.SystemSettings.Add(existing);
            }

            existing.Value = JsonSerializer.Serialize(items);
            existing.Description = description;
            existing.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private static List<StatusItem> NormalizeStatusItems(List<StatusItem>? items, List<StatusItem> fallback)
        {
            var source = items == null || items.Count == 0 ? fallback : items;
            return source
                .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                .Select((item, index) => new StatusItem
                {
                    Key = string.IsNullOrWhiteSpace(item.Key)
                        ? item.Name.Trim().ToUpperInvariant().Replace(" ", "_")
                        : item.Key.Trim().ToUpperInvariant().Replace(" ", "_"),
                    Name = item.Name.Trim(),
                    Color = string.IsNullOrWhiteSpace(item.Color) ? "#94A3B8" : item.Color.Trim(),
                    Position = item.Position > 0 ? item.Position : index,
                    IsDefault = item.IsDefault
                })
                .ToList();
        }

        private static List<StatusItem> GetDefaultTaskStatusItems()
        {
            return new List<StatusItem>
            {
                new() { Key = "BACKLOG", Name = "Backlog", Color = "#71717A", Position = 0, IsDefault = true },
                new() { Key = "TODO", Name = "Todo", Color = "#94A3B8", Position = 1, IsDefault = true },
                new() { Key = "IN_PROGRESS", Name = "In Progress", Color = "#3B82F6", Position = 2, IsDefault = true },
                new() { Key = "IN_REVIEW", Name = "In Review", Color = "#F59E0B", Position = 3, IsDefault = true },
                new() { Key = "DONE", Name = "Done", Color = "#10B981", Position = 4, IsDefault = true },
                new() { Key = "CANCELLED", Name = "Cancelled", Color = "#EF4444", Position = 5, IsDefault = true }
            };
        }

        private static List<StatusItem> GetDefaultProjectStatuses()
        {
            return new List<StatusItem>
            {
                new() { Key = "PLANNING", Name = "Planning", Color = "#94A3B8", Position = 0, IsDefault = true },
                new() { Key = "ACTIVE", Name = "Active", Color = "#3B82F6", Position = 1, IsDefault = true },
                new() { Key = "ON_HOLD", Name = "On Hold", Color = "#F59E0B", Position = 2, IsDefault = false },
                new() { Key = "COMPLETED", Name = "Completed", Color = "#10B981", Position = 3, IsDefault = true },
                new() { Key = "CANCELLED", Name = "Cancelled", Color = "#EF4444", Position = 4, IsDefault = false }
            };
        }

        private async Task<bool> CurrentUserHasAdminAccessAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return false;
            }

            return await _context.Users
                .AsNoTracking()
                .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
                .SelectMany(user => user.UserRoles.Select(ur => ur.Role.Name))
                .AnyAsync(role => AdminAccessRoles.Contains(role.Trim().ToLowerInvariant()));
        }

        private static bool RequiresAdminAccess(string group)
        {
            var normalized = (group ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is
                "themesettings" or
                "tenantprofile" or
                "contactdiscovery" or
                "admindefaults";
        }
    }
}
