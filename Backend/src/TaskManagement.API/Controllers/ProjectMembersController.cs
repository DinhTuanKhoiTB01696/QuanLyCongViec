using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
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

        [HttpGet]
        [ProjectAuthorize("")]
        public async Task<IActionResult> GetProjectMembers(Guid projectId)
        {
            try
            {
                var members = await _projectMemberService.GetProjectMembersAsync(projectId);
                return Ok(new { statusCode = 200, message = "Success", data = members });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost]
        [ProjectAuthorize("PROJECT_MANAGER,PROJECT_LEAD,PM,PO,Admin")]
        public async Task<IActionResult> InviteMember(Guid projectId, [FromBody] ProjectMemberRequestDto request)
        {
            try
            {
                var inviterName = User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue(ClaimTypes.Email)
                    ?? "SprintA admin";

                await _projectMemberService.InviteMemberAsync(projectId, request, inviterName);
                return Ok(new { statusCode = 200, message = "Success", data = "Da gui loi moi tham gia du an qua email." });
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
        [ProjectAuthorize("PROJECT_MANAGER,PROJECT_LEAD,PM,PO,Admin")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            try
            {
                await _projectMemberService.RemoveMemberAsync(projectId, userId);
                return Ok(new { statusCode = 200, message = "Success", data = "Xoa thanh vien thanh cong va da xu ly task mo coi." });
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

        [HttpPut("{userId}/role")]
        [ProjectAuthorize("PROJECT_MANAGER,PROJECT_LEAD,PM,PO,Admin")]
        public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateRoleRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Role))
                {
                    return BadRequest(new { statusCode = 400, message = "Role khong de trong." });
                }

                await _projectMemberService.UpdateMemberRoleAsync(projectId, userId, request.Role);
                return Ok(new { statusCode = 200, message = "Success", data = "Cap nhat role thanh cong." });
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
