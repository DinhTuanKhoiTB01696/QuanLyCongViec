using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using System.Security.Claims;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var (response, refreshToken) = await _authService.LoginAsync(request);

                // Set Refresh Token as HttpOnly Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps, // Set true only if request is HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

                return Ok(new { statusCode = 200, message = "Success", data = response });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { statusCode = 401, message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { statusCode = 200, message = "Đăng ký thành công" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Internal server error" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { statusCode = 401, message = "Refresh token is missing" });
                }

                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { statusCode = 401, message = "Access token is missing" });
                }

                var accessToken = authHeader.Substring("Bearer ".Length).Trim();

                var (newAccessToken, newRefreshToken) = await _authService.RefreshTokenAsync(accessToken, refreshToken);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

                return Ok(new { statusCode = 200, message = "Success", data = new { accessToken = newAccessToken } });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { statusCode = 401, message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    await _authService.RevokeTokenAsync(userId);
                }

                Response.Cookies.Delete("refreshToken");
                return Ok(new { statusCode = 200, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }
    }
}
