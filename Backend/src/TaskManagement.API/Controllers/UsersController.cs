using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private static readonly JsonSerializerOptions ProfileJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ApplicationDbContext _context;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public UsersController(ApplicationDbContext context, IOtpService otpService, IEmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.DepartmentMemberships)
                .ThenInclude(dm => dm.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            if (user == null) return NotFound(new { message = "User not found" });

            // We read extra fields if available, otherwise fallback.
            // Since User entity might not have JobTitle, Organization, Department natively,
            // we will simulate them via SystemSetting or allow the schema to adapt.
            // Since we can't change the db schema without migrations, we will store profile extra data
            // into SystemSettings where Key = "Profile_" + userId.
            
            var extraProfileSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Profile_" + userId.ToString());

            var extra = new UpdateProfileRequest();

            if (extraProfileSetting != null && !string.IsNullOrEmpty(extraProfileSetting.Value))
            {
                try
                {
                    extra = JsonSerializer.Deserialize<UpdateProfileRequest>(extraProfileSetting.Value, ProfileJsonOptions) ?? extra;
                }
                catch
                {
                }
            }

            var activeDepartment = user.DepartmentMemberships
                .Select(dm => dm.Department)
                .FirstOrDefault(department => department != null && department.IsActive && !department.IsDeleted);

            return Ok(new
            {
                statusCode = 200,
                data = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    avatarUrl = user.AvatarUrl,
                    publicName = string.IsNullOrEmpty(extra.PublicName) ? user.FullName : extra.PublicName,
                    jobTitle = extra.JobTitle,
                    departmentName = activeDepartment?.Name ?? extra.DepartmentName,
                    organizationName = extra.OrganizationName,
                    collaborationRules = extra.CollaborationRules,
                    hasPassword = !string.IsNullOrEmpty(user.PasswordHash),
                    is2FaEnabled = user.Is2FAEnabled
                }
            });
        }

        public class UpdateProfileRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string PublicName { get; set; } = string.Empty;
            public string JobTitle { get; set; } = string.Empty;
            public string DepartmentName { get; set; } = string.Empty;
            public string OrganizationName { get; set; } = string.Empty;
            public string CollaborationRules { get; set; } = string.Empty;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            user.FullName = string.IsNullOrWhiteSpace(request.FullName) ? user.FullName : request.FullName.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            var extraProfileSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Profile_" + userId.ToString());
            
            if (extraProfileSetting == null)
            {
                extraProfileSetting = new TaskManagement.Domain.Entities.SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "Profile_" + userId.ToString(),
                    SettingGroup = "UserProfile"
                };
                _context.SystemSettings.Add(extraProfileSetting);
            }

            extraProfileSetting.Value = JsonSerializer.Serialize(new UpdateProfileRequest
            {
                FullName = user.FullName,
                PublicName = request.PublicName?.Trim() ?? string.Empty,
                JobTitle = request.JobTitle?.Trim() ?? string.Empty,
                DepartmentName = request.DepartmentName?.Trim() ?? string.Empty,
                OrganizationName = request.OrganizationName?.Trim() ?? string.Empty,
                CollaborationRules = request.CollaborationRules?.Trim() ?? string.Empty
            });
            extraProfileSetting.LastModifiedAt = DateTime.UtcNow;

            var normalizedDepartmentName = request.DepartmentName?.Trim();
            var currentMemberships = await _context.DepartmentMembers
                .Where(dm => dm.UserId == userId)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(normalizedDepartmentName))
            {
                if (currentMemberships.Count > 0)
                {
                    _context.DepartmentMembers.RemoveRange(currentMemberships);
                }
            }
            else
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.IsActive && !d.IsDeleted && d.Name.ToLower() == normalizedDepartmentName.ToLower());

                if (department != null)
                {
                    var alreadyAssigned = currentMemberships.Any(dm => dm.DepartmentId == department.Id);
                    if (!alreadyAssigned)
                    {
                        _context.DepartmentMembers.RemoveRange(currentMemberships);
                        _context.DepartmentMembers.Add(new DepartmentMember
                        {
                            DepartmentId = department.Id,
                            UserId = userId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Profile updated successfully" });
        }

        public class ChangePasswordRequest
        {
            public string OtpCode { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public bool LogoutOthers { get; set; } = false;
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            // Validate OTP
            if (string.IsNullOrEmpty(request.OtpCode))
                return BadRequest(new { statusCode = 400, message = "Vui lòng nhập mã OTP." });

            var isValidOtp = _otpService.ValidateOtp(user.Email, request.OtpCode);
            if (!isValidOtp)
                return BadRequest(new { statusCode = 400, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đổi mật khẩu thành công" });
        }

        public class SendChangePasswordOtpRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("send-change-password-otp")]
        public async Task<IActionResult> SendChangePasswordOtp([FromBody] SendChangePasswordOtpRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            // Verify the email matches the user's account email
            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { statusCode = 400, message = "Email không khớp với tài khoản của bạn." });

            var otpCode = _otpService.GenerateOtp();
            _otpService.StoreOtp(user.Email, otpCode);
            await _emailService.SendOtpEmailAsync(user.Email, otpCode);

            return Ok(new { statusCode = 200, message = "Đã gửi mã OTP đến email của bạn." });
        }

        [HttpPost("send-set-password-otp")]
        public async Task<IActionResult> SendSetPasswordOtp()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (!string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest(new { message = "Tài khoản đã có mật khẩu." });

            var otpCode = _otpService.GenerateOtp();
            _otpService.StoreOtp(user.Email, otpCode);
            await _emailService.SendOtpEmailAsync(user.Email, otpCode);

            return Ok(new { statusCode = 200, message = "Đã gửi mã OTP đến email của bạn." });
        }

        public class SetPasswordRequest
        {
            public string OtpCode { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("set-password-with-otp")]
        public async Task<IActionResult> SetPasswordWithOtp([FromBody] SetPasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (!string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest(new { message = "Tài khoản đã có mật khẩu." });

            var isValid = _otpService.ValidateOtp(user.Email, request.OtpCode);
            if (!isValid)
                return BadRequest(new { statusCode = 400, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Tạo mật khẩu thành công." });
        }

        public class Toggle2FARequest { public bool Enable { get; set; } }

        [HttpPost("toggle-2fa")]
        public async Task<IActionResult> Toggle2FA([FromBody] Toggle2FARequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            user.Is2FAEnabled = request.Enable;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { statusCode = 200, message = request.Enable ? "Đã bật 2FA." : "Đã tắt 2FA.", is2FaEnabled = user.Is2FAEnabled });
        }

        /// <summary>
        /// PUT /api/users/avatar — Upload avatar image
        /// </summary>
        [HttpPut("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Chưa chọn file." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File quá lớn (tối đa 5MB)." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WebP)." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var uploadsDir = Path.Combine(env.ContentRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var uniqueName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.AvatarUrl = $"/uploads/avatars/{uniqueName}";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Cập nhật ảnh đại diện thành công.", data = new { avatarUrl = user.AvatarUrl } });
        }
    }
}
