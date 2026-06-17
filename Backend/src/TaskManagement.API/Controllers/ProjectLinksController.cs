using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/workspaces/{workspaceId}/projects/{projectId}/links")]
    [Authorize]
    public class ProjectLinksController : ControllerBase
    {
        private readonly IProjectLinkService _projectLinkService;

        public ProjectLinksController(IProjectLinkService projectLinkService)
        {
            _projectLinkService = projectLinkService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid workspaceId, Guid projectId)
        {
            var result = await _projectLinkService.GetAllLinksAsync(projectId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid workspaceId, Guid projectId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _projectLinkService.CreateLinkAsync(userId, projectId, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid workspaceId, Guid projectId, Guid id)
        {
            await _projectLinkService.DeleteLinkAsync(id);
            return NoContent();
        }
    }
}
