using Microsoft.AspNetCore.Mvc;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("seed")]
        public async Task<IActionResult> Seed()
        {
            try
            {
                await _context.Database.MigrateAsync();
                await DatabaseSeeder.SeedAsync(_context);
                return Ok(new { 
                    message = "Seeding completed successfully!", 
                    testUser = "user1@test.com", 
                    testPassword = "Password123" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Seeding failed", 
                    details = ex.Message,
                    inner = ex.InnerException?.Message 
                });
            }
        }

        [HttpGet("fix-access")]
        public async Task<IActionResult> FixAccess()
        {
            try
            {
                // Force activate all memberships
                await _context.Database.ExecuteSqlRawAsync("UPDATE ProjectMembers SET Status = 1");
                
                // Ensure sample project is NOT deleted and is active
                await _context.Database.ExecuteSqlRawAsync("UPDATE Projects SET IsDeleted = 0, Status = 1");
                
                var userCount = await _context.Users.CountAsync();
                var projectCount = await _context.Projects.CountAsync();
                var memberCount = await _context.ProjectMembers.CountAsync();
                
                return Ok(new { 
                    message = "All projects and memberships set to active!",
                    stats = new { users = userCount, projects = projectCount, members = memberCount }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fix failed", details = ex.Message });
            }
        }

        [HttpGet("my-memberships")]
        public async Task<IActionResult> GetMyMemberships()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("No ID in token");
            
            if (!Guid.TryParse(userIdString, out Guid userId)) return BadRequest("Invalid ID format in token");

            var memberships = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => new { pm.ProjectId, pm.UserId, pm.Status })
                .ToListAsync();
                
            return Ok(new { 
                tokenUserId = userIdString, 
                membershipCount = memberships.Count,
                memberships 
            });
        }

        [HttpGet("force-user1")]
        public async Task<IActionResult> ForceUser1Access()
        {
            try
            {
                var user1Id = Guid.Parse("11111111-0000-0000-0000-000000000001");
                var project = await _context.Projects.FirstOrDefaultAsync();
                
                if (project == null) return BadRequest("No projects found to link to.");

                var existing = await _context.ProjectMembers.FindAsync(project.Id, user1Id);
                if (existing == null)
                {
                    _context.ProjectMembers.Add(new ProjectMember {
                        ProjectId = project.Id,
                        UserId = user1Id,
                        ProjectRole = "PM",
                        Status = true,
                        JoinedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.Status = true;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"User1 linked to project '{project.Name}' ({project.Id}) successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Force failed", details = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userCount = await _context.Users.CountAsync();
            var projectCount = await _context.Projects.CountAsync();
            return Ok(new { 
                database = "Connected", 
                users = userCount, 
                projects = projectCount,
                canLogin = userCount > 0
            });
        }

        [HttpGet("join-sample")]
        public async Task<IActionResult> JoinSampleProject()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized("No ID in token");
                if (!Guid.TryParse(userIdString, out Guid userId)) return BadRequest("Invalid ID format in token");

                var project = await _context.Projects.FirstOrDefaultAsync();
                if (project == null) return BadRequest("No projects found to join.");

                var existing = await _context.ProjectMembers.FindAsync(project.Id, userId);
                if (existing == null)
                {
                    _context.ProjectMembers.Add(new ProjectMember {
                        ProjectId = project.Id,
                        UserId = userId,
                        ProjectRole = "Member",
                        Status = true,
                        JoinedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                else if (!existing.Status)
                {
                    existing.Status = true;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Successfully joined project '{project.Name}'" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Join failed", details = ex.Message });
            }
        }
    }
}
