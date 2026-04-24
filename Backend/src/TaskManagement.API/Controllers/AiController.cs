using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.AI;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("FixedWindow")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ApplicationDbContext _dbContext;
        private const int GeminiRetryAttempts = 3;
        private static readonly string[] AiAssigneeAllowedProjectRoles = { "PM", "PO", "SM", "PROJECT_MANAGER", "SCRUM_MASTER", "Admin" };
        private static readonly string[] AiSystemOverrideRoles = { "SuperAdmin", "Admin", "System Admin", "Organization Admin", "AccessAdmin", "Access Admin" };

        public AiController(IAiService aiService, ApplicationDbContext dbContext)
        {
            _aiService = aiService;
            _dbContext = dbContext;
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
                if (IsTransientAiFailure(ex))
                {
                    return Ok(ApiResponse<string>.Success(
                        BuildLocalChatFallback(request.Message),
                        "Gemini tam thoi chua san sang. He thong da dung local chat fallback."));
                }
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex) when (IsTransientAiFailure(ex))
            {
                return Ok(ApiResponse<string>.Success(
                    BuildLocalChatFallback(request.Message),
                    "Gemini tam thoi chua san sang. He thong da dung local chat fallback."));
            }
            catch (TaskCanceledException)
            {
                return Ok(ApiResponse<string>.Success(
                    BuildLocalChatFallback(request.Message),
                    "Gemini tam thoi chua san sang. He thong da dung local chat fallback."));
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
                    return Ok(ApiResponse<List<AiSubTaskDto>>.Success(
                        BuildLocalBreakdownFallback(request),
                        "Gemini tam thoi chua san sang. He thong da dung local breakdown preview."));
                }
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex) when (IsTransientAiFailure(ex))
            {
                return Ok(ApiResponse<List<AiSubTaskDto>>.Success(
                    BuildLocalBreakdownFallback(request),
                    "Gemini tam thoi chua san sang. He thong da dung local breakdown preview."));
            }
            catch (TaskCanceledException)
            {
                return Ok(ApiResponse<List<AiSubTaskDto>>.Success(
                    BuildLocalBreakdownFallback(request),
                    "Gemini tam thoi chua san sang. He thong da dung local breakdown preview."));
            }
        }

        [HttpPost("create-subtasks-from-preview")]
        public async Task<IActionResult> CreateSubtasksFromPreview([FromBody] AiCreateSubtasksFromPreviewRequestDto request)
        {
            if (request.ProjectId == Guid.Empty || request.ParentTaskId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Error("ProjectId va ParentTaskId khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                var created = await _aiService.CreateSubtasksFromPreviewAsync(userId, request);
                return Ok(ApiResponse<List<WorkTaskResponseDto>>.Success(created, "AI preview da duoc tao thanh sub-work items."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("suggest-estimate")]
        public async Task<IActionResult> SuggestEstimate([FromBody] AiEstimateSuggestionRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<object>.Error("Tieu de cong viec khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                var suggestion = await ExecuteWithGeminiRetryAsync(
                    () => _aiService.SuggestEstimateAsync(userId, request),
                    request.Title);
                return Ok(ApiResponse<AiEstimateSuggestionDto>.Success(suggestion, "AI da goi y estimate thanh cong."));
            }
            catch (InvalidOperationException ex)
            {
                if (IsTransientAiFailure(ex))
                {
                    return Ok(ApiResponse<AiEstimateSuggestionDto>.Success(
                        BuildLocalEstimateFallback(request),
                        "Gemini tam thoi chua san sang. He thong da dung local suggestion."));
                }

                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex) when (IsTransientAiFailure(ex))
            {
                return Ok(ApiResponse<AiEstimateSuggestionDto>.Success(
                    BuildLocalEstimateFallback(request),
                    "Gemini tam thoi chua san sang. He thong da dung local suggestion."));
            }
            catch (TaskCanceledException)
            {
                return Ok(ApiResponse<AiEstimateSuggestionDto>.Success(
                    BuildLocalEstimateFallback(request),
                    "Gemini tam thoi chua san sang. He thong da dung local suggestion."));
            }
        }

        [HttpPost("suggest-assignees")]
        public async Task<IActionResult> SuggestAssignees([FromBody] AiAssigneeSuggestionRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Error("ProjectId khong duoc de trong."));
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<object>.Error("Tieu de cong viec khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                await EnsureAiAssigneeAccessAsync(userId, request.ProjectId);
                var suggestion = await _aiService.SuggestAssigneesAsync(userId, request);
                return Ok(ApiResponse<AiAssigneeSuggestionDto>.Success(suggestion, "AI da goi y assignee thanh cong."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("repo-analysis")]
        public async Task<IActionResult> AnalyzeRepository([FromBody] AiRepositoryAnalysisRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RepoUrl))
            {
                return BadRequest(ApiResponse<object>.Error("Repo URL khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                var analysis = await ExecuteWithGeminiRetryAsync(
                    () => _aiService.AnalyzeRepositoryAsync(userId, request),
                    request.RepoUrl);
                return Ok(ApiResponse<AiRepositoryAnalysisDto>.Success(analysis, "AI da phan tich repo thanh cong."));
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
                        ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.RepoUrl)));
                }

                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex) when (IsTransientAiFailure(ex))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.RepoUrl)));
            }
            catch (TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Error(BuildGeminiFallbackMessage(request.RepoUrl)));
            }
        }

        [HttpPost("repo-analysis/create-work-items")]
        public async Task<IActionResult> CreateBacklogItemsFromAnalysis([FromBody] AiCreateBacklogFromAnalysisRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Error("ProjectId khong duoc de trong."));
            }

            try
            {
                var userId = GetUserId();
                var created = await _aiService.CreateBacklogItemsFromAnalysisAsync(userId, request);
                return Ok(ApiResponse<List<WorkTaskResponseDto>>.Success(created, $"AI da tao {created.Count} work items vao project."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
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
            if (!string.IsNullOrWhiteSpace(title) &&
                (title.Contains("github.com", StringComparison.OrdinalIgnoreCase) || title.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
            {
                return "Gemini tam thoi khong san sang sau 3 lan thu lai. Ban co the thu lai sau it phut hoac tu phan tich repo thanh 3-5 nhom viec: quick wins, medium tasks, risky tasks, test plan.";
            }

            var safeTitle = string.IsNullOrWhiteSpace(title) ? "cong viec nay" : $"\"{title}\"";
            return $"Gemini tam thoi khong san sang sau 3 lan thu lai. Ban co the thu lai sau it phut hoac tu tach {safeTitle} thanh 3-5 buoc nho: muc tieu, dau viec, kiem thu, ban giao.";
        }

        private static string BuildLocalChatFallback(string? message)
        {
            var clean = (message ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(clean))
            {
                return "Gemini dang tam nghi vi vuot quota, nhung SprintA van san sang ho tro. Hay nhap ro ten task, du an, hoac cau hoi ban muon xu ly.";
            }

            return
                $"Gemini dang tam nghi vi vuot quota nen SprintA dang tra loi bang local fallback.\n\n" +
                $"Noi dung ban vua gui: \"{clean}\"\n\n" +
                $"De tiep tuc khong bi dung mach, ban co the lam theo 4 buoc:\n" +
                $"- Lam ro muc tieu chinh cua yeu cau nay.\n" +
                $"- Tach no thanh 3-5 dau viec nho de giao ngay.\n" +
                $"- Neu la viec ky thuat, them tieu chi kiem thu va rui ro.\n" +
                $"- Neu can, hoi tiep theo mau: tom tat, breakdown, test case, hoac handoff.";
        }

        private static List<AiSubTaskDto> BuildLocalBreakdownFallback(AiBreakdownRequestDto request)
        {
            var normalizedTitle = string.IsNullOrWhiteSpace(request.Title) ? "Work item" : request.Title.Trim();
            var description = (request.Description ?? string.Empty).Trim();
            var context = string.IsNullOrWhiteSpace(description)
                ? $"Implement the scope of {normalizedTitle} with a safe default delivery flow."
                : description;

            return new List<AiSubTaskDto>
            {
                new()
                {
                    Title = $"Clarify scope for {normalizedTitle}",
                    Description = $"Review requirements, acceptance criteria, dependencies, and edge cases. Context: {context}",
                    EstHours = 1.5,
                    Priority = 3
                },
                new()
                {
                    Title = $"Implement core flow for {normalizedTitle}",
                    Description = "Build the main behavior for this work item and align the code with the current module structure.",
                    EstHours = 4,
                    Priority = 2
                },
                new()
                {
                    Title = $"Handle validation and failure cases for {normalizedTitle}",
                    Description = "Add validation, guards, and error handling so the feature is stable in invalid and edge scenarios.",
                    EstHours = 2.5,
                    Priority = 2
                },
                new()
                {
                    Title = $"Test and hand off {normalizedTitle}",
                    Description = "Verify the completed flow, prepare test steps, and write a short handoff note for reviewer or PM.",
                    EstHours = 2,
                    Priority = 3
                }
            };
        }

        private static AiEstimateSuggestionDto BuildLocalEstimateFallback(AiEstimateSuggestionRequestDto request)
        {
            var normalizedStoryPoints = NormalizeStoryPoints(request.StoryPoints);
            var hours = InferLocalHours(request.Title, request.Priority, normalizedStoryPoints, request.AssigneeCount, request.SubtaskCount);

            return new AiEstimateSuggestionDto
            {
                SuggestedHours = hours,
                SuggestedDays = Math.Clamp((int)Math.Ceiling(hours / 6d), 1, 10),
                SuggestedStoryPoints = normalizedStoryPoints,
                Complexity = InferComplexity(hours),
                Reasoning = "Suggested from priority, story points, assignee count, and task title keywords."
            };
        }

        private static double InferLocalHours(string? title, int priority, double storyPoints, int assigneeCount, int subtaskCount)
        {
            var hours = storyPoints switch
            {
                <= 0 => 4d,
                <= 1 => 3d,
                <= 2 => 5d,
                <= 3 => 8d,
                <= 5 => 14d,
                <= 8 => 24d,
                _ => 40d
            };

            hours += priority switch
            {
                1 => 2,
                2 => 1,
                4 => -1,
                _ => 0
            };

            var lowered = (title ?? string.Empty).ToLowerInvariant();
            if (lowered.Contains("api") || lowered.Contains("integration") || lowered.Contains("payment") || lowered.Contains("deploy") || lowered.Contains("security"))
            {
                hours += 3;
            }

            if (lowered.Contains("refactor") || lowered.Contains("migration"))
            {
                hours += 4;
            }

            if (lowered.Contains("bug") || lowered.Contains("fix") || lowered.Contains("hotfix") || lowered.Contains("patch"))
            {
                hours += 1.5;
            }

            if (lowered.Contains("ui") || lowered.Contains("ux") || lowered.Contains("copy") || lowered.Contains("docs") || lowered.Contains("content"))
            {
                hours -= 1;
            }

            if (assigneeCount > 1)
            {
                hours += Math.Min(4, assigneeCount - 1);
            }

            if (subtaskCount > 0)
            {
                hours += Math.Min(6, subtaskCount);
            }

            return Math.Round(Math.Clamp(hours, 1, 80), 1);
        }

        private static double NormalizeStoryPoints(double storyPoints)
        {
            var allowed = new[] { 1d, 2d, 3d, 5d, 8d, 13d };
            return allowed
                .OrderBy(value => Math.Abs(value - Math.Max(1, storyPoints)))
                .First();
        }

        private static string InferComplexity(double hours)
        {
            if (hours <= 6) return "Low";
            if (hours <= 18) return "Medium";
            if (hours <= 32) return "High";
            return "Critical";
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

        private async Task EnsureAiAssigneeAccessAsync(Guid userId, Guid projectId)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(item => item.UserRoles)
                .ThenInclude(link => link.Role)
                .FirstOrDefaultAsync(item => item.Id == userId);

            var hasSystemOverride = user != null
                && user.IsActive
                && !user.IsDeleted
                && user.UserRoles.Any(ur =>
                    ur.Role != null &&
                    AiSystemOverrideRoles.Contains(ur.Role.Name, StringComparer.OrdinalIgnoreCase));

            if (hasSystemOverride)
            {
                return;
            }

            var projectRole = await _dbContext.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status)
                .Select(pm => pm.ProjectRole)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(projectRole) ||
                !AiAssigneeAllowedProjectRoles.Contains(projectRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Ban khong co quyen dung AI goi y assignee cho project nay.");
            }
        }
    }
}
