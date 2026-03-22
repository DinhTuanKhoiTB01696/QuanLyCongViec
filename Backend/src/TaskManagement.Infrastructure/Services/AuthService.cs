using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace TaskManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
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
            await _context.SaveChangesAsync();
        }
    }
}
