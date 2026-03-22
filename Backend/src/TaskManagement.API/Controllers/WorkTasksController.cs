using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Constants;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")]
    public class WorkTasksController : ControllerBase
    {
        private readonly IWorkTaskService _workTaskService;

        public WorkTasksController(IWorkTaskService workTaskService)
        {
            _workTaskService = workTaskService;
        }

        [HttpGet]
        [ProjectAuthorize("Member,Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<ActionResult<IEnumerable<WorkTaskDto>>> GetTasksByProject(Guid projectId)
        {
            var tasks = await _workTaskService.GetTasksByProjectAsync(projectId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        [ProjectAuthorize("Member,Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<ActionResult<WorkTaskDto>> GetTaskById(Guid projectId, Guid id)
        {
            var task = await _workTaskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            
            // Security check: ensure task belongs to project
            if (task.ProjectId != projectId) return Forbidden();
            
            return Ok(task);
        }

        [HttpPost]
        [ProjectAuthorize("Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<ActionResult<WorkTaskDto>> CreateTask(Guid projectId, CreateWorkTaskDto dto)
        {
            if (dto.ProjectId != projectId) return BadRequest("Project ID mismatch");
            
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _workTaskService.CreateTaskAsync(userId, dto);
            return CreatedAtAction(nameof(GetTaskById), new { projectId = projectId, id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ProjectAuthorize("Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<ActionResult<WorkTaskDto>> UpdateTask(Guid projectId, Guid id, UpdateWorkTaskDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var result = await _workTaskService.UpdateTaskAsync(userId, id, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("Conflict"))
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPatch("{id}/move")]
        [ProjectAuthorize("Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<ActionResult<WorkTaskDto>> MoveTask(Guid projectId, Guid id, MoveTaskDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var result = await _workTaskService.MoveTaskAsync(userId, id, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("Conflict"))
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProjectAuthorize("Admin,PM,PROJECT_MANAGER")]
        public async Task<IActionResult> DeleteTask(Guid projectId, Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _workTaskService.DeleteTaskAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        private ActionResult Forbidden() => StatusCode(403, new { message = "Access denied to this task." });
    }
}
