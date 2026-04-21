using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagement.Application.DTOs.AI;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("FixedWindow")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private const int GeminiRetryAttempts = 3;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("usage")]
        public async Task<IActionResult> Usage()
        {
            var userId = GetUserId();
            var usage = await _aiService.GetUsageAsync(userId);
            return Ok(ApiResponse<AiUsageDto>.Success(usage));
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<object>.Error("Message cannot be empty."));
            }

            try
            {
                var userId = GetUserId();
                var response = await _aiService.ChatAsync(userId, request);
                return Ok(ApiResponse<string>.Success(response));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDescription([FromBody] AiGenerateDescriptionRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(ApiResponse<object>.Error("Prompt khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                var description = await _aiService.GenerateDescriptionAsync(userId, request);
                return Ok(ApiResponse<string>.Success(description, "Generated"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("breakdown-task")]
        public async Task<IActionResult> BreakdownTask([FromBody] AiBreakdownRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<object>.Error("Tieu de cong viec khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();

                if (request.CreateSubtasks)
                {
                    var created = await ExecuteWithGeminiRetryAsync(
                        () => _aiService.BreakdownAndCreateSubtasksAsync(userId, request),
                        request.Title);
                    return Ok(ApiResponse<List<WorkTaskResponseDto>>.Success(created, "AI da phan ra va tao sub-work items."));
                }

                var subtasks = await ExecuteWithGeminiRetryAsync(
                    () => _aiService.BreakdownTaskAsync(userId, request),
                    request.Title);
                return Ok(ApiResponse<List<AiSubTaskDto>>.Success(subtasks, "AI da phan tach cong viec thanh cong."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                if (IsTransientAiFailure(ex))
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable,
                        ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.Title)));
                }
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex) when (IsTransientAiFailure(ex))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.Title)));
            }
            catch (TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.Title)));
            }
        }

        private async Task<T> ExecuteWithGeminiRetryAsync<T>(Func<Task<T>> action, string? title)
        {
            Exception? lastException = null;

            for (var attempt = 1; attempt <= GeminiRetryAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsTransientAiFailure(ex))
                {
                    lastException = ex;

                    if (attempt == GeminiRetryAttempts)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(400 * attempt));
                }
            }

            throw new InvalidOperationException(BuildGeminiFallbackMessage(title), lastException);
        }

        private static bool IsTransientAiFailure(Exception ex)
        {
            var message = ex.Message?.ToLowerInvariant() ?? string.Empty;

            return ex is HttpRequestException
                || ex is TaskCanceledException
                || message.Contains("503")
                || message.Contains("gemini unavailable")
                || message.Contains("service unavailable")
                || message.Contains("temporarily unavailable")
                || message.Contains("quota")
                || message.Contains("timeout");
        }

        private static string BuildGeminiFallbackMessage(string? title)
        {
            var safeTitle = string.IsNullOrWhiteSpace(title) ? "cong viec nay" : $"\"{title}\"";
            return $"Gemini tam thoi khong san sang sau 3 lan thu lai. Ban co the thu lai sau it phut hoac tu tach {safeTitle} thanh 3-5 buoc nho: muc tieu, dau viec, kiem thu, ban giao.";
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Token khong hop le.");
            }

            return userId;
        }
    }
}
