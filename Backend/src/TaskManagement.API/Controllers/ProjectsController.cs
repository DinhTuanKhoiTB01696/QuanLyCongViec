using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Infrastructure.Data;
using System.Security.Claims;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            // Get projects where user is a member
            var projects = await _dbContext.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.Status)
                .Select(pm => pm.Project)
                .Where(p => !p.IsDeleted)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status ? 1 : 0,
                    // Mock some colors/icons for the UI
                    Color = "#ff5c35",
                    Icon = "fa-solid fa-folder"
                })
                .ToListAsync();

            return Ok(projects);
        }
    }
}
