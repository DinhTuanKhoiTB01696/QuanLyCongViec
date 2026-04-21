using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ApplicationDbContext _context;

        public ProjectsController(IProjectService projectService, ApplicationDbContext context)
        {
            _projectService = projectService;
            _context = context;
        }

        private static Dictionary<string, object?> ParseNavigationConfig(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Dictionary<string, object?>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(raw) ?? new Dictionary<string, object?>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, object?>();
            }
        }

        private static bool ReadFavoriteFlag(string? raw)
        {
            var config = ParseNavigationConfig(raw);
            if (!config.TryGetValue("favorite", out var value) || value == null) return false;

            return value switch
            {
                bool boolValue => boolValue,
                JsonElement json when json.ValueKind == JsonValueKind.True => true,
                JsonElement json when json.ValueKind == JsonValueKind.False => false,
                JsonElement json when json.ValueKind == JsonValueKind.String && bool.TryParse(json.GetString(), out var parsed) => parsed,
                string text when bool.TryParse(text, out var parsed) => parsed,
                _ => false
            };
        }

        private static string MergeFavoriteIntoNavigationConfig(string? raw, bool favorite)
        {
            var config = ParseNavigationConfig(raw);
            config["favorite"] = favorite;
            return JsonSerializer.Serialize(config);
        }

        /// <summary>
        /// 5.2 Lấy danh sách dự án - chống N+1 Query Problem
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllAsync();
            var favoriteMap = await _context.Projects
                .AsNoTracking()
                .Where(project => projects.Select(p => p.Id).Contains(project.Id))
                .ToDictionaryAsync(project => project.Id, project => ReadFavoriteFlag(project.NavigationConfig));

            return Ok(ApiResponse<object>.Success(projects.Select(project => new
            {
                project.Id,
                project.Name,
                project.Key,
                project.Description,
                project.StartDate,
                project.EndDate,
                project.Status,
                project.CreatorName,
                project.DepartmentId,
                project.DepartmentName,
                project.ActiveMemberCount,
                project.NetworkType,
                project.Cover,
                project.Icon,
                project.LeadUserId,
                project.LeadName,
                project.CreatedAt,
                project.UpdatedAt,
                IsFavorite = favoriteMap.TryGetValue(project.Id, out var favorite) && favorite
            }).ToList()));
        }

        /// <summary>
        /// Returns ALL active projects with IsMember flag per current user.
        /// Dashboard uses this to show "Tham gia" (Join) for non-member projects.
        /// </summary>
        [HttpGet("discovery")]
        public async Task<IActionResult> GetAllForDiscovery()
        {
            var projects = await _projectService.GetAllForDiscoveryAsync();
            var favoriteMap = await _context.Projects
                .AsNoTracking()
                .Where(project => projects.Select(p => p.Id).Contains(project.Id))
                .ToDictionaryAsync(project => project.Id, project => ReadFavoriteFlag(project.NavigationConfig));

            return Ok(ApiResponse<object>.Success(projects.Select(project => new
            {
                project.Id,
                project.Name,
                project.Key,
                project.Description,
                project.StartDate,
                project.EndDate,
                project.Status,
                project.CreatorName,
                project.DepartmentId,
                project.DepartmentName,
                project.ActiveMemberCount,
                project.NetworkType,
                project.Cover,
                project.Icon,
                project.LeadUserId,
                project.LeadName,
                project.CreatedAt,
                project.UpdatedAt,
                project.IsMember,
                project.MyRole,
                IsFavorite = favoriteMap.TryGetValue(project.Id, out var favorite) && favorite
            }).ToList()));
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchived()
        {
            try
            {
                var projects = await _projectService.GetArchivedAsync();
                var favoriteMap = await _context.Projects
                    .AsNoTracking()
                    .Where(project => projects.Select(p => p.Id).Contains(project.Id))
                    .ToDictionaryAsync(project => project.Id, project => ReadFavoriteFlag(project.NavigationConfig));

                return Ok(ApiResponse<object>.Success(projects.Select(project => new
                {
                    project.Id,
                    project.Name,
                    project.Key,
                    project.Description,
                    project.StartDate,
                    project.EndDate,
                    project.Status,
                    project.CreatorName,
                    project.DepartmentId,
                    project.DepartmentName,
                    project.ActiveMemberCount,
                    project.NetworkType,
                    project.Cover,
                    project.Icon,
                    project.LeadUserId,
                    project.LeadName,
                    project.CreatedAt,
                    project.UpdatedAt,
                    project.IsMember,
                    project.MyRole,
                    IsFavorite = favoriteMap.TryGetValue(project.Id, out var favorite) && favorite
                }).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
                return NotFound(ApiResponse<object>.Error("Dự án không tồn tại.", 404));

            var rawProject = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return Ok(ApiResponse<object>.Success(new
            {
                project.Id,
                project.Name,
                project.Key,
                project.Description,
                project.StartDate,
                project.EndDate,
                project.Status,
                project.CreatorName,
                project.DepartmentId,
                project.DepartmentName,
                project.ActiveMemberCount,
                project.NetworkType,
                project.Cover,
                project.Icon,
                project.LeadUserId,
                project.LeadName,
                project.CreatedAt,
                project.UpdatedAt,
                IsFavorite = ReadFavoriteFlag(rawProject?.NavigationConfig)
            }));
        }

        [HttpPut("{id:guid}/favorite")]
        public async Task<IActionResult> UpdateFavorite(Guid id, [FromBody] JsonElement payload)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (project == null)
                return NotFound(ApiResponse<object>.Error("Project not found.", 404));

            var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status);
            if (!isMember)
                return StatusCode(403, ApiResponse<object>.Error("Forbidden.", 403));

            var favorite = true;
            if (payload.ValueKind == JsonValueKind.Object &&
                payload.TryGetProperty("favorite", out var favoriteProperty) &&
                (favoriteProperty.ValueKind == JsonValueKind.True || favoriteProperty.ValueKind == JsonValueKind.False))
            {
                favorite = favoriteProperty.GetBoolean();
            }

            project.NavigationConfig = MergeFavoriteIntoNavigationConfig(project.NavigationConfig, favorite);
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success(new { id = project.Id, favorite }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid creatorId))
                    return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

                var result = await _projectService.CreateAsync(creatorId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    ApiResponse<ProjectResponseDto>.Created(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        public class CreateCommentRequest {
            public Guid WorkTaskId { get; set; }
            public string Content { get; set; } = string.Empty;
            public Guid? ParentCommentId { get; set; }
        }

        [HttpPost("{id:guid}/Comments")]
        public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest request, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

            // Vá lỗi IDOR: Đảm bảo WorkTaskId này thuộc về đúng projectId trên tham số URL
            var taskNode = await context.WorkTasks.FirstOrDefaultAsync(wt => wt.Id == request.WorkTaskId && !wt.IsDeleted);
            if (taskNode == null || taskNode.ProjectId != id)
            {
                return StatusCode(403, ApiResponse<object>.Error("Forbidden: Việc cần làm không tồn tại hoặc không thuộc dự án này."));
            }

            var comment = new TaskManagement.Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                WorkTaskId = request.WorkTaskId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var user = await context.Users.FindAsync(userId);
            return Ok(new { statusCode = 200, message = "Success", data = new {
                comment.Id,
                comment.Content,
                comment.ParentCommentId,
                comment.CreatedAt,
                UserId = userId,
                FullName = user?.FullName ?? user?.Email,
                Avatar = user?.FullName != null ? user.FullName.Substring(0, 1) : "U"
            }});
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var result = await _projectService.UpdateAsync(id, dto);
                return Ok(ApiResponse<ProjectResponseDto>.Success(result, "Cập nhật thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// 5.1 Archive: Vô hiệu hóa dự án
        /// </summary>
        [HttpPut("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            try
            {
                await _projectService.ArchiveAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Dự án đã được vô hiệu hóa."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                await _projectService.RestoreAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Dự án đã được khôi phục."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// 5.1 Soft Delete
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                await _projectService.SoftDeleteAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Dự án đã được xóa."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            try
            {
                var members = await _projectService.GetMembersAsync(id);
                return Ok(ApiResponse<List<ProjectMemberResponseDto>>.Success(members));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpGet("{id:guid}/work-items")]
        public async Task<IActionResult> GetProjectWorkItems(Guid id, [FromQuery] string? search)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

            var isMember = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == userId && pm.Status);
            if (!isMember)
                return StatusCode(403, ApiResponse<object>.Error("Forbidden.", 403));

            var query = _context.WorkTasks
                .AsNoTracking()
                .Include(wt => wt.TaskStatus)
                .Where(wt => wt.ProjectId == id && !wt.IsDeleted && !wt.IsArchived);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(wt => wt.Title.ToLower().Contains(keyword)
                    || (wt.Description != null && wt.Description.ToLower().Contains(keyword)));
            }

            var tasks = await query
                .OrderByDescending(wt => wt.UpdatedAt)
                .Take(50)
                .Select(wt => new
                {
                    wt.Id,
                    wt.ProjectId,
                    wt.Title,
                    wt.SequenceId,
                    wt.Priority,
                    wt.UpdatedAt,
                    wt.CreatedAt,
                    StatusName = wt.TaskStatus.Name
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Success(tasks));
        }

        [HttpGet("/api/worktasks")]
        public async Task<IActionResult> SearchWorkTasks([FromQuery] string? search)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

            var userProjectIds = await _context.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.UserId == userId && pm.Status)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            var query = _context.WorkTasks
                .AsNoTracking()
                .Include(wt => wt.Project)
                .Include(wt => wt.TaskStatus)
                .Where(wt => userProjectIds.Contains(wt.ProjectId) && !wt.IsDeleted && !wt.IsArchived);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(wt => wt.Title.ToLower().Contains(keyword)
                    || (wt.Description != null && wt.Description.ToLower().Contains(keyword)));
            }

            var results = await query
                .OrderByDescending(wt => wt.UpdatedAt)
                .Take(20)
                .Select(wt => new
                {
                    wt.Id,
                    wt.ProjectId,
                    ProjectName = wt.Project.Name,
                    wt.Title,
                    wt.SequenceId,
                    wt.Priority,
                    wt.UpdatedAt,
                    StatusName = wt.TaskStatus.Name
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Success(results));
        }
    }
}
