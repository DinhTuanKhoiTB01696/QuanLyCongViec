using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;

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

        [HttpPost]
        [ProjectAuthorize("PM, Admin")]
        public async Task<IActionResult> InviteMember(Guid projectId, [FromBody] ProjectMemberRequestDto request)
        {
            try
            {
                await _projectMemberService.InviteMemberAsync(projectId, request);
                return Ok(new { statusCode = 200, message = "Success", data = "Thêm thành viên thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { statusCode = 409, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        [ProjectAuthorize("PM, Admin")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            try
            {
                await _projectMemberService.RemoveMemberAsync(projectId, userId);
                return Ok(new { statusCode = 200, message = "Success", data = "Xóa thành viên thành công và đã xử lý task mồ côi." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Internal server error: " + ex.Message });
            }
        }
    }
}
