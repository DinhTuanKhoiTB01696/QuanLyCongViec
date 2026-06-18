using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/workspaces/{workspaceId}/[controller]")]
    [Authorize]
    public class GoalsController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalsController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid workspaceId)
        {
            var result = await _goalService.GetAllAsync(workspaceId);
            return Ok(result);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses(Guid workspaceId)
        {
            var result = await _goalService.GetStatusesAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid workspaceId, Guid id)
        {
            var result = await _goalService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid workspaceId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.CreateAsync(userId, workspaceId, dto);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var result = await _goalService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid workspaceId, Guid id)
        {
            await _goalService.ArchiveAsync(id);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid workspaceId, Guid id)
        {
            await _goalService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/updates")]
        public async Task<IActionResult> AddUpdate(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddUpdateAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/lessons")]
        public async Task<IActionResult> AddLesson(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddLessonAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/risks")]
        public async Task<IActionResult> AddRisk(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddRiskAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/decisions")]
        public async Task<IActionResult> AddDecision(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddDecisionAsync(id, userId, dto);
            return Ok(result);
        }
    }
}
