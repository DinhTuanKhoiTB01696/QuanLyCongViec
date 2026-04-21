using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly string[] FullAccessRoles =
        {
            "superadmin",
            "admin",
            "system admin",
            "organization admin",
            "accessadmin",
            "access admin"
        };

        public ProjectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private static string BuildProjectUiConfig(string? existingConfig, string? cover, string? icon)
        {
            var config = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(existingConfig))
            {
                config["navigationConfig"] = existingConfig;
            }

            if (!string.IsNullOrWhiteSpace(cover))
            {
                config["cover"] = cover;
            }

            if (!string.IsNullOrWhiteSpace(icon))
            {
                config["icon"] = icon;
            }

            return JsonSerializer.Serialize(config);
        }

        private static void ApplyProjectUiConfig(List<ProjectResponseDto> projects)
        {
            foreach (var project in projects)
            {
                ApplyProjectUiConfig(project);
            }
        }

        private static void ApplyProjectUiConfig(List<ProjectDiscoveryDto> projects)
        {
            foreach (var project in projects)
            {
                ApplyProjectUiConfig(project);
            }
        }

        private static void ApplyProjectUiConfig(ProjectResponseDto project)
        {
            var config = ParseProjectUiConfig(project.Cover);
            project.Cover = config.Cover;
            project.Icon = config.Icon;
        }

        private static void ApplyProjectUiConfig(ProjectDiscoveryDto project)
        {
            var config = ParseProjectUiConfig(project.Cover);
            project.Cover = config.Cover;
            project.Icon = config.Icon;
        }

        private static (string? Cover, string? Icon) ParseProjectUiConfig(string? rawConfig)
        {
            if (string.IsNullOrWhiteSpace(rawConfig))
            {
                return (null, null);
            }

            try
            {
                using var document = JsonDocument.Parse(rawConfig);
                var root = document.RootElement;
                var cover = root.TryGetProperty("cover", out var coverElement) ? coverElement.GetString() : null;
                var icon = root.TryGetProperty("icon", out var iconElement) ? iconElement.GetString() : null;
                return (cover, icon);
            }
            catch (JsonException)
            {
                return (null, null);
            }
        }

        /// <summary>
        /// 5.2 Chống N+1: Dùng .Include().Select() gom data trong 1 câu SQL
        /// </summary>
        public async Task<List<ProjectResponseDto>> GetAllAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return new List<ProjectResponseDto>();
            }

            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status))
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Key = p.Identifier,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status == true),
                    NetworkType = p.NetworkType,
                    LeadUserId = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => (Guid?)pm.UserId)
                        .FirstOrDefault(),
                    LeadName = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => pm.User.FullName)
                        .FirstOrDefault(),
                    Cover = p.NavigationConfig,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            ApplyProjectUiConfig(projects);
            return projects;
        }

        /// <summary>
        /// Returns ALL active projects with a flag indicating whether the current user is a member.
        /// Used by Dashboard/ManageSpaces to show "Tham gia" for non-member projects.
        /// </summary>
        public async Task<List<ProjectDiscoveryDto>> GetAllForDiscoveryAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return new List<ProjectDiscoveryDto>();
            }

            var normalizedRoles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name.Trim().ToLower())
                .ToListAsync();

            var canAccessAllProjects = normalizedRoles.Any(role => FullAccessRoles.Contains(role));

            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && !p.IsArchived && p.Status);

            if (!canAccessAllProjects)
            {
                var assignedProjectIds = await _context.ProjectMembers
                    .AsNoTracking()
                    .Where(pm => pm.UserId == userId && pm.Status)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                query = query.Where(p => assignedProjectIds.Contains(p.Id));
            }

            var projects = await query
                .Select(p => new ProjectDiscoveryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Key = p.Identifier,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status == true),
                    NetworkType = p.NetworkType,
                    LeadUserId = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => (Guid?)pm.UserId)
                        .FirstOrDefault(),
                    LeadName = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => pm.User.FullName)
                        .FirstOrDefault(),
                    Cover = p.NavigationConfig,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsMember = canAccessAllProjects || p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status),
                    MyRole = p.ProjectMembers
                        .Where(pm => pm.UserId == userId && pm.Status)
                        .Select(pm => pm.ProjectRole)
                        .FirstOrDefault()
                })
                .ToListAsync();

            ApplyProjectUiConfig(projects);
            return projects;
        }

        public async Task<List<ProjectDiscoveryDto>> GetArchivedAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return new List<ProjectDiscoveryDto>();
            }

            var normalizedRoles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name.Trim().ToLower())
                .ToListAsync();

            var canAccessAllProjects = normalizedRoles.Any(role => FullAccessRoles.Contains(role));

            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && p.IsArchived);

            if (!canAccessAllProjects)
            {
                var assignedProjectIds = await _context.ProjectMembers
                    .AsNoTracking()
                    .Where(pm => pm.UserId == userId && pm.Status)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                query = query.Where(p => assignedProjectIds.Contains(p.Id));
            }

            var projects = await query
                .Select(p => new ProjectDiscoveryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsMember = canAccessAllProjects || p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status),
                    MyRole = p.ProjectMembers
                        .Where(pm => pm.UserId == userId && pm.Status)
                        .Select(pm => pm.ProjectRole)
                        .FirstOrDefault()
                })
                .ToListAsync();

            ApplyProjectUiConfig(projects);
            return projects;
        }


        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => p.Id == id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Key = p.Identifier,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status == true),
                    NetworkType = p.NetworkType,
                    LeadUserId = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => (Guid?)pm.UserId)
                        .FirstOrDefault(),
                    LeadName = p.ProjectMembers
                        .Where(pm => pm.Status && (pm.ProjectRole == "PROJECT_LEAD" || pm.ProjectRole == "PROJECT_MANAGER"))
                        .OrderBy(pm => pm.ProjectRole == "PROJECT_LEAD" ? 0 : 1)
                        .Select(pm => pm.User.FullName)
                        .FirstOrDefault(),
                    Cover = p.NavigationConfig,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (project != null)
            {
                ApplyProjectUiConfig(project);
            }

            return project;
        }

        public async Task<ProjectResponseDto> CreateAsync(Guid creatorId, CreateProjectDto dto)
        {
            // Validate DepartmentId nếu có
            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value);
                if (!deptExists)
                    throw new ArgumentException("Phòng ban không tồn tại.");
            }

            // Resolve WorkspaceId: user phải thuộc ít nhất 1 workspace
            var workspaceMembership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.UserId == creatorId && wm.IsActive);
            
            Guid workspaceId;
            if (workspaceMembership != null)
            {
                workspaceId = workspaceMembership.WorkspaceId;
            }
            else
            {
                // Auto-create a default workspace for the user if none exists
                var defaultWorkspace = new Workspace
                {
                    Id = Guid.NewGuid(),
                    Slug = "workspace-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = "Default Workspace",
                    OwnerId = creatorId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Workspaces.Add(defaultWorkspace);
                _context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = defaultWorkspace.Id,
                    UserId = creatorId,
                    WorkspaceRole = "OWNER",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
                workspaceId = defaultWorkspace.Id;
            }

            // Generate Identifier: lấy 3-4 ký tự đầu viết hoa từ tên project
            string identifier = string.IsNullOrWhiteSpace(dto.Key) ? GenerateIdentifier(dto.Name) : NormalizeIdentifier(dto.Key);
            // Ensure unique within workspace
            int suffix = 1;
            string originalIdentifier = identifier;
            while (await _context.Projects.AnyAsync(p => p.WorkspaceId == workspaceId && p.Identifier == identifier))
            {
                identifier = originalIdentifier + suffix;
                suffix++;
            }

            string? templateType = null;
            string? navConfig = null;
            if (dto.ProjectTemplateId.HasValue)
            {
                var template = await _context.ProjectTemplates.FirstOrDefaultAsync(t => t.Id == dto.ProjectTemplateId.Value);
                if (template != null)
                {
                    templateType = template.TemplateCode;
                    navConfig = template.DefaultNavigationConfig;
                }
            }

            var networkType = string.Equals(dto.NetworkType, "Private", StringComparison.OrdinalIgnoreCase)
                ? "Private"
                : "Public";

            Guid? leadUserId = dto.LeadUserId;
            if (leadUserId.HasValue)
            {
                var leadIsWorkspaceMember = await _context.WorkspaceMembers
                    .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == leadUserId.Value && wm.IsActive);

                if (!leadIsWorkspaceMember)
                    throw new ArgumentException("Lead phải là thành viên đang hoạt động trong workspace.");
            }

            navConfig = BuildProjectUiConfig(navConfig, dto.Cover, dto.Icon);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Identifier = identifier,
                WorkspaceId = workspaceId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = true,
                CreatorId = creatorId,
                DepartmentId = dto.DepartmentId,
                ProjectTemplateId = dto.ProjectTemplateId,
                TemplateType = templateType,
                NavigationConfig = navConfig,
                NetworkType = networkType,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            // Seed default Task Statuses for the new project
            _context.TaskStatuses.AddRange(
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "TO DO", Position = 1 },
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "IN PROGRESS", Position = 2 },
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "DONE", Position = 3 }
            );

            // Tự động sinh TaskType dựa theo Template
            if (templateType == "IT_SERVICE")
            {
                _context.TaskTypes.AddRange(
                    new TaskManagement.Domain.Entities.TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Ticket lỗi", ColorCode = "#FF0000" },
                    new TaskManagement.Domain.Entities.TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Yêu cầu thiết bị", ColorCode = "#00FF00" }
                );
            }
            else if (templateType == "SOFTWARE_DEV")
            {
                _context.TaskTypes.AddRange(
                    new TaskManagement.Domain.Entities.TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Bug", ColorCode = "#FF0000" },
                    new TaskManagement.Domain.Entities.TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Feature", ColorCode = "#0000FF" }
                );
            }
            else
            {
                _context.TaskTypes.Add(
                    new TaskManagement.Domain.Entities.TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Task", ColorCode = "#3b82f6" }
                );
            }

            // Add the creator as the PROJECT_MANAGER
            var projectMember = new TaskManagement.Domain.Entities.ProjectMember
            {
                ProjectId = project.Id,
                UserId = creatorId,
                ProjectRole = "PROJECT_MANAGER",
                JoinedAt = DateTime.UtcNow,
                Status = true
            };
            _context.ProjectMembers.Add(projectMember);

            if (leadUserId.HasValue && leadUserId.Value != creatorId)
            {
                _context.ProjectMembers.Add(new TaskManagement.Domain.Entities.ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = leadUserId.Value,
                    ProjectRole = "PROJECT_LEAD",
                    JoinedAt = DateTime.UtcNow,
                    Status = true
                });
            }
            else if (leadUserId.HasValue)
            {
                projectMember.ProjectRole = "PROJECT_LEAD";
            }
            
            // Add System Audit Log
            var auditLog = new TaskManagement.Domain.Entities.SystemAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = creatorId,
                Action = "CREATE_PROJECT",
                Resource = $"Project: {project.Name}",
                Status = "Success",
                Details = $"User {creatorId} created project {project.Name}",
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemAuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(project.Id))!;
        }

        public async Task<ProjectResponseDto> UpdateAsync(Guid id, UpdateProjectDto dto)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            // Validate DepartmentId nếu có
            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value);
                if (!deptExists)
                    throw new ArgumentException("Phòng ban không tồn tại.");
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.DepartmentId = dto.DepartmentId;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(project.Id))!;
        }

        /// <summary>
        /// 5.1 Archive: Set Status = false (ẩn khỏi danh sách active, vẫn xem được lịch sử)
        /// </summary>
        public async Task ArchiveAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.IsArchived = true;
            project.Status = false;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.IsArchived = false;
            project.Status = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 5.1 Soft Delete: Set IsDeleted = true, Global Query Filter sẽ tự ẩn
        /// </summary>
        public async Task SoftDeleteAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProjectMemberResponseDto>> GetMembersAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .AsNoTracking()
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId && pm.Status)
                .Select(pm => new ProjectMemberResponseDto
                {
                    UserId = pm.UserId,
                    FullName = pm.User.FullName ?? pm.User.Email,
                    Email = pm.User.Email,
                    ProjectRole = pm.ProjectRole,
                    JoinedAt = pm.JoinedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Generate a short identifier from project name (e.g., "My Project" → "MP")
        /// </summary>
        private static string GenerateIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "PRJ";

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                // Single word: take first 3 chars
                return name.Substring(0, Math.Min(3, name.Length)).ToUpper();
            }

            // Multiple words: take first letter of each word (max 4)
            var initials = string.Concat(words.Take(4).Select(w => char.ToUpper(w[0])));
            return initials;
        }

        private static string NormalizeIdentifier(string key)
        {
            var normalized = new string(key
                .Trim()
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .Take(8)
                .ToArray());

            return string.IsNullOrWhiteSpace(normalized) ? "PRJ" : normalized;
        }
    }
}
