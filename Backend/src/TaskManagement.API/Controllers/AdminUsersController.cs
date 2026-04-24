using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Admin;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [SystemAuthorize(roles: "SuperAdmin, Admin, System Admin, Organization Admin, AccessAdmin")]
    public class AdminUsersController : ControllerBase
    {
        private static readonly string[] ProtectedSystemRoles =
        {
            "admin",
            "pm",
            "system admin",
            "superadmin",
            "organization admin",
            "accessadmin",
            "access admin"
        };

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
                    .Include(u => u.DepartmentMemberships)
                    .ThenInclude(dm => dm.Department)
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
                        departments = u.DepartmentMemberships.Select(dm => dm.Department.Name).ToList(),
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

        [HttpGet("roles")]
        public async Task<IActionResult> GetSystemRoles([FromQuery] string? search)
        {
            var query = _context.Roles
                .Include(role => role.UserRoles)
                .Include(role => role.RolePermissions)
                .ThenInclude(link => link.Permission)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLowerInvariant();
                query = query.Where(role =>
                    role.Name.ToLower().Contains(keyword) ||
                    (role.Description != null && role.Description.ToLower().Contains(keyword)));
            }

            var roles = await query
                .OrderBy(role => role.Name)
                .Select(role => new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    isProtected = ProtectedSystemRoles.Contains(role.Name.Trim().ToLower()),
                    memberCount = role.UserRoles.Count,
                    permissions = role.RolePermissions
                        .OrderBy(link => link.Permission.Module)
                        .ThenBy(link => link.Permission.Code)
                        .Select(link => new
                        {
                            id = link.PermissionId,
                            code = link.Permission.Code,
                            module = link.Permission.Module
                        })
                        .ToList()
                })
                .ToListAsync();

            var permissions = await _context.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Module)
                .ThenBy(permission => permission.Code)
                .Select(permission => new
                {
                    id = permission.Id,
                    code = permission.Code,
                    module = permission.Module
                })
                .ToListAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Success",
                data = new
                {
                    roles,
                    permissions
                }
            });
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] SaveRoleRequest request)
        {
            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return BadRequest(new { statusCode = 400, message = "Role name is required." });
            }

            var protectedRole = ProtectedSystemRoles.Contains(normalizedName.ToLowerInvariant());
            if (protectedRole)
            {
                return BadRequest(new { statusCode = 400, message = "Protected roles can only be assigned, not created or edited here." });
            }

            var exists = await _context.Roles.AnyAsync(role => role.Name.ToLower() == normalizedName.ToLower());
            if (exists)
            {
                return Conflict(new { statusCode = 409, message = "Role already exists." });
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
            };

            _context.Roles.Add(role);
            await ReplaceRolePermissionsAsync(role.Id, request.PermissionIds);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 201,
                message = "Role created successfully.",
                data = new { role.Id, role.Name, role.Description }
            });
        }

        [HttpPut("roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] SaveRoleRequest request)
        {
            var role = await _context.Roles
                .Include(item => item.RolePermissions)
                .FirstOrDefaultAsync(item => item.Id == roleId);
            if (role == null)
            {
                return NotFound(new { statusCode = 404, message = "Role not found." });
            }

            if (ProtectedSystemRoles.Contains(role.Name.Trim().ToLowerInvariant()))
            {
                return BadRequest(new { statusCode = 400, message = "Protected roles can only be assigned to users." });
            }

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return BadRequest(new { statusCode = 400, message = "Role name is required." });
            }

            var duplicateExists = await _context.Roles
                .AnyAsync(item => item.Id != roleId && item.Name.ToLower() == normalizedName.ToLower());
            if (duplicateExists)
            {
                return Conflict(new { statusCode = 409, message = "Role already exists." });
            }

            role.Name = normalizedName;
            role.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            await ReplaceRolePermissionsAsync(roleId, request.PermissionIds);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Role updated successfully.",
                data = new { role.Id, role.Name, role.Description }
            });
        }

        [HttpDelete("roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            var role = await _context.Roles
                .Include(item => item.UserRoles)
                .Include(item => item.RolePermissions)
                .FirstOrDefaultAsync(item => item.Id == roleId);
            if (role == null)
            {
                return NotFound(new { statusCode = 404, message = "Role not found." });
            }

            if (ProtectedSystemRoles.Contains(role.Name.Trim().ToLowerInvariant()))
            {
                return BadRequest(new { statusCode = 400, message = "Protected roles can only be assigned to users." });
            }

            if (role.UserRoles.Any())
            {
                return BadRequest(new { statusCode = 400, message = "Remove this role from users before deleting it." });
            }

            if (role.RolePermissions.Any())
            {
                _context.RolePermissions.RemoveRange(role.RolePermissions);
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Role deleted successfully." });
        }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRolesToUser(Guid userId, [FromBody] AssignRolesRequest request)
        {
            var user = await _context.Users
                .Include(item => item.UserRoles)
                .FirstOrDefaultAsync(item => item.Id == userId && !item.IsDeleted);
            if (user == null)
            {
                return NotFound(new { statusCode = 404, message = "User not found." });
            }

            var requestedRoleIds = (request.RoleIds ?? new List<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var roles = await _context.Roles
                .Where(role => requestedRoleIds.Contains(role.Id))
                .ToListAsync();

            if (roles.Count != requestedRoleIds.Count)
            {
                return BadRequest(new { statusCode = 400, message = "One or more roles do not exist." });
            }

            var existingRoleIds = user.UserRoles.Select(item => item.RoleId).ToHashSet();
            foreach (var roleId in existingRoleIds.Except(requestedRoleIds).ToList())
            {
                var link = user.UserRoles.FirstOrDefault(item => item.RoleId == roleId);
                if (link != null)
                {
                    _context.UserRoles.Remove(link);
                }
            }

            foreach (var roleId in requestedRoleIds.Where(id => !existingRoleIds.Contains(id)))
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "User roles updated successfully." });
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.Name,
                    managerId = d.ManagerId,
                    isActive = d.IsActive,
                    require2FA = d.Require2FA,
                    memberCount = d.DepartmentMembers.Count(dm => dm.DepartmentId == d.Id)
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, data = departments });
        }

        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequest request)
        {
            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return BadRequest(new { statusCode = 400, message = "Department name is required." });
            }

            var exists = await _context.Departments.AnyAsync(d => !d.IsDeleted && d.Name.ToLower() == normalizedName.ToLower());
            if (exists)
            {
                return Conflict(new { statusCode = 409, message = "Department already exists." });
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                ManagerId = request.ManagerId,
                Require2FA = request.Require2FA,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Department created successfully.", data = new { id = department.Id } });
        }

        [HttpPut("departments/{departmentId}")]
        public async Task<IActionResult> UpdateDepartment(Guid departmentId, [FromBody] DepartmentRequest request)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);
            if (department == null)
            {
                return NotFound(new { statusCode = 404, message = "Department not found." });
            }

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return BadRequest(new { statusCode = 400, message = "Department name is required." });
            }

            var duplicated = await _context.Departments.AnyAsync(d =>
                d.Id != departmentId &&
                !d.IsDeleted &&
                d.Name.ToLower() == normalizedName.ToLower());

            if (duplicated)
            {
                return Conflict(new { statusCode = 409, message = "Department already exists." });
            }

            department.Name = normalizedName;
            department.ManagerId = request.ManagerId;
            department.Require2FA = request.Require2FA;
            department.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Department updated successfully." });
        }

        [HttpDelete("departments/{departmentId}")]
        public async Task<IActionResult> DeleteDepartment(Guid departmentId)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);
            if (department == null)
            {
                return NotFound(new { statusCode = 404, message = "Department not found." });
            }

            department.IsDeleted = true;
            department.IsActive = false;

            var memberships = await _context.DepartmentMembers
                .Where(dm => dm.DepartmentId == departmentId)
                .ToListAsync();

            if (memberships.Count > 0)
            {
                _context.DepartmentMembers.RemoveRange(memberships);
            }

            var projectRoles = await _context.ProjectDepartmentRoles
                .Where(role => role.DepartmentId == departmentId)
                .ToListAsync();

            if (projectRoles.Count > 0)
            {
                _context.ProjectDepartmentRoles.RemoveRange(projectRoles);
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Department removed successfully." });
        }

        [HttpGet("project-role-assignments")]
        public async Task<IActionResult> GetProjectRoleAssignments()
        {
            var assignments = await _context.ProjectDepartmentRoles
                .Include(item => item.Project)
                .Include(item => item.Department)
                .OrderBy(item => item.Project.Name)
                .ThenBy(item => item.Department.Name)
                .ThenBy(item => item.RoleName)
                .Select(item => new
                {
                    projectId = item.ProjectId,
                    projectName = item.Project.Name,
                    departmentId = item.DepartmentId,
                    departmentName = item.Department.Name,
                    roleName = item.RoleName,
                    assignedAt = item.AssignedAt
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, data = assignments });
        }

        [HttpPut("project-role-assignments")]
        public async Task<IActionResult> UpsertProjectRoleAssignment([FromBody] ProjectDepartmentRoleRequest request)
        {
            if (request.ProjectId == Guid.Empty || request.DepartmentId == Guid.Empty || string.IsNullOrWhiteSpace(request.RoleName))
            {
                return BadRequest(new { statusCode = 400, message = "Project, department, and role are required." });
            }

            var projectExists = await _context.Projects.AnyAsync(project => project.Id == request.ProjectId && !project.IsDeleted);
            if (!projectExists)
            {
                return NotFound(new { statusCode = 404, message = "Project not found." });
            }

            var departmentExists = await _context.Departments.AnyAsync(department => department.Id == request.DepartmentId && !department.IsDeleted);
            if (!departmentExists)
            {
                return NotFound(new { statusCode = 404, message = "Department not found." });
            }

            var normalizedRoleName = request.RoleName.Trim();
            var existing = await _context.ProjectDepartmentRoles.FirstOrDefaultAsync(item =>
                item.ProjectId == request.ProjectId &&
                item.DepartmentId == request.DepartmentId &&
                item.RoleName == normalizedRoleName);

            if (existing == null)
            {
                _context.ProjectDepartmentRoles.Add(new ProjectDepartmentRole
                {
                    ProjectId = request.ProjectId,
                    DepartmentId = request.DepartmentId,
                    RoleName = normalizedRoleName,
                    AssignedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.AssignedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Project role assignment saved successfully." });
        }

        [HttpDelete("project-role-assignments")]
        public async Task<IActionResult> DeleteProjectRoleAssignment([FromBody] ProjectDepartmentRoleRequest request)
        {
            var existing = await _context.ProjectDepartmentRoles.FirstOrDefaultAsync(item =>
                item.ProjectId == request.ProjectId &&
                item.DepartmentId == request.DepartmentId &&
                item.RoleName == request.RoleName);

            if (existing == null)
            {
                return NotFound(new { statusCode = 404, message = "Project role assignment not found." });
            }

            _context.ProjectDepartmentRoles.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Project role assignment removed successfully." });
        }

        public class DepartmentRequest
        {
            public string Name { get; set; } = string.Empty;
            public Guid? ManagerId { get; set; }
            public bool IsActive { get; set; } = true;
            public bool Require2FA { get; set; }
        }

        public class ProjectDepartmentRoleRequest
        {
            public Guid ProjectId { get; set; }
            public Guid DepartmentId { get; set; }
            public string RoleName { get; set; } = string.Empty;
        }

        public class SaveRoleRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<Guid>? PermissionIds { get; set; }
        }

        public class AssignRolesRequest
        {
            public List<Guid>? RoleIds { get; set; }
        }

        private async Task ReplaceRolePermissionsAsync(Guid roleId, List<Guid>? requestedPermissionIds)
        {
            var nextPermissionIds = (requestedPermissionIds ?? new List<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var existingLinks = await _context.RolePermissions
                .Where(link => link.RoleId == roleId)
                .ToListAsync();

            if (existingLinks.Count > 0)
            {
                _context.RolePermissions.RemoveRange(existingLinks);
            }

            if (nextPermissionIds.Count == 0)
            {
                return;
            }

            var validPermissionIds = await _context.Permissions
                .Where(permission => nextPermissionIds.Contains(permission.Id))
                .Select(permission => permission.Id)
                .ToListAsync();

            foreach (var permissionId in validPermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
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
                return "Developer";
            }

            return role.Trim() switch
            {
                "DEV" => "Developer",
                "dev" => "Developer",
                "PROJECT_MANAGER" => "PM",
                "project_manager" => "PM",
                "PROJECT_LEAD" => "Project Lead",
                "project_lead" => "Project Lead",
                "SCRUM_MASTER" => "SM",
                "scrum_master" => "SM",
                _ => role.Trim()
            };
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
