using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Admin;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [SystemAuthorize(roles: "SuperAdmin, Admin, Developer, DEV")]
    public class AdminUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AdminUsersController(
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? search)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Where(u => !u.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(keyword) ||
                        u.Email.ToLower().Contains(keyword));
                }

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .ThenBy(u => u.Id)
                    .Select(u => new
                    {
                        id = u.Id,
                        name = string.IsNullOrEmpty(u.FullName) ? u.Email.Split('@')[0] : u.FullName,
                        email = u.Email,
                        isActive = u.IsActive,
                        status = u.IsActive
                            ? "Active"
                            : string.IsNullOrEmpty(u.PasswordHash) ? "Invited" : "Suspended",
                        avatar = u.AvatarUrl,
                        roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                        createdAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { statusCode = 200, message = "Success", data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AddUserRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { statusCode = 400, message = "Invalid user invitation data." });
            }

            try
            {
                var email = request.Email.Trim().ToLowerInvariant();
                var roleName = NormalizeSystemRole(request.Role);
                var projectRole = NormalizeProjectRole(request.ProjectRole);
                var now = DateTime.UtcNow;
                var inviterName = GetInviterName();

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user?.IsDeleted == true)
                {
                    return BadRequest(new { statusCode = 400, message = "This email belongs to a deleted account." });
                }

                var createdPendingUser = false;
                if (user == null)
                {
                    createdPendingUser = true;
                    user = new TaskManagement.Domain.Entities.User
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        FullName = BuildNameFromEmail(email),
                        PasswordHash = string.Empty,
                        IsActive = false,
                        IsDeleted = false,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    _context.Users.Add(user);
                }
                else
                {
                    if (!user.IsActive && !string.IsNullOrEmpty(user.PasswordHash))
                    {
                        return BadRequest(new { statusCode = 400, message = "This account is suspended. Reactivate it before inviting again." });
                    }

                    user.UpdatedAt = now;
                }

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new TaskManagement.Domain.Entities.Role
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        Description = $"{roleName} access"
                    };
                    _context.Roles.Add(role);
                }

                var hasRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
                if (!hasRole)
                {
                    _context.UserRoles.Add(new TaskManagement.Domain.Entities.UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }
                
                if (request.InviteGroups != null && request.InviteGroups.Any())
                {
                    foreach (var groupRef in request.InviteGroups)
                    {
                        var groupRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == groupRef);
                        if (groupRole == null)
                        {
                            groupRole = new TaskManagement.Domain.Entities.Role
                            {
                                Id = Guid.NewGuid(),
                                Name = groupRef,
                                Description = $"{groupRef} access"
                            };
                            _context.Roles.Add(groupRole);
                        }
                        var hasGrpRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == groupRole.Id);
                        if (!hasGrpRole)
                        {
                            _context.UserRoles.Add(new TaskManagement.Domain.Entities.UserRole
                            {
                                UserId = user.Id,
                                RoleId = groupRole.Id
                            });
                        }
                    }
                }

                if (request.ProjectId.HasValue)
                {
                    var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value);
                    if (project == null)
                    {
                        return NotFound(new { statusCode = 404, message = "Project does not exist." });
                    }

                    var projectMember = await _context.ProjectMembers.FirstOrDefaultAsync(pm =>
                        pm.ProjectId == request.ProjectId.Value && pm.UserId == user.Id);

                    if (projectMember == null)
                    {
                        _context.ProjectMembers.Add(new TaskManagement.Domain.Entities.ProjectMember
                        {
                            ProjectId = request.ProjectId.Value,
                            UserId = user.Id,
                            ProjectRole = projectRole,
                            JoinedAt = now,
                            Status = user.IsActive
                        });
                    }
                    else
                    {
                        projectMember.ProjectRole = projectRole;
                        projectMember.LeftAt = null;
                        if (user.IsActive)
                        {
                            projectMember.Status = true;
                        }
                    }
                }

                var activeInviteTokens = await _context.RefreshTokens
                    .Where(token => token.UserId == user.Id &&
                                    token.DeviceId == "Invite" &&
                                    !token.IsRevoked)
                    .ToListAsync();

                foreach (var inviteToken in activeInviteTokens)
                {
                    inviteToken.IsRevoked = true;
                }

                var rawInviteToken = GenerateInviteToken();
                var inviteTokenHash = HashToken(rawInviteToken);
                _context.RefreshTokens.Add(new TaskManagement.Domain.Entities.RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = inviteTokenHash,
                    DeviceId = "Invite",
                    ExpiryTime = now.AddDays(7),
                    IsRevoked = false
                });

                await _context.SaveChangesAsync();

                var projectName = request.ProjectId.HasValue
                    ? await _context.Projects
                        .Where(project => project.Id == request.ProjectId.Value)
                        .Select(project => project.Name)
                        .FirstOrDefaultAsync()
                    : null;

                var acceptUrl = BuildInviteUrl(rawInviteToken);
                await _emailService.SendInviteEmailAsync(
                    email,
                    user.FullName,
                    inviterName,
                    "SprintA",
                    projectName,
                    acceptUrl,
                    request.InviteMessage);

                var message = createdPendingUser
                    ? "Invitation email sent. The user will appear as Invited until they finish onboarding."
                    : "Invitation email sent and user access updated.";

                return Ok(new
                {
                    statusCode = 200,
                    message,
                    data = new
                    {
                        userId = user.Id,
                        user.Email,
                        role = roleName,
                        status = user.IsActive ? "Active" : "Invited"
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.GetBaseException().Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpPut("{userId}/suspend")]
        public async Task<IActionResult> SuspendUser(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { statusCode = 404, message = "User not found." });
                }

                user.IsActive = false;
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.UpdatedAt = DateTime.UtcNow;

                var activeTokens = await _context.RefreshTokens
                    .Where(token => token.UserId == userId && !token.IsRevoked)
                    .ToListAsync();

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { statusCode = 200, message = "User suspended successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveUser(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new { statusCode = 404, message = "User not found." });
                }

                user.IsDeleted = true;
                user.IsActive = false;
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.UpdatedAt = DateTime.UtcNow;

                var activeTokens = await _context.RefreshTokens
                    .Where(token => token.UserId == userId && !token.IsRevoked)
                    .ToListAsync();
                foreach (var token in activeTokens) token.IsRevoked = true;

                await _context.SaveChangesAsync();

                return Ok(new { statusCode = 200, message = "User removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        private static string NormalizeSystemRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role) || role.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return "Developer";
            }

            return role.Trim();
        }

        private static string NormalizeProjectRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role) || role.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return "DEV";
            }

            return role.Trim();
        }

        private static string BuildNameFromEmail(string email)
        {
            var localPart = email.Split('@')[0];
            var words = localPart
                .Split(new[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..]);

            return string.Join(' ', words);
        }

        private string GetInviterName()
        {
            return User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? "SprintA admin";
        }

        private string BuildInviteUrl(string rawInviteToken)
        {
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            return $"{frontendBaseUrl.TrimEnd('/')}/accept-invite?token={Uri.EscapeDataString(rawInviteToken)}";
        }

        private static string GenerateInviteToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(48);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-", StringComparison.Ordinal)
                .Replace("/", "_", StringComparison.Ordinal)
                .TrimEnd('=');
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }
    }
}
