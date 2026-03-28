using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace TaskManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;

        public AuthService(ApplicationDbContext context, IJwtService jwtService, IConfiguration configuration, IOtpService otpService)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
            _otpService = otpService;
        }

        public async Task<(AuthResponseDto response, string refreshToken)> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update user tokens
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SystemRoles = roles.ToArray()
            };

            return (response, refreshToken);
        }

        public async Task<(AuthResponseDto response, string refreshToken)> GoogleLoginAsync(GoogleLoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Credential))
            {
                throw new ArgumentException("Thiếu token xác thực từ Google (Credential is empty). Vui lòng thử lại.");
            }

            string email;
            string name;

            // Thử 2 luồng: JWT ID Token (One Tap) hoặc Access Token (Popup TOKEN flow)
            try
            {
                // Luồng 1: Nếu Credential là JWT ID Token → validate trực tiếp
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { "1008910270642-b5ic5oo3sb2rnemts5dp9sfaq025cud8.apps.googleusercontent.com" }
                };
                var jwtPayload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);
                email = jwtPayload.Email;
                name = jwtPayload.Name;
            }
            catch
            {
                // Luồng 2: Nếu Credential là Access Token → gọi Google Userinfo API
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Credential);
                
                var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    throw new UnauthorizedAccessException("Token Google không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
                }

                var content = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
                
                email = userInfo.GetProperty("email").GetString() 
                    ?? throw new UnauthorizedAccessException("Không thể lấy email từ Google.");
                name = userInfo.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? email : email;
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = name,
                    PasswordHash = string.Empty, // Empty or random hash since they login via Google
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                
                _context.Users.Add(user);
                
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Developer" || r.Name == "DEV");
                if (defaultRole != null)
                {
                    var ur = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id,
                        Role = defaultRole
                    };
                    _context.UserRoles.Add(ur);
                     // Assign for instant token generation using the exact same tracked instance
                    user.UserRoles = new List<UserRole> { ur };
                }
                
                await _context.SaveChangesAsync();
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SystemRoles = roles.ToArray()
            };

            return (response, refreshToken);
        }

        public async Task<(AuthResponseDto response, string refreshToken)> GitHubLoginAsync(GitHubLoginRequestDto request)
        {
            var gitHubConfig = _configuration.GetSection("GitHub");
            var clientId = gitHubConfig["ClientId"] ?? throw new InvalidOperationException("GitHub ClientId chưa được cấu hình.");
            var clientSecret = gitHubConfig["ClientSecret"] ?? throw new InvalidOperationException("GitHub ClientSecret chưa được cấu hình.");

            // Bước 1: Đổi authorization code lấy access_token từ GitHub
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SprintA", "1.0"));

            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", request.Code }
            };

            var tokenResponse = await httpClient.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(tokenRequest));
            
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);

            if (!tokenJson.TryGetProperty("access_token", out var accessTokenElement))
            {
                var errorDesc = tokenJson.TryGetProperty("error_description", out var errEl) 
                    ? errEl.GetString() 
                    : "Không thể xác thực với GitHub.";
                throw new UnauthorizedAccessException(errorDesc);
            }

            var githubAccessToken = accessTokenElement.GetString()!;

            // Bước 2: Dùng access_token lấy thông tin user từ GitHub API
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubAccessToken);

            var userResponse = await httpClient.GetAsync("https://api.github.com/user");
            var userContent = await userResponse.Content.ReadAsStringAsync();
            var githubUser = JsonSerializer.Deserialize<JsonElement>(userContent);

            var githubEmail = githubUser.TryGetProperty("email", out var emailEl) && emailEl.ValueKind != JsonValueKind.Null
                ? emailEl.GetString()
                : null;

            // Nếu email bị ẩn, gọi thêm API emails
            if (string.IsNullOrEmpty(githubEmail))
            {
                var emailsResponse = await httpClient.GetAsync("https://api.github.com/user/emails");
                var emailsContent = await emailsResponse.Content.ReadAsStringAsync();
                var emails = JsonSerializer.Deserialize<JsonElement>(emailsContent);

                foreach (var emailItem in emails.EnumerateArray())
                {
                    if (emailItem.TryGetProperty("primary", out var primary) && primary.GetBoolean())
                    {
                        githubEmail = emailItem.GetProperty("email").GetString();
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(githubEmail))
            {
                throw new UnauthorizedAccessException("Không thể lấy email từ tài khoản GitHub. Vui lòng cho phép truy cập email.");
            }

            var githubName = githubUser.TryGetProperty("name", out var nameEl) && nameEl.ValueKind != JsonValueKind.Null
                ? nameEl.GetString()
                : githubUser.GetProperty("login").GetString();

            // Bước 3: Tìm hoặc tạo User mới
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == githubEmail && !u.IsDeleted);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = githubEmail,
                    FullName = githubName ?? githubEmail,
                    PasswordHash = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Users.Add(user);

                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Developer" || r.Name == "DEV");
                if (defaultRole != null)
                {
                    var ur = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id,
                        Role = defaultRole
                    };
                    _context.UserRoles.Add(ur);
                    user.UserRoles = new List<UserRole> { ur };
                }

                await _context.SaveChangesAsync();
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SystemRoles = roles.ToArray()
            };

            return (response, refreshToken);
        }

        public async Task<(string newAccessToken, string newRefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null) throw new UnauthorizedAccessException("Invalid access token or refresh token");

            var userIdString = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                throw new UnauthorizedAccessException("Invalid token claims");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid access token or refresh token");
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return (newAccessToken, newRefreshToken);
        }

        public async Task RevokeTokenAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            
            await _context.SaveChangesAsync();
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            if(storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$")) {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            return inputPassword == storedHash;
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (exists)
            {
                throw new InvalidOperationException("Email đã được sử dụng.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(newUser);

            // Assign default role (e.g. Developer)
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Developer" || r.Name == "DEV");
            if (defaultRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = defaultRole.Id
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
