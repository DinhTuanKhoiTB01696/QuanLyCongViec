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

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
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
                try {
                    extra = System.Text.Json.JsonSerializer.Deserialize<UpdateProfileRequest>(extraProfileSetting.Value) ?? extra;
                } catch { }
            }

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
                    departmentName = extra.DepartmentName,
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

            user.FullName = request.FullName;
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

            var extraProfileData = new
            {
                publicName = request.PublicName,
                jobTitle = request.JobTitle,
                departmentName = request.DepartmentName,
                organizationName = request.OrganizationName,
                collaborationRules = request.CollaborationRules
            };

            extraProfileSetting.Value = System.Text.Json.JsonSerializer.Serialize(extraProfileData);
            extraProfileSetting.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Profile updated successfully" });
        }

        public class ChangePasswordRequest
        {
            public string OldPassword { get; set; } = string.Empty;
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

            if (string.IsNullOrEmpty(user.PasswordHash)) 
                return BadRequest(new { statusCode = 400, message = "Tài khoản của bạn được liên kết qua Google/Github. Không thể đổi mật khẩu." });

            bool isValidOld = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
            if (!isValidOld) return BadRequest(new { statusCode = 400, message = "Mật khẩu hiện tại không chính xác." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đổi mật khẩu thành công" });
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

        public class SendOtpRequest { public string Email { get; set; } = string.Empty; }

        [HttpPost("send-change-password-otp")]
        public async Task<IActionResult> SendChangePasswordOtp([FromBody] SendOtpRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (user.Email != request.Email)
                return BadRequest(new { message = "Email không khớp với tài khoản hiện tại." });

            var otpCode = _otpService.GenerateOtp();
            _otpService.StoreOtp(user.Email, otpCode);
            await _emailService.SendOtpEmailAsync(user.Email, otpCode);

            return Ok(new { statusCode = 200, message = "Đã gửi mã OTP đến email của bạn." });
        }

        public class ChangePasswordWithOtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public bool LogoutOthers { get; set; } = false;
        }

        [HttpPut("change-password-with-otp")]
        public async Task<IActionResult> ChangePasswordWithOtp([FromBody] ChangePasswordWithOtpRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (user.Email != request.Email)
                return BadRequest(new { message = "Email không hợp lệ." });

            var isValid = _otpService.ValidateOtp(user.Email, request.OtpCode);
            if (!isValid)
                return BadRequest(new { statusCode = 400, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Cập nhật mật khẩu thành công." });
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
