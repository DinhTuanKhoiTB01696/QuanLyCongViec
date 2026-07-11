using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
        private readonly IWorkTaskService _workTaskService;
        private readonly ApplicationDbContext _dbContext;
        private const int GeminiRetryAttempts = 3;
        private const long AiDocumentMaxBytes = 10 * 1024 * 1024;
        private static readonly IReadOnlyDictionary<string, string[]> AiDocumentMimeTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".txt"] = ["text/plain"],
                [".md"] = ["text/markdown", "text/plain"],
                [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
                [".pdf"] = ["application/pdf"]
            };
        private static readonly string[] AiAssigneeAllowedProjectRoles = { "PM", "PO", "SM", "PROJECT_MANAGER", "SCRUM_MASTER", "Admin" };
        private static readonly string[] AiSystemOverrideRoles = { "SuperAdmin", "Admin", "System Admin", "Organization Admin", "AccessAdmin", "Access Admin" };

        public AiController(IAiService aiService, IWorkTaskService workTaskService, ApplicationDbContext dbContext)
        {
            _aiService = aiService;
            _workTaskService = workTaskService;
            _dbContext = dbContext;
        }

        [HttpGet("usage")]
        public async Task<IActionResult> Usage()
        {
            var userId = GetUserId();
            var usage = await _aiService.GetUsageAsync(userId);
            return Ok(ApiResponse<AiUsageDto>.Success(usage));
        }

        [HttpPost("analyze-file")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> AnalyzeFile(
            [FromForm] IFormFile file,
            [FromForm] Guid? projectId = null,
            [FromForm] string? userPrompt = null,
            [FromForm] string? previousAnalysisJson = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Error("Vui lòng cung cấp file dữ liệu hợp lệ."));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AiDocumentMimeTypes.TryGetValue(ext, out var allowedMimeTypes))
            {
                return BadRequest(ApiResponse<object>.Error("Chỉ hỗ trợ tài liệu .txt, .md, .docx hoặc .pdf. Vui lòng dùng chức năng Import Excel/CSV cho dữ liệu bảng."));
            }

            var mimeType = (file.ContentType ?? string.Empty).Split(';', 2)[0].Trim();
            if (!allowedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Error($"Loại nội dung '{mimeType}' không khớp với phần mở rộng {ext}."));
            }

            if (file.Length > AiDocumentMaxBytes)
            {
                return BadRequest(ApiResponse<object>.Error("Dung lượng file vượt quá giới hạn cho phép (10MB)."));
            }

            try
            {
                var userId = GetUserId();
                if (projectId.HasValue && projectId.Value != Guid.Empty)
                {
                    if (!await UserHasProjectAccessAsync(userId, projectId.Value))
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error("Bạn không có quyền truy cập dự án này."));
                    }
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                if (!HasValidDocumentSignature(ext, fileBytes))
                {
                    return BadRequest(ApiResponse<object>.Error("Nội dung file không hợp lệ hoặc không khớp với định dạng đã chọn."));
                }

                var result = await _aiService.AnalyzeFileAsync(
                    userId,
                    file.FileName,
                    mimeType,
                    fileBytes,
                    userPrompt,
                    projectId,
                    previousAnalysisJson);

                return Ok(ApiResponse<AiFileAnalysisResultDto>.Success(result));
            }
            catch (InvalidOperationException ex) when (IsGeminiQuotaFailure(ex))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, ApiResponse<object>.Error("AI tạm thời hết hạn mức, vui lòng thử lại sau"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
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

        [HttpPost("project-assistant")]
        public async Task<IActionResult> ProjectAssistant([FromBody] AiProjectAssistantRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Error("Request body khong hop le."));
            }

            if (request.ProjectId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Error("ID dự án không hợp lệ."));
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<object>.Error("Nội dung tin nhắn không được để trống."));
            }

            try
            {
                var userId = GetUserId();

                if (!await UserHasProjectAccessAsync(userId, request.ProjectId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error("Bạn không có quyền truy cập dự án này."));
                }

                var response = await _aiService.ProjectAssistantAsync(userId, request);
                return Ok(ApiResponse<AiProjectAssistantResponseDto>.Success(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        private static bool HasValidDocumentSignature(string extension, byte[] bytes)
        {
            if (bytes.Length == 0) return false;
            return extension switch
            {
                ".pdf" => bytes.Length >= 5 && bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
                ".docx" => bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B &&
                           bytes[2] is 0x03 or 0x05 or 0x07 && bytes[3] is 0x04 or 0x06 or 0x08,
                ".txt" or ".md" => Array.IndexOf(bytes, (byte)0) < 0,
                _ => false
            };
        }

        [HttpPost("context-chat")]
        public async Task<IActionResult> ContextChat([FromBody] AiContextChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<object>.Error("Message cannot be empty."));
            }

            var userId = GetUserId();
            if (request.ProjectId.HasValue && request.ProjectId.Value != Guid.Empty)
            {
                var project = await _dbContext.Projects
                    .AsNoTracking()
                    .Where(p => p.Id == request.ProjectId.Value && !p.IsDeleted)
                    .Select(p => new { p.Id, p.WorkspaceId })
                    .FirstOrDefaultAsync();

                if (project == null || !await UserHasProjectContextAccessAsync(userId, project.WorkspaceId, project.Id))
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<object>.Error("Bạn không có quyền truy cập dữ liệu dự án này."));
                }

                if (request.WorkspaceId.HasValue && request.WorkspaceId.Value != project.WorkspaceId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<object>.Error("Workspace không khớp với dự án."));
                }
            }
            else if (request.WorkspaceId.HasValue && request.WorkspaceId.Value != Guid.Empty &&
                     !await UserHasWorkspaceAccessAsync(userId, request.WorkspaceId.Value))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Error("Bạn không có quyền truy cập workspace này."));
            }

            try
            {
                var response = await _aiService.ContextChatAsync(userId, request);
                return Ok(ApiResponse<AiContextChatResponseDto>.Success(response));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Error(ex.Message));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("command")]
        public async Task<IActionResult> Command([FromBody] AiCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(ApiResponse<object>.Error("Prompt khong duoc de trong."));
            }

            var userId = GetUserId();
            var prompt = request.Prompt.Trim();
            var locale = string.Equals(request.Locale, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
            var lowered = prompt.ToLowerInvariant();

            if (request.ProjectId.HasValue && request.ProjectId.Value != Guid.Empty)
            {
                if (!await UserHasProjectAccessAsync(userId, request.ProjectId.Value))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error(locale == "en"
                        ? "You do not have permission to access this project."
                        : "Bạn không có quyền truy cập dự án này."));
                }
            }

            if (ContainsAny(lowered, "tao task", "t\u1ea1o task", "tao cong viec", "t\u1ea1o c\u00f4ng vi\u1ec7c", "create task", "add task"))
            {
                if (request.ProjectId == null || request.ProjectId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<object>.Error(locale == "en"
                        ? "Please choose a project before asking AI to create a task."
                        : "B\u1ea1n c\u1ea7n ch\u1ecdn project tr\u01b0\u1edbc khi y\u00eau c\u1ea7u AI t\u1ea1o task."));
                }

                var title = ExtractTaskTitle(prompt);
                var created = await _workTaskService.CreateAsync(userId, new CreateWorkTaskDto
                {
                    ProjectId = request.ProjectId.Value,
                    Title = title,
                    Description = locale == "en"
                        ? $"Created by SprintA AI from prompt:\n{prompt}"
                        : $"\u0110\u01b0\u1ee3c t\u1ea1o b\u1edfi SprintA AI t\u1eeb prompt:\n{prompt}",
                    TypeName = "Task",
                    Priority = InferPromptPriority(lowered),
                    StoryPoints = 0
                });

                return Ok(ApiResponse<object>.Success(new
                {
                    action = "create-task",
                    createdTask = created,
                    message = locale == "en"
                        ? $"Created task \"{created.Title}\" in the selected project."
                        : $"\u0110\u00e3 t\u1ea1o task \"{created.Title}\" trong project \u0111ang ch\u1ecdn."
                }));
            }

            if (request.ProjectId.HasValue && ContainsAny(lowered, "thong ke", "th\u1ed1ng k\u00ea", "tong ket", "t\u1ed5ng k\u1ebft", "summary", "report", "bao cao", "b\u00e1o c\u00e1o"))
            {
                var stats = await BuildProjectStatsAsync(request.ProjectId.Value);
                var message = locale == "en"
                    ? $"Project summary: {stats.Total} tasks, {stats.Done} done, {stats.InProgress} in progress, {stats.Todo} to do, {stats.Overdue} overdue."
                    : $"Th\u1ed1ng k\u00ea d\u1ef1 \u00e1n: {stats.Total} task, {stats.Done} \u0111\u00e3 xong, {stats.InProgress} \u0111ang l\u00e0m, {stats.Todo} c\u1ea7n l\u00e0m, {stats.Overdue} qu\u00e1 h\u1ea1n.";

                return Ok(ApiResponse<object>.Success(new
                {
                    action = "project-summary",
                    stats,
                    message
                }));
            }

            try
            {
                var languageInstruction = locale == "en"
                    ? "Answer in English. If you suggest tasks, use clear titles and explain that the user can ask 'create task ...' to create a real task."
                    : "Tr\u1ea3 l\u1eddi b\u1eb1ng ti\u1ebfng Vi\u1ec7t c\u00f3 d\u1ea5u. N\u1ebfu g\u1ee3i \u00fd task, h\u00e3y vi\u1ebft ti\u00eau \u0111\u1ec1 r\u00f5 r\u00e0ng v\u00e0 nh\u1eafc r\u1eb1ng ng\u01b0\u1eddi d\u00f9ng c\u00f3 th\u1ec3 nh\u1eadp 't\u1ea1o task ...' \u0111\u1ec3 t\u1ea1o task th\u1eadt.";
                var response = await _aiService.ChatAsync(userId, new AiChatRequestDto
                {
                    Message = $"{languageInstruction}\n\nProjectId: {request.ProjectId?.ToString() ?? "none"}\nPrompt: {prompt}",
                    History = request.History?.Select(h => new TaskManagement.Application.DTOs.AI.AiChatMessageDto
                    {
                        Role = h.Role,
                        Content = h.Content
                    }).ToList()
                });

                return Ok(ApiResponse<object>.Success(new
                {
                    action = "chat",
                    message = response
                }));
            }
            catch (Exception ex) when (IsTransientAiFailure(ex) || ex is InvalidOperationException)
            {
                return Ok(ApiResponse<object>.Success(new
                {
                    action = "local-fallback",
                    message = BuildLocalizedLocalChatFallback(prompt, locale)
                }));
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

        private static bool IsGeminiQuotaFailure(Exception ex)
        {
            var message = ex.Message ?? string.Empty;

            return message.Contains("Gemini API loi 429", StringComparison.OrdinalIgnoreCase)
                || message.Contains("Gemini Multimodal API loi 429", StringComparison.OrdinalIgnoreCase)
                || message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase)
                || message.Contains("GenerateRequestsPerDayPerProjectPerModel-FreeTier", StringComparison.OrdinalIgnoreCase)
                || message.Contains("rate-limits", StringComparison.OrdinalIgnoreCase)
                || message.Contains("quota exceeded", StringComparison.OrdinalIgnoreCase);
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

        private static string BuildLocalizedLocalChatFallback(string message, string locale)
        {
            if (locale == "en")
            {
                return
                    "Gemini is not available right now, so SprintA is using a local fallback.\n\n" +
                    $"Your request: \"{message}\"\n\n" +
                    "You can ask me to create a real task with: create task <title>. " +
                    "You can also ask for a project summary, report, priorities, or a task breakdown.";
            }

            return
                "Gemini hiá»‡n chÆ°a pháº£n há»“i á»•n Ä‘á»‹nh, nÃªn SprintA Ä‘ang dÃ¹ng xá»­ lÃ½ cá»¥c bá»™.\n\n" +
                $"YÃªu cáº§u cá»§a báº¡n: \"{message}\"\n\n" +
                "Báº¡n cÃ³ thá»ƒ nháº­p: táº¡o task <tiÃªu Ä‘á»> Ä‘á»ƒ AI táº¡o task tháº­t. " +
                "Báº¡n cÅ©ng cÃ³ thá»ƒ yÃªu cáº§u: thá»‘ng kÃª dá»± Ã¡n, tÃ³m táº¯t cÃ´ng viá»‡c, Ä‘á» xuáº¥t Æ°u tiÃªn, hoáº·c breakdown task.";
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static string ExtractTaskTitle(string prompt)
        {
            var patterns = new[]
            {
                @"(?:táº¡o|tao|create|add)\s+(?:task|cÃ´ng viá»‡c|cong viec)\s*[:\-]?\s*(?<title>.+)",
                @"(?:task|cÃ´ng viá»‡c|cong viec)\s*[:\-]\s*(?<title>.+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(prompt, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    var title = match.Groups["title"].Value.Trim().Trim('"', '\'', '.', ';');
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        return title.Length > 180 ? title[..180] : title;
                    }
                }
            }

            var fallback = prompt.Trim();
            return fallback.Length > 180 ? fallback[..180] : fallback;
        }

        private static int InferPromptPriority(string loweredPrompt)
        {
            if (ContainsAny(loweredPrompt, "kháº©n", "khan", "urgent", "gáº¥p", "gap", "p1")) return 1;
            if (ContainsAny(loweredPrompt, "cao", "high", "p2")) return 2;
            if (ContainsAny(loweredPrompt, "tháº¥p", "thap", "low", "p4")) return 4;
            return 3;
        }

        private async Task<ProjectAiStats> BuildProjectStatsAsync(Guid projectId)
        {
            var now = DateTime.UtcNow;
            var tasks = await _dbContext.WorkTasks
                .AsNoTracking()
                .Where(task => task.ProjectId == projectId && !task.IsDeleted)
                .Select(task => new
                {
                    task.Id,
                    task.DueDate,
                    StatusName = task.TaskStatus != null ? task.TaskStatus.Name : null
                })
                .ToListAsync();

            static bool IsDone(string? status) =>
                !string.IsNullOrWhiteSpace(status) &&
                (status.Contains("done", StringComparison.OrdinalIgnoreCase) ||
                 status.Contains("complete", StringComparison.OrdinalIgnoreCase) ||
                 status.Contains("xong", StringComparison.OrdinalIgnoreCase));

            static bool IsInProgress(string? status) =>
                !string.IsNullOrWhiteSpace(status) &&
                (status.Contains("progress", StringComparison.OrdinalIgnoreCase) ||
                 status.Contains("doing", StringComparison.OrdinalIgnoreCase) ||
                 status.Contains("Ä‘ang", StringComparison.OrdinalIgnoreCase) ||
                 status.Contains("dang", StringComparison.OrdinalIgnoreCase));

            var done = tasks.Count(task => IsDone(task.StatusName));
            var inProgress = tasks.Count(task => IsInProgress(task.StatusName));
            var overdue = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value < now && !IsDone(task.StatusName));

            return new ProjectAiStats
            {
                Total = tasks.Count,
                Done = done,
                InProgress = inProgress,
                Todo = Math.Max(0, tasks.Count - done - inProgress),
                Overdue = overdue
            };
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

        private async Task<bool> UserHasProjectAccessAsync(Guid userId, Guid projectId)
        {
            var workspaceMember = await _dbContext.WorkspaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(wm => wm.UserId == userId && wm.IsActive);

            if (workspaceMember != null)
            {
                var wsRole = workspaceMember.WorkspaceRole.Trim().ToUpperInvariant();
                if (wsRole == "OWNER" || wsRole == "ADMIN")
                {
                    return true;
                }
            }

            return await _dbContext.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);
        }

        private async Task<bool> UserHasProjectContextAccessAsync(Guid userId, Guid workspaceId, Guid projectId)
        {
            var workspaceRole = await _dbContext.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.UserId == userId && wm.WorkspaceId == workspaceId && wm.IsActive)
                .Select(wm => wm.WorkspaceRole)
                .FirstOrDefaultAsync();

            if (string.Equals(workspaceRole, "OWNER", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(workspaceRole, "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return await _dbContext.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);
        }

        private Task<bool> UserHasWorkspaceAccessAsync(Guid userId, Guid workspaceId)
        {
            return _dbContext.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.UserId == userId && wm.WorkspaceId == workspaceId && wm.IsActive);
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

    public class AiCommandRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public Guid? ProjectId { get; set; }
        public string? Locale { get; set; }
        public List<TaskManagement.Application.Interfaces.AiChatMessageDto>? History { get; set; }
    }

    public class ProjectAiStats
    {
        public int Total { get; set; }
        public int Done { get; set; }
        public int InProgress { get; set; }
        public int Todo { get; set; }
        public int Overdue { get; set; }
    }
}
