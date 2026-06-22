using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/workspaces/{workspaceId}/[controller]")]
    [Authorize]
    public class StarredItemsController : ControllerBase
    {
        private readonly IStarredItemService _starredItemService;

        public StarredItemsController(IStarredItemService starredItemService)
        {
            _starredItemService = starredItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid workspaceId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _starredItemService.GetAllAsync(userId, workspaceId);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleStar(Guid workspaceId, [FromQuery] string itemType, [FromQuery] Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _starredItemService.ToggleStarAsync(userId, workspaceId, itemType, itemId);
            return Ok(ApiResponse<object>.Success(result));
        }
    }
}
