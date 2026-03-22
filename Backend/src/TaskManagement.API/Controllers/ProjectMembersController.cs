using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.API.Filters;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Constants;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/members")]
    public class ProjectMembersController : ControllerBase
    {
        private readonly IProjectMemberService _projectMemberService;

        public ProjectMembersController(IProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers(Guid projectId)
        {
            var members = await _projectMemberService.GetProjectMembersAsync(projectId);
            return Ok(new { statusCode = 200, message = "Success", data = members });
        }

        [HttpDelete("{userId}")]
        [ProjectAuthorize($"{ProjectRoles.PO},{ProjectRoles.PM},{ProjectRoles.SM},{ProjectRoles.TechLead}")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out Guid adminId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Invalid token" });
                }

                // Cannot remove oneself through this API (or handle logically)
                if (adminId == userId)
                {
                    return BadRequest(new { statusCode = 400, message = "Cannot kick yourself. Use leave project API." });
                }

                await _projectMemberService.RemoveMemberAsync(projectId, userId, adminId);

                return Ok(new 
                { 
                    statusCode = 200, 
                    message = "Member removed successfully. Orphan tasks removed and PMs notified." 
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
            catch (Exception ex)
            {
                // In production, should log the exception
                return StatusCode(500, new { statusCode = 500, message = "Internal server error: " + ex.Message });
            }
        }
    }
}
