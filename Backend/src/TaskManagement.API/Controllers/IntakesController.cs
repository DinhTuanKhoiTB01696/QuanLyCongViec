using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Text.Json;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    [Authorize]
    public class IntakesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IntakesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("intakes")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var intakes = await _context.Intakes
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId)
                .Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.Description,
                    i.Source,
                    i.Status,
                    i.Priority,
                    i.DesiredDueDate,
                    SubmittedByName = i.SubmittedBy != null ? i.SubmittedBy.FullName : null,
                    ReviewedByName = i.ReviewedBy != null ? i.ReviewedBy.FullName : null,
                    i.CreatedIssueId,
                    i.CreatedAt,
                    i.ReviewedAt
                })
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Success", data = intakes });
        }

        [HttpPost("intakes")]
        public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateIntakeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? parsedUserId = Guid.TryParse(userId, out Guid uid) ? uid : null;

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
                return BadRequest(new { statusCode = 400, message = "Dự án không tồn tại." });

            var intake = new Intake
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title,
                Description = request.Description,
                Source = request.Source ?? "MANUAL",
                Status = "Pending",
                Priority = request.Priority == 0 ? 3 : request.Priority,
                DesiredDueDate = request.DesiredDueDate,
                SubmittedById = parsedUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Intakes.Add(intake);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByProject), new { projectId },
                new { statusCode = 201, message = "Gửi yêu cầu thành công.", data = new { intake.Id } });
        }

        /// <summary>
        /// PM/PO duyệt hoặc từ chối yêu cầu intake
        /// </summary>
        [HttpPut("intakes/{intakeId}/review")]
        public async Task<IActionResult> Review(Guid projectId, Guid intakeId, [FromBody] ReviewIntakeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var intake = await _context.Intakes.FirstOrDefaultAsync(i => i.Id == intakeId && i.ProjectId == projectId);
            if (intake == null)
                return NotFound(new { statusCode = 404, message = "Yêu cầu không tồn tại." });

            if (intake.Status != "Pending")
                return BadRequest(new { statusCode = 400, message = "Yêu cầu đã được xử lý trước đó." });

            intake.Status = request.Status;
            intake.ReviewedById = parsedUserId;
            intake.ReviewedAt = DateTime.UtcNow;

            // If accepted, auto-create a WorkTask from the intake
            if (request.Status == "Accepted")
            {
                var project = await _context.Projects.FirstAsync(p => p.Id == projectId);
                project.IssueSequence += 1;
                string sequenceId = $"{project.Identifier}-{project.IssueSequence}";

                var todoStatus = await _context.TaskStatuses
                    .FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name == "TO DO");
                var defaultType = await _context.TaskTypes
                    .FirstOrDefaultAsync(tt => tt.ProjectId == projectId);

                double maxSort = await _context.WorkTasks
                    .Where(wt => wt.ProjectId == projectId && !wt.IsDeleted)
                    .MaxAsync(wt => (double?)wt.SortOrder) ?? 0;

                var newTask = new WorkTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    WorkspaceId = project.WorkspaceId,
                    Title = intake.Title,
                    Description = intake.Description,
                    TaskStatusId = todoStatus?.Id ?? Guid.Empty,
                    TaskTypeId = defaultType?.Id ?? Guid.Empty,
                    ReporterId = intake.SubmittedById ?? parsedUserId,
                    Priority = intake.Priority,
                    DueDate = intake.DesiredDueDate,
                    SortOrder = maxSort + 65536,
                    SequenceId = sequenceId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.WorkTasks.Add(newTask);
                intake.CreatedIssueId = newTask.Id;
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = $"Yêu cầu đã được {(request.Status == "Accepted" ? "duyệt" : "từ chối")}." });
        }
    }

    public class CreateIntakeRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Source { get; set; }
        public int Priority { get; set; } = 3;
        public DateTime? DesiredDueDate { get; set; }
    }

    public class ReviewIntakeRequest
    {
        public string Status { get; set; } = "Accepted";
    }
}
