using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Application.Common;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ProjectAuthorizeAttribute : TypeFilterAttribute
    {
        public ProjectAuthorizeAttribute(string roles) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class ProjectAuthorizeFilter : IAsyncActionFilter
    {
        private static readonly string[] SystemOverrideRoles =
        {
            "superadmin",
            "admin",
            "system admin",
            "organization admin",
            "accessadmin",
            "access admin"
        };

        private readonly string[] _allowedRoles;
        private readonly ApplicationDbContext _dbContext;

        public ProjectAuthorizeFilter(string roles, ApplicationDbContext dbContext)
        {
            _allowedRoles = roles
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ProjectExecutionRuleHelper.NormalizeProjectRole)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Get UserId from JWT
            var userIdString = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                context.Result = new UnauthorizedObjectResult(new { statusCode = 401, message = "Unauthorized. JWT is missing or invalid." });
                return;
            }

            // 2. Extract ProjectId from Route Data
            // Assumes route is something like /api/projects/{projectId}/...
            var projectIdString =
                context.RouteData.Values["projectId"]?.ToString()
                ?? context.RouteData.Values["id"]?.ToString();
            if (string.IsNullOrEmpty(projectIdString) || !Guid.TryParse(projectIdString, out Guid projectId))
            {
                // Nếu API không cung cấp projectId trong url, chặn ngay lập tức hoặc bỏ qua tuỳ thiết kế.
                // Theo BusinessLogic, các API liên quan đến project BAO GIỜ cũng có projectId
                context.Result = new BadRequestObjectResult(new { statusCode = 400, message = "Missing projectId in route." });
                return;
            }

            var claimRoles = context.HttpContext.User
                .FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value?.Trim().ToLower())
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .ToHashSet();

            var hasSystemOverrideFromClaims = claimRoles.Any(role => SystemOverrideRoles.Contains(role!));

            var hasSystemOverrideFromDatabase = await _dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
                .SelectMany(user => user.UserRoles.Select(ur => ur.Role.Name))
                .AnyAsync(role => SystemOverrideRoles.Contains(role.Trim().ToLower()));

            if (hasSystemOverrideFromClaims || hasSystemOverrideFromDatabase)
            {
                context.HttpContext.Items["ProjectRole"] = "SystemOverride";
                await next();
                return;
            }

            // 3. Query ProjectMember
            var member = await _dbContext.ProjectMembers
                .AsNoTracking() // Optimize since we only read
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            // 4. Validate Membership & Soft Delete Status
            if (member == null || !member.Status) // member.Status = false means soft-deleted based on BusinessLogic.md
            {
                context.Result = new ObjectResult(new { statusCode = 403, message = "Forbidden. You are not an active member of this project." })
                {
                    StatusCode = 403
                };
                return;
            }

            // 5. Check Roles
            var normalizedMemberRole = ProjectExecutionRuleHelper.NormalizeProjectRole(member.ProjectRole);

            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(normalizedMemberRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new { statusCode = 403, message = $"Forbidden. Role '{member.ProjectRole}' is not allowed to perform this action." })
                {
                    StatusCode = 403
                };
                return;
            }

            // 6. Special Case for Data Modification using HTTP methods: POST, PUT, DELETE for Guest/Stakeholder
            var httpMethod = context.HttpContext.Request.Method;
            var isWriteMethod = httpMethod == HttpMethod.Post.Method || 
                                httpMethod == HttpMethod.Put.Method || 
                                httpMethod == HttpMethod.Delete.Method;

            if (isWriteMethod && (member.ProjectRole.Equals("Guest", StringComparison.OrdinalIgnoreCase) || member.ProjectRole.Equals("Stakeholder", StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new ObjectResult(new { statusCode = 403, message = "Forbidden. Guests and Stakeholders cannot modify project data." })
                {
                    StatusCode = 403
                };
                return;
            }

            // Save project member info into HttpContext items for later usage in controllers if needed
            context.HttpContext.Items["ProjectRole"] = member.ProjectRole;

            // Authorized, continue
            await next();
        }
    }
}
