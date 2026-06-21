using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/workspaces")]
    [Authorize]
    public class WorkspacesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public WorkspacesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Lấy tất cả workspaces mà user hiện tại là thành viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var workspaces = await _context.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.UserId == parsedUserId && wm.IsActive)
                .Select(wm => new
                {
                    wm.Workspace.Id,
                    wm.Workspace.Name,
                    wm.Workspace.Slug,
                    wm.Workspace.Logo,
                    wm.Workspace.Timezone,
                    wm.WorkspaceRole,
                    OwnerName = wm.Workspace.Owner.FullName,
                    MemberCount = wm.Workspace.Members.Count(m => m.IsActive),
                    ProjectCount = wm.Workspace.Projects.Count(p => !p.IsDeleted),
                    wm.Workspace.CreatedAt
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Success", data = workspaces });
        }

        /// <summary>
        /// Tạo workspace mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            // Validate slug uniqueness
            var slugExists = await _context.Workspaces.AnyAsync(w => w.Slug == request.Slug);
            if (slugExists)
                return BadRequest(new { statusCode = 400, message = "Slug đã tồn tại. Vui lòng chọn tên khác." });

            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Slug.ToLower().Trim(),
                OwnerId = parsedUserId,
                Timezone = request.Timezone ?? "Asia/Ho_Chi_Minh",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Workspaces.Add(workspace);

            // Auto-add creator as OWNER
            _context.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = parsedUserId,
                WorkspaceRole = "OWNER",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyWorkspaces), null,
                new { statusCode = 201, message = "Tạo workspace thành công.", data = new { workspace.Id, workspace.Name, workspace.Slug } });
        }

        /// <summary>
        /// Lấy thông tin workspace theo slug
        /// </summary>
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var workspace = await _context.Workspaces
                .AsNoTracking()
                .Where(w => w.Slug == slug)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Slug,
                    w.Logo,
                    w.Timezone,
                    OwnerName = w.Owner.FullName,
                    MemberCount = w.Members.Count(m => m.IsActive),
                    ProjectCount = w.Projects.Count(p => !p.IsDeleted),
                    w.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (workspace == null)
                return NotFound(new { statusCode = 404, message = "Workspace không tồn tại." });

            // Check membership
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspace.Id && wm.UserId == parsedUserId && wm.IsActive);

            if (!isMember)
                return StatusCode(403, new { statusCode = 403, message = "Bạn không phải thành viên của workspace này." });

            return Ok(new { statusCode = 200, message = "Success", data = workspace });
        }

        /// <summary>
        /// Thêm thành viên vào workspace
        /// </summary>
        [HttpPost("{workspaceId}/members")]
        public async Task<IActionResult> AddMember(Guid workspaceId, [FromBody] AddWorkspaceMemberRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            // Check requester is OWNER or ADMIN
            var requesterMembership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId);

            if (requesterMembership == null || (requesterMembership.WorkspaceRole != "OWNER" && requesterMembership.WorkspaceRole != "ADMIN"))
                return StatusCode(403, new { statusCode = 403, message = "Bạn không có quyền thêm thành viên." });

            // Check user exists
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (targetUser == null)
                return BadRequest(new { statusCode = 400, message = "Không tìm thấy người dùng với email này." });

            // Check not already member
            var existing = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == targetUser.Id);

            if (existing != null)
            {
                if (existing.IsActive)
                    return BadRequest(new { statusCode = 400, message = "Người dùng đã là thành viên." });
                existing.IsActive = true;
                existing.WorkspaceRole = request.Role ?? "MEMBER";
            }
            else
            {
                _context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspaceId,
                    UserId = targetUser.Id,
                    WorkspaceRole = request.Role ?? "MEMBER",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Thêm thành viên thành công." });
        }

        /// <summary>
        /// Lấy danh sách thành viên workspace
        /// </summary>
        [HttpGet("{workspaceId}/members")]
        public async Task<IActionResult> GetMembers(Guid workspaceId)
        {
            var members = await _context.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.WorkspaceId == workspaceId && wm.IsActive)
                .Select(wm => new
                {
                    wm.UserId,
                    wm.User.FullName,
                    wm.User.Email,
                    wm.User.AvatarUrl,
                    wm.WorkspaceRole,
                    wm.JoinedAt
                })
                .OrderBy(m => m.FullName)
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Success", data = members });
        }

        /// <summary>
        /// Cập nhật thông tin workspace
        /// </summary>
        [HttpPut("{workspaceId}")]
        public async Task<IActionResult> UpdateWorkspace(Guid workspaceId, [FromBody] UpdateWorkspaceRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var membership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId && wm.IsActive);

            if (membership == null || (membership.WorkspaceRole != "OWNER" && membership.WorkspaceRole != "ADMIN"))
                return StatusCode(403, new { statusCode = 403, message = "Bạn không có quyền cập nhật workspace này." });

            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null || workspace.IsDeleted)
                return NotFound(new { statusCode = 404, message = "Workspace không tồn tại." });

            if (!string.IsNullOrEmpty(request.Slug) && request.Slug != workspace.Slug)
            {
                var slugExists = await _context.Workspaces.AnyAsync(w => w.Slug == request.Slug && w.Id != workspaceId);
                if (slugExists) return BadRequest(new { statusCode = 400, message = "Slug đã tồn tại." });
                workspace.Slug = request.Slug.ToLower().Trim();
            }

            if (!string.IsNullOrEmpty(request.Name)) workspace.Name = request.Name;
            if (request.Logo != null) workspace.Logo = request.Logo;
            if (!string.IsNullOrEmpty(request.Timezone)) workspace.Timezone = request.Timezone;

            workspace.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Cập nhật thành công.", data = workspace });
        }

        /// <summary>
        /// Xóa workspace
        /// </summary>
        [HttpDelete("{workspaceId}")]
        public async Task<IActionResult> DeleteWorkspace(Guid workspaceId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var membership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId && wm.IsActive);

            if (membership == null || membership.WorkspaceRole != "OWNER")
                return StatusCode(403, new { statusCode = 403, message = "Chỉ OWNER mới có thể xóa workspace." });

            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                return NotFound(new { statusCode = 404, message = "Workspace không tồn tại." });

            workspace.IsDeleted = true;
            workspace.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Xóa workspace thành công." });
        }

        /// <summary>
        /// Cập nhật vai trò thành viên
        /// </summary>
        [HttpPut("{workspaceId}/members/{memberId}")]
        public async Task<IActionResult> UpdateMemberRole(Guid workspaceId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var requester = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId && wm.IsActive);

            if (requester == null || (requester.WorkspaceRole != "OWNER" && requester.WorkspaceRole != "ADMIN"))
                return StatusCode(403, new { statusCode = 403, message = "Bạn không có quyền." });

            var targetMember = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == memberId && wm.IsActive);

            if (targetMember == null)
                return NotFound(new { statusCode = 404, message = "Thành viên không tồn tại." });
            
            // Limit: ADMIN cannot modify OWNER
            if (requester.WorkspaceRole == "ADMIN" && targetMember.WorkspaceRole == "OWNER")
                return StatusCode(403, new { statusCode = 403, message = "Admin không thể sửa quyền của Owner." });

            targetMember.WorkspaceRole = request.Role.ToUpper();
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Cập nhật vai trò thành công." });
        }

        private string GenerateInviteToken()
        {
            var tokenData = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }
            return Convert.ToBase64String(tokenData);
        }

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Mời thành viên vào workspace
        /// </summary>
        [HttpPost("{workspaceId}/members/invite")]
        public async Task<IActionResult> InviteMember(Guid workspaceId, [FromBody] InviteWorkspaceMemberRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var requester = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId && wm.IsActive);

            if (requester == null || (requester.WorkspaceRole != "OWNER" && requester.WorkspaceRole != "ADMIN"))
                return StatusCode(403, new { statusCode = 403, message = "Bạn không có quyền mời thành viên vào site này." });

            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                return NotFound(new { statusCode = 404, message = "Workspace không tồn tại." });

            var email = request.Email.Trim().ToLowerInvariant();
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            var createdPendingUser = false;

            if (targetUser == null)
            {
                createdPendingUser = true;
                targetUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = email.Split('@')[0],
                    PasswordHash = string.Empty,
                    IsActive = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(targetUser);
            }
            else
            {
                if (targetUser.IsDeleted)
                {
                    return BadRequest(new { statusCode = 400, message = "Email này thuộc về một tài khoản đã bị xóa." });
                }
            }

            var existing = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == targetUser.Id);

            if (existing != null)
            {
                if (existing.IsActive)
                    return BadRequest(new { statusCode = 400, message = "Thành viên này đã nằm trong workspace." });
                else
                {
                    existing.IsActive = true;
                    existing.WorkspaceRole = "MEMBER";
                }
            }
            else
            {
                _context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspaceId,
                    UserId = targetUser.Id,
                    WorkspaceRole = "MEMBER",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            var activeInviteTokens = await _context.RefreshTokens
                .Where(token => token.UserId == targetUser.Id && token.DeviceId == "Invite" && !token.IsRevoked)
                .ToListAsync();

            foreach (var token in activeInviteTokens)
            {
                token.IsRevoked = true;
            }

            var rawInviteToken = GenerateInviteToken();
            var inviteTokenHash = HashToken(rawInviteToken);
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = targetUser.Id,
                Token = inviteTokenHash,
                DeviceId = "Invite",
                ExpiryTime = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            // Currently we use a mocked URL. Frontend can build the real URL if needed.
            var acceptUrl = $"http://localhost:5173/accept-invite?token={Uri.EscapeDataString(rawInviteToken)}";
            
            try 
            {
                await _emailService.SendInviteEmailAsync(
                    email, 
                    targetUser.FullName, 
                    User.FindFirst(ClaimTypes.Name)?.Value ?? "System", 
                    workspace.Name, 
                    null, 
                    acceptUrl, 
                    "Bạn đã được mời vào workspace");
            }
            catch (Exception)
            {
                // Fallback if email sending fails, we just notify delivery failure but record is created
                return Ok(new { statusCode = 200, message = "Đã tạo bản ghi mời, nhưng cấu hình email service chưa khả dụng để gửi thư thực tế." });
            }

            var message = createdPendingUser
                ? "Đã gửi email mời. Người dùng sẽ ở trạng thái Invited cho đến khi đăng nhập."
                : "Đã gửi email mời và cập nhật quyền truy cập.";

            return Ok(new { statusCode = 200, message = message });
        }

        /// <summary>
        /// Xóa thành viên
        /// </summary>
        [HttpDelete("{workspaceId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid memberId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

            var requester = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == parsedUserId && wm.IsActive);

            if (requester == null)
                return NotFound(new { statusCode = 404, message = "Workspace không tồn tại." });

            // People can remove themselves, or OWNER/ADMIN can remove others.
            if (parsedUserId != memberId && requester.WorkspaceRole != "OWNER" && requester.WorkspaceRole != "ADMIN")
                return StatusCode(403, new { statusCode = 403, message = "Bạn không có quyền xóa thành viên này." });

            var targetMember = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == memberId && wm.IsActive);

            if (targetMember == null)
                return NotFound(new { statusCode = 404, message = "Thành viên không tồn tại." });

            // Prevent removing the last OWNER
            if (targetMember.WorkspaceRole == "OWNER")
            {
                var ownerCount = await _context.WorkspaceMembers.CountAsync(wm => wm.WorkspaceId == workspaceId && wm.WorkspaceRole == "OWNER" && wm.IsActive);
                if (ownerCount <= 1)
                    return BadRequest(new { statusCode = 400, message = "Không thể xóa Owner duy nhất. Cần chỉ định Owner mới trước." });
            }

            targetMember.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Xóa thành viên thành công." });
        }
    }

    public class CreateWorkspaceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Timezone { get; set; }
    }

    public class AddWorkspaceMemberRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class UpdateWorkspaceRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Logo { get; set; }
        public string? Timezone { get; set; }
    }

    public class UpdateMemberRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class InviteWorkspaceMemberRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
