using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// 5.2 Lấy danh sách dự án - chống N+1 Query Problem
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllAsync();
            return Ok(ApiResponse<List<ProjectResponseDto>>.Success(projects));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
                return NotFound(ApiResponse<object>.Error("Dự án không tồn tại.", 404));

            return Ok(ApiResponse<ProjectResponseDto>.Success(project));
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
        }

        [HttpPost("{id}/Comments")]
        public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest request, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized(ApiResponse<object>.Error("Unauthorized.", 401));

            var comment = new TaskManagement.Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                WorkTaskId = request.WorkTaskId,
                Content = request.Content,
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
                comment.CreatedAt,
                UserId = userId,
                FullName = user?.FullName ?? user?.Email,
                Avatar = user?.FullName != null ? user.FullName.Substring(0, 1) : "U"
            }});
        }

        [HttpPut("{id}")]
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
        [HttpPut("{id}/archive")]
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

        [HttpPut("{id}/restore")]
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
        [HttpDelete("{id}")]
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
    }
}
