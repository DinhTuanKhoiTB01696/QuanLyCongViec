using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/WorkTasks/{taskId}/dependencies")]
    [Authorize]
    public class TaskDependenciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskDependenciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDependencies(Guid projectId, Guid taskId)
        {
            var relations = await _context.TaskDependencies
                .Include(td => td.PredecessorTask)
                .ThenInclude(pt => pt.TaskStatus)
                .Include(td => td.SuccessorTask)
                .ThenInclude(st => st.TaskStatus)
                .Where(td => td.PredecessorTaskId == taskId || td.SuccessorTaskId == taskId)
                .Select(td => new {
                    td.PredecessorTaskId,
                    td.SuccessorTaskId,
                    td.DependencyType,
                    PredecessorTitle = td.PredecessorTask.Title,
                    PredecessorSequenceId = td.PredecessorTask.SequenceId,
                    PredecessorStatus = td.PredecessorTask.TaskStatus.Name,
                    SuccessorTitle = td.SuccessorTask.Title,
                    SuccessorSequenceId = td.SuccessorTask.SequenceId,
                    SuccessorStatus = td.SuccessorTask.TaskStatus.Name
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Success", data = relations });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDependency(Guid projectId, Guid taskId, [FromBody] CreateDependencyRequest request)
        {
            var taskExists = await _context.WorkTasks.AnyAsync(wt => wt.Id == taskId && wt.ProjectId == projectId && !wt.IsDeleted);
            var relatedTaskExists = await _context.WorkTasks.AnyAsync(wt => wt.Id == request.RelatedTaskId && wt.ProjectId == projectId && !wt.IsDeleted);

            if (!taskExists || !relatedTaskExists)
                return BadRequest(new { statusCode = 400, message = "Công việc không tồn tại." });

            if (taskId == request.RelatedTaskId)
                return BadRequest(new { statusCode = 400, message = "Không thể tự phụ thuộc vào chính mình." });

            Guid predecessorId = taskId;
            Guid successorId = request.RelatedTaskId;
            int depType = 1; // 1 = Blocks (Predecessor blocks Successor)

            if (request.RelationType == "blocked_by")
            {
                predecessorId = request.RelatedTaskId;
                successorId = taskId;
                depType = 1;
            }
            else if (request.RelationType == "blocks")
            {
                predecessorId = taskId;
                successorId = request.RelatedTaskId;
                depType = 1;
            }
            else if (request.RelationType == "relates_to")
            {
                depType = 2; // 2 = Relates To
            }
            else if (request.RelationType == "duplicate")
            {
                depType = 3; // 3 = Duplicate
            }

            var existing = await _context.TaskDependencies
                .FirstOrDefaultAsync(td => td.PredecessorTaskId == predecessorId && td.SuccessorTaskId == successorId);

            if (existing != null)
            {
                existing.DependencyType = depType;
                await _context.SaveChangesAsync();
                return Ok(new { statusCode = 200, message = "Cập nhật quan hệ thành công." });
            }

            // Dò tìm vòng lặp phụ thuộc (circular dependency chặn đơn giản)
            if (depType == 1)
            {
                var reverse = await _context.TaskDependencies
                    .FirstOrDefaultAsync(td => td.PredecessorTaskId == successorId && td.SuccessorTaskId == predecessorId && td.DependencyType == 1);
                if (reverse != null)
                    return BadRequest(new { statusCode = 400, message = "Gặp vòng lặp chặn (Circular dependency)." });
            }

            var dependency = new TaskDependency
            {
                PredecessorTaskId = predecessorId,
                SuccessorTaskId = successorId,
                DependencyType = depType
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 201, message = "Thêm quan hệ thành công." });
        }

        [HttpDelete("{relatedTaskId}")]
        public async Task<IActionResult> RemoveDependency(Guid projectId, Guid taskId, Guid relatedTaskId)
        {
            var dep = await _context.TaskDependencies
                .FirstOrDefaultAsync(td => 
                    (td.PredecessorTaskId == taskId && td.SuccessorTaskId == relatedTaskId) ||
                    (td.SuccessorTaskId == taskId && td.PredecessorTaskId == relatedTaskId)
                );

            if (dep == null)
                return NotFound(new { statusCode = 404, message = "Không tìm thấy quan hệ." });

            _context.TaskDependencies.Remove(dep);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Xóa quan hệ thành công." });
        }
    }

    public class CreateDependencyRequest
    {
        public Guid RelatedTaskId { get; set; }
        /// <summary> "blocks", "blocked_by", "relates_to", "duplicate" </summary>
        public string RelationType { get; set; } = "relates_to";
    }
}
