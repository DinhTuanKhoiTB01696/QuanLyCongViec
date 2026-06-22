using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace TaskManagement.Tests.Logic
{
    public class AuthLogicTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly OtpService _otpService;
        private readonly AuthService _authService;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        public AuthLogicTests()
        {
            // Set up DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Set up memory cache
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _otpService = new OtpService(_memoryCache);

            // Set up mocks
            _jwtServiceMock = new Mock<IJwtService>();
            _configurationMock = new Mock<IConfiguration>();
            _emailServiceMock = new Mock<IEmailService>();

            _authService = new AuthService(
                _context,
                _jwtServiceMock.Object,
                _configurationMock.Object,
                _otpService,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public void OtpService_Generate_ReturnsSixCharacters()
        {
            var otp = _otpService.GenerateOtp();
            otp.Should().NotBeNullOrWhiteSpace();
            otp.Length.Should().Be(6);
        }

        [Fact]
        public void OtpService_StoreAndValidate_SucceedsAndRemoves()
        {
            var email = "test@example.com";
            var otp = _otpService.GenerateOtp();

            _otpService.StoreOtp(email, otp);

            // First validation should succeed
            var isValid = _otpService.ValidateOtp(email, otp);
            isValid.Should().BeTrue();

            // Second validation should fail (deleted from cache)
            var isStillValid = _otpService.ValidateOtp(email, otp);
            isStillValid.Should().BeFalse();
        }

        [Fact]
        public void OtpService_ValidateWrongCode_Fails()
        {
            var email = "test@example.com";
            var otp = _otpService.GenerateOtp();

            _otpService.StoreOtp(email, otp);

            var isValid = _otpService.ValidateOtp(email, "WRONG1");
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPassword_ValidOtp_UpdatesPasswordCorrectly()
        {
            // Seed user
            var email = "user@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Store OTP
            var otp = _otpService.GenerateOtp();
            _otpService.StoreOtp(email, otp);

            // Reset request
            var request = new ResetPasswordRequestDto
            {
                Email = email,
                OtpToken = otp,
                NewPassword = "NewPass123!"
            };

            // Act
            await _authService.ResetPasswordAsync(request);

            // Assert - check password hash
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify("NewPass123!", updatedUser!.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task ResetPassword_InvalidOtp_ThrowsUnauthorizedAccessException()
        {
            var email = "user@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new ResetPasswordRequestDto
            {
                Email = email,
                OtpToken = "INVALID",
                NewPassword = "NewPass123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ResetPasswordAsync(request));
        }

        [Fact]
        public async Task ResetPassword_NonExistentEmail_ThrowsArgumentException()
        {
            // Seed OTP for email that doesn't belong to any user
            var email = "nonexistent@example.com";
            var otp = _otpService.GenerateOtp();
            _otpService.StoreOtp(email, otp);

            var request = new ResetPasswordRequestDto
            {
                Email = email,
                OtpToken = otp,
                NewPassword = "NewPass123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.ResetPasswordAsync(request));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _memoryCache.Dispose();
        }
    }
}
