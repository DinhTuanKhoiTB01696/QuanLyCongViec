using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/permissions/catalog")]
    [Authorize]
    public class PermissionCatalogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] AiActionPermissions =
        {
            "ai.actions.execute",
            "ai.actions.create_project",
            "ai.actions.create_task",
            "ai.actions.update_task_status",
            "ai.actions.assign_task",
            "ai.actions.create_cycle",
            "ai.actions.create_goal",
            "ai.tools.read"
        };

        public PermissionCatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalog()
        {
            var permissions = await _context.Permissions
                .AsNoTracking()
                .OrderBy(item => item.Module)
                .ThenBy(item => item.Code)
                .Select(item => new
                {
                    item.Id,
                    item.Code,
                    item.Module,
                    item.RiskLevel,
                    item.IsDefault,
                    item.Description,
                    source = "database"
                })
                .ToListAsync();

            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Badge,
                    item.IsProtected,
                    permissions = item.RolePermissions
                        .Select(rolePermission => rolePermission.Permission.Code)
                        .OrderBy(code => code)
                        .ToList()
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Success(new
            {
                permissions,
                roles,
                functionCatalog = new
                {
                    aiActions = AiActionPermissions.Select(code => new
                    {
                        code,
                        module = "ai",
                        guard = "Require authenticated user plus project/workspace membership checks at execution time."
                    }),
                    projectCover = new
                    {
                        code = "projects.cover.update",
                        module = "projects",
                        guard = "Workspace owner/admin, project manager/admin/lead, or system override role."
                    },
                    taskContingencyPlan = new
                    {
                        code = "tasks.contingency.manage",
                        module = "tasks",
                        guard = "Project role/reporting/assignment checks with support-person membership validation."
                    }
                }
            }));
        }
    }
}
