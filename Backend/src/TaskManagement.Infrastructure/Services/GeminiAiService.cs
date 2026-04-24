using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs.AI;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IWorkTaskService _workTaskService;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly string[] RepositoryExecutionProjectRoles = { "PM", "PO", "SM", "Project Lead", "PROJECT_MANAGER", "PROJECT_LEAD", "Admin" };
        private static readonly string[] AssigneeSuggestionProjectRoles = { "PM", "PO", "SM", "Project Lead", "PROJECT_MANAGER", "PROJECT_LEAD", "SCRUM_MASTER", "Admin" };
        private static readonly string[] SystemExecutionRoles = { "Admin", "System Admin", "SuperAdmin", "Organization Admin" };

        public GeminiAiService(
            ApplicationDbContext context,
            HttpClient httpClient,
            IWorkTaskService workTaskService,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _workTaskService = workTaskService;
            _configuration = configuration;
        }

        public async Task<string> ChatAsync(Guid userId, AiChatRequestDto request)
        {
            await EnsureQuotaAsync(userId);

            var history = request.History?
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .TakeLast(10)
                .Select(m => $"{m.Role}: {m.Content}")
                .ToList() ?? new List<string>();

            var prompt = $"""
            You are SprintA AI, a concise Vietnamese project-management assistant.
            Help with tasks, projects, cycles, modules, Agile planning, summaries, and risk analysis.
            Do not invent private data. If project context is missing, ask for the missing detail.

            Recent conversation:
            {string.Join("\n", history)}

            User message:
            {request.Message}
            """;

            var result = await GenerateTextAsync(userId, "ai-chat", prompt);
            return string.IsNullOrWhiteSpace(result.Text)
                ? "Mình chưa tạo được phản hồi từ AI. Hãy kiểm tra API key Gemini hoặc thử lại sau."
                : result.Text.Trim();
        }

        public async Task<string> GenerateDescriptionAsync(Guid userId, AiGenerateDescriptionRequestDto request)
        {
            await EnsureQuotaAsync(userId);

            var prompt = $"""
            You are SprintA AI. Write a clear work item description in Vietnamese Markdown.
            Return only the description, no surrounding explanation.

            Work item prompt:
            {request.Prompt}

            Optional context:
            {request.Context}
            """;

            var result = await GenerateTextAsync(userId, "generate-description", prompt);
            return result.Text.Trim();
        }

        public async Task<List<AiSubTaskDto>> BreakdownTaskAsync(Guid userId, AiBreakdownRequestDto request)
        {
            await EnsureQuotaAsync(userId);

            var prompt = $$"""
            You are SprintA AI. Break the parent work item into practical sub-work items.
            Return STRICT JSON only, matching this schema:
            [
              {
                "title": "short actionable title",
                "description": "implementation detail",
                "estHours": 2,
                "priority": 3
              }
            ]
            Rules:
            - Return 3 to 7 subtasks.
            - priority is an integer: 1 urgent, 2 high, 3 medium, 4 low.
            - estHours is a number from 0.5 to 16.
            - No markdown, no code fences, no prose outside JSON.

            Parent title: {{request.Title}}
            Parent description: {{request.Description}}
            """;

            try
            {
                var result = await GenerateTextAsync(userId, "breakdown-task", prompt, forceJson: true);
                return DeserializeSubtasks(result.Text);
            }
            catch (Exception ex) when (CanUseBreakdownFallback(ex))
            {
                return BuildBreakdownFallback(request);
            }
        }

        public async Task<List<WorkTaskResponseDto>> BreakdownAndCreateSubtasksAsync(Guid userId, AiBreakdownRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                throw new ArgumentException("ProjectId la bat buoc khi tao sub-work items bang AI.");
            }

            if (!request.ParentTaskId.HasValue)
            {
                throw new ArgumentException("ParentTaskId la bat buoc khi tao sub-work items bang AI.");
            }

            var subtasks = await BreakdownTaskAsync(userId, request);
            return await CreateSubtasksAsync(userId, request.ProjectId, request.ParentTaskId.Value, subtasks);
        }

        public async Task<List<WorkTaskResponseDto>> CreateSubtasksFromPreviewAsync(Guid userId, AiCreateSubtasksFromPreviewRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                throw new ArgumentException("ProjectId la bat buoc khi tao sub-work items tu preview.");
            }

            if (request.ParentTaskId == Guid.Empty)
            {
                throw new ArgumentException("ParentTaskId la bat buoc khi tao sub-work items tu preview.");
            }

            var subtasks = (request.Subtasks ?? new List<AiSubTaskDto>())
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Take(10)
                .ToList();

            if (!subtasks.Any())
            {
                throw new ArgumentException("Preview sub-work items dang rong.");
            }

            return await CreateSubtasksAsync(userId, request.ProjectId, request.ParentTaskId, subtasks);
        }

        private async Task<List<WorkTaskResponseDto>> CreateSubtasksAsync(Guid userId, Guid projectId, Guid parentTaskId, List<AiSubTaskDto> subtasks)
        {
            var created = new List<WorkTaskResponseDto>();

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                created.Clear();
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var subtask in subtasks)
                    {
                        var dto = new CreateWorkTaskDto
                        {
                            ProjectId = projectId,
                            ParentTaskId = parentTaskId,
                            Title = subtask.Title,
                            Description = subtask.Description,
                            StatusName = "TO DO",
                            Priority = subtask.Priority is >= 1 and <= 4 ? subtask.Priority : 3,
                            TotalEstimatedHours = Math.Max(0, subtask.EstHours)
                        };

                        created.Add(await _workTaskService.CreateAsync(userId, dto));
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return created;
        }

        public async Task<AiEstimateSuggestionDto> SuggestEstimateAsync(Guid userId, AiEstimateSuggestionRequestDto request)
        {
            await EnsureQuotaAsync(userId);

            var prompt = $$"""
            You are SprintA AI. Suggest an estimate for a work item.
            Return STRICT JSON only with this schema:
            {
              "suggestedHours": 8,
              "suggestedStoryPoints": 3,
              "suggestedDays": 2,
              "complexity": "Medium",
              "reasoning": "short explanation"
            }

            Rules:
            - suggestedHours: number from 0.5 to 80
            - suggestedStoryPoints: one of 1, 2, 3, 5, 8, 13
            - suggestedDays: integer from 1 to 10
            - complexity: one of Low, Medium, High, Critical
            - reasoning: max 220 chars
            - Consider multi-assignee collaboration and subtask count.
            - No markdown, no prose outside JSON.

            Work item title: {{request.Title}}
            Description: {{request.Description}}
            Priority: {{request.Priority}}
            Existing story points: {{request.StoryPoints}}
            Assignee count: {{request.AssigneeCount}}
            Subtask count: {{request.SubtaskCount}}
            """;

            try
            {
                var result = await GenerateTextAsync(userId, "suggest-estimate", prompt, forceJson: true);
                return DeserializeEstimateSuggestion(result.Text);
            }
            catch (Exception ex) when (CanUseEstimateFallback(ex))
            {
                return BuildEstimateFallback(request);
            }
        }

        public async Task<AiAssigneeSuggestionDto> SuggestAssigneesAsync(Guid userId, AiAssigneeSuggestionRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                throw new ArgumentException("ProjectId la bat buoc.");
            }

            await EnsureAiAssigneeSuggestionAccessAsync(userId, request.ProjectId);

            var members = await _context.ProjectMembers
                .AsNoTracking()
                .Where(member => member.ProjectId == request.ProjectId && member.Status)
                .Select(member => new
                {
                    member.UserId,
                    member.ProjectRole,
                    FullName = member.User.FullName ?? member.User.Email,
                    member.User.Email
                })
                .ToListAsync();

            if (!members.Any())
            {
                throw new InvalidOperationException("Project nay chua co thanh vien hoat dong de AI goi y.");
            }

            var allProjectAssignments = await _context.TaskAssignments
                .AsNoTracking()
                .Where(assignment => assignment.WorkTask.ProjectId == request.ProjectId && !assignment.WorkTask.IsDeleted)
                .Select(assignment => new
                {
                    assignment.UserId,
                    assignment.Status,
                    assignment.EstimatedHours,
                    assignment.TotalActualHours,
                    assignment.ProgressPercent,
                    assignment.WorkTaskId,
                    StoryPoints = assignment.WorkTask.StoryPoints,
                    StatusName = assignment.WorkTask.TaskStatus.Name
                })
                .ToListAsync();

            var activeAssignments = allProjectAssignments.Where(item => item.Status).ToList();
            var completedAssignments = allProjectAssignments
                .Where(item => IsDoneStatusName(item.StatusName))
                .ToList();

            var recentLoggedHours = await _context.TimeLogs
                .AsNoTracking()
                .Where(log =>
                    log.WorkTask.ProjectId == request.ProjectId &&
                    !log.WorkTask.IsDeleted &&
                    log.LoggedAt >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(log => log.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    LoggedHours = group.Sum(item => item.Hours)
                })
                .ToDictionaryAsync(item => item.UserId, item => item.LoggedHours);

            var maxCompletedStoryPoints = Math.Max(1, completedAssignments
                .GroupBy(item => item.UserId)
                .Select(group => group.Sum(item => Math.Max(0, item.StoryPoints)))
                .DefaultIfEmpty(0)
                .Max());

            var taskEstimate = Math.Max(Math.Max(0, request.EstimatedHours), InferAssigneePlanningEstimate(request));
            var candidateCount = Math.Clamp(request.CandidateCount <= 0 ? 3 : request.CandidateCount, 1, 5);

            var candidates = members
                .Select(member =>
                {
                    var memberActiveAssignments = activeAssignments.Where(item => item.UserId == member.UserId).ToList();
                    var memberCompletedAssignments = completedAssignments.Where(item => item.UserId == member.UserId).ToList();

                    var activeTaskCount = memberActiveAssignments.Select(item => item.WorkTaskId).Distinct().Count();
                    var activeEstimatedHours = Math.Round(memberActiveAssignments.Sum(item => Math.Max(0, item.EstimatedHours)), 1);
                    var completedStoryPoints = Math.Round(memberCompletedAssignments.Sum(item => Math.Max(0, item.StoryPoints)), 1);
                    var accuracyValues = memberCompletedAssignments
                        .Where(item => item.EstimatedHours > 0 || item.TotalActualHours > 0)
                        .Select(item =>
                        {
                            var estimate = Math.Max(0, item.EstimatedHours);
                            var actual = Math.Max(0, item.TotalActualHours);
                            if (estimate <= 0)
                            {
                                return actual <= 0 ? 100d : 0d;
                            }

                            return Math.Max(0, Math.Round(100 - (Math.Abs(actual - estimate) / estimate * 100), 1));
                        })
                        .ToList();

                    var averageAccuracy = Math.Round(accuracyValues.DefaultIfEmpty(100).Average(), 1);
                    var loggedHours = Math.Round(recentLoggedHours.GetValueOrDefault(member.UserId), 1);

                    var velocityScore = Math.Min(1, completedStoryPoints / maxCompletedStoryPoints);
                    var accuracyScore = Math.Min(1, averageAccuracy / 100d);
                    var workloadScore = 1 - Math.Min(1, activeEstimatedHours / 40d);
                    var taskLoadScore = 1 - Math.Min(1, activeTaskCount / 8d);
                    var recentActivityScore = Math.Min(1, loggedHours / 40d);
                    var estimateFitScore = taskEstimate <= 0
                        ? 0.5
                        : 1 - Math.Min(1, Math.Abs(activeEstimatedHours - taskEstimate) / Math.Max(taskEstimate, 1));

                    var fitScore = Math.Round(
                        (velocityScore * 0.28) +
                        (accuracyScore * 0.26) +
                        (workloadScore * 0.20) +
                        (taskLoadScore * 0.10) +
                        (recentActivityScore * 0.10) +
                        (estimateFitScore * 0.06),
                        3);

                    return new AiAssigneeSuggestionCandidateDto
                    {
                        UserId = member.UserId,
                        FullName = member.FullName ?? member.Email ?? "Member",
                        Email = member.Email,
                        ProjectRole = member.ProjectRole ?? string.Empty,
                        ActiveTaskCount = activeTaskCount,
                        ActiveEstimatedHours = activeEstimatedHours,
                        CompletedStoryPoints = completedStoryPoints,
                        AverageAccuracyPercent = averageAccuracy,
                        LoggedHoursLast30Days = loggedHours,
                        FitScore = fitScore,
                        SuggestedContributionWeight = 1,
                        SuggestedEstimatedHours = 0,
                        Reasoning = BuildAssigneeReasoning(member.ProjectRole, completedStoryPoints, averageAccuracy, activeEstimatedHours, activeTaskCount)
                    };
                })
                .OrderByDescending(item => item.FitScore)
                .ThenByDescending(item => item.CompletedStoryPoints)
                .ThenBy(item => item.ActiveEstimatedHours)
                .Take(candidateCount)
                .ToList();

            if (!candidates.Any())
            {
                throw new InvalidOperationException("AI khong tim duoc assignee phu hop trong project.");
            }

            var recommendedAssigneeCount = SuggestRecommendedAssigneeCount(request.StoryPoints, taskEstimate);
            var selectedForSplit = candidates.Take(recommendedAssigneeCount).ToList();
            var totalFit = Math.Max(0.001, selectedForSplit.Sum(item => Math.Max(0.05, item.FitScore)));

            foreach (var candidate in selectedForSplit)
            {
                var share = Math.Max(0.05, candidate.FitScore) / totalFit;
                candidate.SuggestedContributionWeight = Math.Round(share, 2);
                candidate.SuggestedEstimatedHours = taskEstimate > 0
                    ? Math.Round(taskEstimate * share, 1)
                    : 0;
            }

            foreach (var candidate in candidates.Skip(recommendedAssigneeCount))
            {
                candidate.SuggestedContributionWeight = 0;
                candidate.SuggestedEstimatedHours = 0;
            }

            return new AiAssigneeSuggestionDto
            {
                Summary = BuildAssigneeSuggestionSummary(candidates, request.Title, recommendedAssigneeCount),
                RecommendedAssigneeCount = recommendedAssigneeCount,
                Suggestions = candidates
            };
        }

        public async Task<AiRepositoryAnalysisDto> AnalyzeRepositoryAsync(Guid userId, AiRepositoryAnalysisRequestDto request)
        {
            await EnsureQuotaAsync(userId);

            var repo = ParseRepository(request.RepoUrl);
            if (repo == null)
            {
                throw new ArgumentException("Repo URL khong dung dinh dang GitHub.");
            }

            var snapshot = await FetchGitHubSnapshotAsync(repo.Owner, repo.Repository, request.GitHubToken);
            var prompt = $$"""
            You are SprintA AI. Analyze a software repository and propose implementation backlog.
            Return STRICT JSON only with this schema:
            {
              "repository": "owner/repo",
              "summary": "short repository summary",
              "quickWins": [
                {
                  "title": "small useful task",
                  "category": "quick-win",
                  "suggestedHours": 3,
                  "priority": 2,
                  "reasoning": "why"
                }
              ],
              "mediumTasks": [
                {
                  "title": "medium task",
                  "category": "medium",
                  "suggestedHours": 8,
                  "priority": 3,
                  "reasoning": "why"
                }
              ],
              "riskyTasks": [
                {
                  "title": "risky task",
                  "category": "risky",
                  "suggestedHours": 16,
                  "priority": 1,
                  "reasoning": "why"
                }
              ],
              "testPlan": ["test item 1", "test item 2"],
              "suggestedPrompt": "a Vietnamese prompt for SprintA AI to continue planning from this repo"
            }

            Rules:
            - quickWins: 2 to 4 items
            - mediumTasks: 2 to 4 items
            - riskyTasks: 1 to 3 items
            - suggestedHours: between 1 and 40
            - priority: integer 1 urgent, 2 high, 3 medium, 4 low
            - testPlan: 3 to 6 concise bullets
            - suggestedPrompt must be in Vietnamese and usable directly in chat
            - no markdown, no prose outside JSON

            Focus: {{request.Focus ?? "Repository planning, task breakdown, and implementation risks"}}
            Repository: {{snapshot.Repository}}
            Description: {{snapshot.Description}}
            Languages: {{snapshot.LanguagesText}}
            Open issues: {{snapshot.OpenIssuesText}}
            README snippet: {{snapshot.ReadmeSnippet}}
            """;

            try
            {
                var result = await GenerateTextAsync(userId, "repo-analysis", prompt, forceJson: true);
                return DeserializeRepositoryAnalysis(result.Text, snapshot.Repository);
            }
            catch (Exception ex) when (CanUseRepositoryFallback(ex))
            {
                return BuildRepositoryFallbackAnalysis(snapshot);
            }
        }

        public async Task<List<WorkTaskResponseDto>> CreateBacklogItemsFromAnalysisAsync(Guid userId, AiCreateBacklogFromAnalysisRequestDto request)
        {
            if (request.ProjectId == Guid.Empty)
            {
                throw new ArgumentException("ProjectId la bat buoc.");
            }

            await EnsureRepositoryExecutionAccessAsync(userId, request.ProjectId);

            var selectedItems = new List<AiRepositoryBacklogItemDto>();
            if (request.SelectedItems != null && request.SelectedItems.Count > 0)
            {
                selectedItems.AddRange(NormalizeBacklogItems(request.SelectedItems, "selected"));
            }
            else
            {
                if (request.IncludeQuickWins)
                {
                    selectedItems.AddRange(NormalizeBacklogItems(request.QuickWins, "quick-win"));
                }

                if (request.IncludeMediumTasks)
                {
                    selectedItems.AddRange(NormalizeBacklogItems(request.MediumTasks, "medium"));
                }

                if (request.IncludeRiskyTasks)
                {
                    selectedItems.AddRange(NormalizeBacklogItems(request.RiskyTasks, "risky"));
                }
            }

            if (!selectedItems.Any())
            {
                throw new ArgumentException("Khong co backlog item nao duoc chon de tao.");
            }

            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted);

            if (project == null)
            {
                throw new ArgumentException("Project khong ton tai hoac da bi xoa.");
            }

            if (request.TargetSprintId.HasValue)
            {
                var sprintExists = await _context.Sprints
                    .AsNoTracking()
                    .AnyAsync(sprint => sprint.Id == request.TargetSprintId.Value && sprint.ProjectId == request.ProjectId);

                if (!sprintExists)
                {
                    throw new ArgumentException("Cycle dich khong thuoc du an nay.");
                }
            }

            var reporterId = await ResolveReporterIdForProjectAsync(userId, request.ProjectId, project.CreatorId);
            var created = new List<WorkTaskResponseDto>();
            foreach (var item in selectedItems)
            {
                var dto = new CreateWorkTaskDto
                {
                    ProjectId = request.ProjectId,
                    Title = item.Title.Trim(),
                    Description = BuildRepositoryBacklogDescription(item, request.Repository),
                    StatusName = "TO DO",
                    TypeName = "Task",
                    Priority = Math.Clamp(item.Priority, 1, 4),
                    StoryPoints = SuggestStoryPointsFromHours(item.SuggestedHours),
                    SprintId = request.TargetSprintId,
                    TotalEstimatedHours = Math.Round(Math.Max(1, item.SuggestedHours), 1)
                };

                created.Add(await _workTaskService.CreateAsync(reporterId, dto));
            }

            return created;
        }

        public async Task<AiUsageDto> GetUsageAsync(Guid userId)
        {
            var quota = GetMonthlyQuota();
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var used = await _context.AITokenUsages
                .Where(x => x.UserId == userId && x.CreatedAt >= monthStart)
                .SumAsync(x => (long?)x.TokensUsed) ?? 0;

            return new AiUsageDto
            {
                UsedTokensThisMonth = used,
                MonthlyTokenQuota = quota
            };
        }

        private async Task EnsureQuotaAsync(Guid userId)
        {
            var usage = await GetUsageAsync(userId);
            if (usage.UsedTokensThisMonth >= usage.MonthlyTokenQuota)
            {
                throw new InvalidOperationException($"Ban da vuot han muc AI thang nay ({usage.MonthlyTokenQuota:N0} tokens).");
            }
        }

        private async Task<GeminiResult> GenerateTextAsync(Guid userId, string featureCode, string prompt, bool forceJson = false)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("PASTE_YOUR_GEMINI_API_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chua cau hinh Gemini API key. Hay nhap key vao appsettings.json tai Gemini:ApiKey.");
            }

            var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

            var payload = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "You must follow the user requested output format exactly." } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = forceJson ? 0.2 : 0.5,
                    responseMimeType = forceJson ? "application/json" : "text/plain"
                }
            };

            using var response = await _httpClient.PostAsJsonAsync(endpoint, payload, _jsonOptions);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API loi {(int)response.StatusCode}: {responseBody}");
            }

            var result = ParseGeminiResponse(responseBody);
            var fallbackTokenEstimate = Math.Max(1, (prompt.Length + result.Text.Length) / 4);

            _context.AITokenUsages.Add(new AITokenUsage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureCode = featureCode,
                TokensUsed = result.TotalTokens > 0 ? result.TotalTokens : fallbackTokenEstimate,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return result;
        }

        private GeminiResult ParseGeminiResponse(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var text = "";
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];
                if (first.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts))
                {
                    text = string.Join("", parts.EnumerateArray()
                        .Where(part => part.TryGetProperty("text", out _))
                        .Select(part => part.GetProperty("text").GetString()));
                }
            }

            long totalTokens = 0;
            if (root.TryGetProperty("usageMetadata", out var usage) &&
                usage.TryGetProperty("totalTokenCount", out var totalTokenCount))
            {
                totalTokens = totalTokenCount.GetInt64();
            }

            return new GeminiResult(text ?? "", totalTokens);
        }

        private List<AiSubTaskDto> DeserializeSubtasks(string rawText)
        {
            var json = rawText.Trim();
            if (json.StartsWith("```", StringComparison.Ordinal))
            {
                json = json.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "", StringComparison.Ordinal)
                    .Trim();
            }

            var subtasks = JsonSerializer.Deserialize<List<AiSubTaskDto>>(json, _jsonOptions);
            if (subtasks == null || subtasks.Count == 0)
            {
                throw new InvalidOperationException("AI khong tra ve danh sach subtask hop le.");
            }

            return subtasks
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .Take(10)
                .ToList();
        }

        private AiEstimateSuggestionDto DeserializeEstimateSuggestion(string rawText)
        {
            var json = rawText.Trim();
            if (json.StartsWith("```", StringComparison.Ordinal))
            {
                json = json.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "", StringComparison.Ordinal)
                    .Trim();
            }

            var suggestion = JsonSerializer.Deserialize<AiEstimateSuggestionDto>(json, _jsonOptions);
            if (suggestion == null)
            {
                throw new InvalidOperationException("AI khong tra ve estimate suggestion hop le.");
            }

            suggestion.SuggestedHours = Math.Round(Math.Clamp(suggestion.SuggestedHours, 0.5, 80), 1);
            suggestion.SuggestedStoryPoints = NormalizeStoryPoint(suggestion.SuggestedStoryPoints);
            suggestion.SuggestedDays = Math.Clamp(suggestion.SuggestedDays, 1, 10);
            suggestion.Complexity = NormalizeComplexity(suggestion.Complexity);
            suggestion.Reasoning = string.IsNullOrWhiteSpace(suggestion.Reasoning)
                ? "Suggested from task scope, complexity, and collaboration load."
                : suggestion.Reasoning.Trim();
            if (suggestion.Reasoning.StartsWith("Based on ", StringComparison.OrdinalIgnoreCase))
            {
                suggestion.Reasoning = $"Suggested from {suggestion.Reasoning[9..]}";
            }

            return suggestion;
        }

        private AiRepositoryAnalysisDto DeserializeRepositoryAnalysis(string rawText, string repository)
        {
            var json = rawText.Trim();
            if (json.StartsWith("```", StringComparison.Ordinal))
            {
                json = json.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "", StringComparison.Ordinal)
                    .Trim();
            }

            var analysis = JsonSerializer.Deserialize<AiRepositoryAnalysisDto>(json, _jsonOptions);
            if (analysis == null)
            {
                throw new InvalidOperationException("AI khong tra ve repo analysis hop le.");
            }

            analysis.Repository = string.IsNullOrWhiteSpace(analysis.Repository) ? repository : analysis.Repository.Trim();
            analysis.Summary = string.IsNullOrWhiteSpace(analysis.Summary)
                ? "Repository analysis was generated but summary was empty."
                : analysis.Summary.Trim();
            analysis.SuggestedPrompt = string.IsNullOrWhiteSpace(analysis.SuggestedPrompt)
                ? $"Phan tich repo {analysis.Repository} va de xuat backlog, uu tien, rui ro va test plan chi tiet."
                : analysis.SuggestedPrompt.Trim();
            analysis.QuickWins = NormalizeBacklogItems(analysis.QuickWins, "quick-win");
            analysis.MediumTasks = NormalizeBacklogItems(analysis.MediumTasks, "medium");
            analysis.RiskyTasks = NormalizeBacklogItems(analysis.RiskyTasks, "risky");
            analysis.TestPlan = (analysis.TestPlan ?? new List<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Take(6)
                .ToList();

            return analysis;
        }

        private static List<AiRepositoryBacklogItemDto> NormalizeBacklogItems(List<AiRepositoryBacklogItemDto>? items, string category)
        {
            return (items ?? new List<AiRepositoryBacklogItemDto>())
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Take(6)
                .Select(item =>
                {
                    item.Title = item.Title.Trim();
                    item.Category = string.IsNullOrWhiteSpace(item.Category) ? category : item.Category.Trim();
                    item.SuggestedHours = Math.Round(Math.Clamp(item.SuggestedHours, 1, 40), 1);
                    item.Priority = Math.Clamp(item.Priority, 1, 4);
                    item.Reasoning = string.IsNullOrWhiteSpace(item.Reasoning) ? "AI suggested this task from repository context." : item.Reasoning.Trim();
                    return item;
                })
                .ToList();
        }

        private static bool CanUseBreakdownFallback(Exception ex)
        {
            var message = ex.Message?.ToLowerInvariant() ?? string.Empty;

            return ex is InvalidOperationException
                || ex is HttpRequestException
                || ex is TaskCanceledException
                || message.Contains("gemini")
                || message.Contains("service unavailable")
                || message.Contains("temporarily unavailable")
                || message.Contains("timeout")
                || message.Contains("quota")
                || message.Contains("ai khong tra ve");
        }

        private static List<AiSubTaskDto> BuildBreakdownFallback(AiBreakdownRequestDto request)
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

        private static bool IsDoneStatusName(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return false;
            }

            var normalized = statusName.Trim().ToUpperInvariant();
            return normalized.Contains("DONE") || normalized.Contains("COMPLETE") || normalized.Contains("HOAN THANH");
        }

        private static int SuggestRecommendedAssigneeCount(double storyPoints, double estimatedHours)
        {
            if (storyPoints >= 8 || estimatedHours >= 20)
            {
                return 3;
            }

            if (storyPoints >= 5 || estimatedHours >= 10)
            {
                return 2;
            }

            return 1;
        }

        private static double InferAssigneePlanningEstimate(AiAssigneeSuggestionRequestDto request)
        {
            var hours = Math.Max(2, request.StoryPoints > 0 ? request.StoryPoints * 2 : 2);
            var title = (request.Title ?? string.Empty).ToLowerInvariant();

            if (request.Priority == 1) hours += 4;
            if (request.Priority == 2) hours += 2;
            if (request.Priority == 4) hours = Math.Max(1, hours - 0.5);

            if (Regex.IsMatch(title, "(api|integration|refactor|migration|security|payment|deploy)")) hours += 3;
            if (Regex.IsMatch(title, "(bug|fix|hotfix|patch)")) hours += 1.5;
            if (Regex.IsMatch(title, "(ui|ux|copy|docs|content)")) hours = Math.Max(1.5, hours - 0.5);

            return Math.Round(hours * 2) / 2;
        }

        private static string BuildAssigneeReasoning(string? projectRole, double completedStoryPoints, double averageAccuracy, double activeEstimatedHours, int activeTaskCount)
        {
            var accuracyLabel = averageAccuracy switch
            {
                >= 90 => "very accurate estimates",
                >= 75 => "stable estimate accuracy",
                >= 60 => "acceptable estimate accuracy",
                _ => "needs estimate monitoring"
            };

            var loadLabel = activeEstimatedHours switch
            {
                <= 8 => "low current workload",
                <= 20 => "balanced workload",
                <= 32 => "moderate workload",
                _ => "high workload"
            };

            var roleLabel = string.IsNullOrWhiteSpace(projectRole) ? "member" : projectRole.Trim();
            return $"{roleLabel} with {completedStoryPoints:0.#} completed points, {accuracyLabel}, {loadLabel}, and {activeTaskCount} active task(s).";
        }

        private static string BuildAssigneeSuggestionSummary(List<AiAssigneeSuggestionCandidateDto> candidates, string title, int recommendedAssigneeCount)
        {
            if (!candidates.Any())
            {
                return "SprintA AI could not prepare assignee suggestions.";
            }

            var top = candidates.Take(recommendedAssigneeCount).Select(item => item.FullName).ToList();
            var target = string.IsNullOrWhiteSpace(title) ? "this work item" : $"\"{title}\"";
            return recommendedAssigneeCount <= 1
                ? $"SprintA recommends {top.First()} as the primary assignee for {target} based on delivery history, accuracy, and current workload."
                : $"SprintA recommends {string.Join(", ", top)} as the suggested team for {target}, balancing velocity, estimate accuracy, and current workload.";
        }

        private async Task EnsureRepositoryExecutionAccessAsync(Guid userId, Guid projectId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var isSystemAllowed = user?.UserRoles?.Any(ur =>
                ur.Role != null &&
                SystemExecutionRoles.Contains(ur.Role.Name, StringComparer.OrdinalIgnoreCase)) == true;

            if (isSystemAllowed)
            {
                return;
            }

            var projectRole = await _context.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status)
                .Select(pm => pm.ProjectRole)
                .FirstOrDefaultAsync();

            var normalizedProjectRole = ProjectExecutionRuleHelper.NormalizeProjectRole(projectRole);

            if (string.IsNullOrWhiteSpace(normalizedProjectRole) ||
                !RepositoryExecutionProjectRoles.Any(role => ProjectExecutionRuleHelper.NormalizeProjectRole(role) == normalizedProjectRole))
            {
                throw new UnauthorizedAccessException("Ban khong co quyen tao backlog AI cho project nay.");
            }
        }

        private async Task EnsureAiAssigneeSuggestionAccessAsync(Guid userId, Guid projectId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var isSystemAllowed = user?.UserRoles?.Any(ur =>
                ur.Role != null &&
                SystemExecutionRoles.Contains(ur.Role.Name, StringComparer.OrdinalIgnoreCase)) == true;

            if (isSystemAllowed)
            {
                return;
            }

            var projectRole = await _context.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status)
                .Select(pm => pm.ProjectRole)
                .FirstOrDefaultAsync();

            var normalizedProjectRole = ProjectExecutionRuleHelper.NormalizeProjectRole(projectRole);

            if (string.IsNullOrWhiteSpace(normalizedProjectRole) ||
                !AssigneeSuggestionProjectRoles.Any(role => ProjectExecutionRuleHelper.NormalizeProjectRole(role) == normalizedProjectRole))
            {
                throw new UnauthorizedAccessException("Ban khong co quyen dung AI goi y assignee cho project nay.");
            }
        }

        private static bool CanUseEstimateFallback(Exception ex)
        {
            var message = ex.Message?.ToLowerInvariant() ?? string.Empty;
            return ex is InvalidOperationException
                || ex is HttpRequestException
                || ex is TaskCanceledException
                || message.Contains("gemini")
                || message.Contains("service unavailable")
                || message.Contains("temporarily unavailable")
                || message.Contains("timeout")
                || message.Contains("quota")
                || message.Contains("estimate suggestion hop le")
                || message.Contains("ai khong tra ve");
        }

        private static AiEstimateSuggestionDto BuildEstimateFallback(AiEstimateSuggestionRequestDto request)
        {
            var suggestedStoryPoints = NormalizeStoryPoint(request.StoryPoints <= 0 ? 3 : request.StoryPoints);
            var suggestedHours = 4d;

            suggestedHours += suggestedStoryPoints switch
            {
                <= 1 => 0,
                <= 2 => 1.5,
                <= 3 => 3,
                <= 5 => 7,
                <= 8 => 14,
                _ => 22
            };

            suggestedHours += request.Priority switch
            {
                1 => 2,
                2 => 1,
                4 => -1,
                _ => 0
            };

            suggestedHours += Math.Max(0, request.AssigneeCount - 1);
            suggestedHours += Math.Min(6, Math.Max(0, request.SubtaskCount));

            var lowered = (request.Title ?? string.Empty).ToLowerInvariant();
            if (lowered.Contains("api") || lowered.Contains("integration") || lowered.Contains("payment") || lowered.Contains("deploy") || lowered.Contains("security"))
            {
                suggestedHours += 3;
            }

            if (lowered.Contains("refactor") || lowered.Contains("migration"))
            {
                suggestedHours += 4;
            }

            if (lowered.Contains("bug") || lowered.Contains("fix") || lowered.Contains("hotfix") || lowered.Contains("patch"))
            {
                suggestedHours += 1.5;
            }

            if (lowered.Contains("ui") || lowered.Contains("ux") || lowered.Contains("copy") || lowered.Contains("docs") || lowered.Contains("content"))
            {
                suggestedHours -= 1;
            }

            suggestedHours = Math.Round(Math.Clamp(suggestedHours, 0.5, 80), 1);

            return new AiEstimateSuggestionDto
            {
                SuggestedHours = suggestedHours,
                SuggestedStoryPoints = suggestedStoryPoints,
                SuggestedDays = Math.Clamp((int)Math.Ceiling(suggestedHours / 6d), 1, 10),
                Complexity = NormalizeComplexity(suggestedHours <= 6 ? "Low" : suggestedHours <= 18 ? "Medium" : suggestedHours <= 32 ? "High" : "Critical"),
                Reasoning = "Suggested from priority, story points, assignee count, subtask count, and task title keywords."
            };
        }

        private async Task<Guid> ResolveReporterIdForProjectAsync(Guid userId, Guid projectId, Guid creatorId)
        {
            var isProjectMember = await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);

            if (isProjectMember)
            {
                return userId;
            }

            var creatorIsMember = creatorId != Guid.Empty && await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == creatorId && pm.Status);

            if (creatorIsMember)
            {
                return creatorId;
            }

            var firstMember = await _context.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == projectId && pm.Status)
                .OrderBy(pm => pm.JoinedAt)
                .Select(pm => pm.UserId)
                .FirstOrDefaultAsync();

            if (firstMember != Guid.Empty)
            {
                return firstMember;
            }

            throw new InvalidOperationException("Project nay chua co member hop le de tao work items.");
        }

        private static string BuildRepositoryBacklogDescription(AiRepositoryBacklogItemDto item, string repository)
        {
            var category = string.IsNullOrWhiteSpace(item.Category) ? "analysis" : item.Category.Trim();
            var reasoning = string.IsNullOrWhiteSpace(item.Reasoning) ? "No additional reasoning provided." : item.Reasoning.Trim();

            return $"AI-generated from repository: {repository}\nCategory: {category}\nSuggested estimate: {Math.Round(Math.Max(1, item.SuggestedHours), 1)}h\n\nReasoning:\n{reasoning}";
        }

        private static double SuggestStoryPointsFromHours(double hours)
        {
            var normalized = Math.Max(1, hours);

            if (normalized <= 2) return 1;
            if (normalized <= 4) return 2;
            if (normalized <= 8) return 3;
            if (normalized <= 16) return 5;
            if (normalized <= 24) return 8;
            return 13;
        }

        private static bool CanUseRepositoryFallback(Exception ex)
        {
            var message = ex.Message?.ToLowerInvariant() ?? string.Empty;

            return ex is InvalidOperationException
                || ex is HttpRequestException
                || ex is TaskCanceledException
                || message.Contains("gemini")
                || message.Contains("service unavailable")
                || message.Contains("temporarily unavailable")
                || message.Contains("timeout")
                || message.Contains("quota")
                || message.Contains("ai khong tra ve");
        }

        private static AiRepositoryAnalysisDto BuildRepositoryFallbackAnalysis(GitHubRepositorySnapshot snapshot)
        {
            var hasIssues = !string.IsNullOrWhiteSpace(snapshot.OpenIssuesText)
                && !snapshot.OpenIssuesText.Contains("Khong co issue", StringComparison.OrdinalIgnoreCase)
                && !snapshot.OpenIssuesText.Contains("Khong doc duoc", StringComparison.OrdinalIgnoreCase);
            var languageHint = string.IsNullOrWhiteSpace(snapshot.LanguagesText)
                ? "unknown stack"
                : snapshot.LanguagesText;

            return new AiRepositoryAnalysisDto
            {
                Repository = snapshot.Repository,
                Summary = $"Gemini is temporarily unavailable, so SprintA created a fallback plan from GitHub metadata. Stack: {languageHint}. Description: {snapshot.Description}",
                QuickWins = new List<AiRepositoryBacklogItemDto>
                {
                    new()
                    {
                        Title = "Review README and setup instructions",
                        Category = "quick-win",
                        SuggestedHours = 2,
                        Priority = 3,
                        Reasoning = "Use the README to confirm local setup, scripts, environment variables, and first-run blockers."
                    },
                    new()
                    {
                        Title = hasIssues ? "Convert open GitHub issues into SprintA work items" : "Create baseline backlog from repository structure",
                        Category = "quick-win",
                        SuggestedHours = 3,
                        Priority = 2,
                        Reasoning = hasIssues
                            ? $"Open issue signals: {snapshot.OpenIssuesText}"
                            : "No readable open issues were found, so start with setup, module review, and smoke-test tasks."
                    },
                    new()
                    {
                        Title = "Document main modules and owner candidates",
                        Category = "quick-win",
                        SuggestedHours = 4,
                        Priority = 3,
                        Reasoning = "A quick module map helps PM/PO split work without waiting for Gemini."
                    }
                },
                MediumTasks = new List<AiRepositoryBacklogItemDto>
                {
                    new()
                    {
                        Title = "Map core user flows and API/UI touchpoints",
                        Category = "medium",
                        SuggestedHours = 8,
                        Priority = 2,
                        Reasoning = "Repository planning needs clear flows before accurate estimates and task breakdown."
                    },
                    new()
                    {
                        Title = "Create implementation backlog by feature area",
                        Category = "medium",
                        SuggestedHours = 10,
                        Priority = 3,
                        Reasoning = $"Group work by stack and modules from: {languageHint}."
                    },
                    new()
                    {
                        Title = "Add automated smoke checks for critical paths",
                        Category = "medium",
                        SuggestedHours = 8,
                        Priority = 3,
                        Reasoning = "Smoke checks reduce regression risk before AI-generated tasks are approved."
                    }
                },
                RiskyTasks = new List<AiRepositoryBacklogItemDto>
                {
                    new()
                    {
                        Title = "Assess integration, secret, and deployment risks",
                        Category = "risky",
                        SuggestedHours = 12,
                        Priority = 2,
                        Reasoning = "GitHub-based planning often misses environment, token, CORS, and deployment constraints."
                    },
                    new()
                    {
                        Title = "Review large refactor areas before auto-creating tasks",
                        Category = "risky",
                        SuggestedHours = 16,
                        Priority = 2,
                        Reasoning = "Risky work should be confirmed by PM/PO before creating or assigning many work items."
                    }
                },
                TestPlan = new List<string>
                {
                    "Run the project locally and verify build/start scripts.",
                    "Smoke test authentication, project navigation, work item CRUD, and integrations.",
                    "Compare open GitHub issues with generated SprintA backlog.",
                    "Validate estimate, assignee, reward, and velocity data after creating tasks.",
                    "Ask PM/PO to approve risky tasks before assigning them."
                },
                SuggestedPrompt = $"Phan tich repo {snapshot.Repository} thanh backlog thuc chien theo 4 nhom: quick wins, medium tasks, risky tasks, test plan. Hay uu tien module chinh, estimate gio, rui ro, nguoi phu hop va tieu chi nghiem thu."
            };
        }

        private async Task<GitHubRepositorySnapshot> FetchGitHubSnapshotAsync(string owner, string repository, string? token)
        {
            var repoRequest = CreateGitHubRequest($"https://api.github.com/repos/{owner}/{repository}", token);
            var issuesRequest = CreateGitHubRequest($"https://api.github.com/repos/{owner}/{repository}/issues?state=open&per_page=5", token);
            var readmeRequest = CreateGitHubRequest($"https://api.github.com/repos/{owner}/{repository}/readme", token);
            var languagesRequest = CreateGitHubRequest($"https://api.github.com/repos/{owner}/{repository}/languages", token);

            using var repoResponse = await _httpClient.SendAsync(repoRequest);
            if (!repoResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await BuildGitHubRepoErrorAsync(repoResponse, owner, repository, token));
            }

            using var issuesResponse = await _httpClient.SendAsync(issuesRequest);
            using var readmeResponse = await _httpClient.SendAsync(readmeRequest);
            using var languagesResponse = await _httpClient.SendAsync(languagesRequest);

            var repoJson = JsonDocument.Parse(await repoResponse.Content.ReadAsStringAsync()).RootElement;
            var issuesText = issuesResponse.IsSuccessStatusCode
                ? await BuildOpenIssuesTextAsync(issuesResponse)
                : "Khong doc duoc issue mo.";
            var languagesText = languagesResponse.IsSuccessStatusCode
                ? await BuildLanguagesTextAsync(languagesResponse)
                : "Khong ro ngon ngu chinh.";
            var readmeSnippet = readmeResponse.IsSuccessStatusCode
                ? await BuildReadmeSnippetAsync(readmeResponse)
                : "Khong doc duoc README.";

            var description = repoJson.TryGetProperty("description", out var descriptionElement) && descriptionElement.ValueKind != JsonValueKind.Null
                ? descriptionElement.GetString() ?? "Khong co mo ta."
                : "Khong co mo ta.";

            return new GitHubRepositorySnapshot(
                $"{owner}/{repository}",
                description,
                languagesText,
                issuesText,
                readmeSnippet);
        }

        private static async Task<string> BuildGitHubRepoErrorAsync(HttpResponseMessage response, string owner, string repository, string? token)
        {
            var statusCode = (int)response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync();

            if (statusCode == 404)
            {
                return $"Khong tim thay repo {owner}/{repository}. Hay kiem tra lai owner/repository hoac quyen truy cap.";
            }

            if (statusCode == 401)
            {
                return "GitHub token khong hop le hoac da het han. Hay tao token moi va luu lai trong Project Settings > Integrations.";
            }

            if (statusCode == 403)
            {
                var lowered = responseBody.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(token))
                {
                    return $"GitHub API dang tu choi repo {owner}/{repository} (403). Co the repo private hoac ban da cham gioi han request cho repo public. Hay them GitHub token vao Project Settings > Integrations.";
                }

                if (lowered.Contains("rate limit"))
                {
                    return "GitHub token da cham gioi han request. Hay doi mot luc hoac dung token khac.";
                }

                return "GitHub token khong du quyen de doc repo nay. Hay tao Fine-grained Personal Access Token co quyen read-only cho Contents va Metadata.";
            }

            return $"Khong doc duoc repo tu GitHub API ({statusCode}).";
        }

        private HttpRequestMessage CreateGitHubRequest(string url, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.UserAgent.ParseAdd("SprintA-AI/1.0");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Trim());
            }

            return request;
        }

        private static async Task<string> BuildOpenIssuesTextAsync(HttpResponseMessage response)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var titles = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray()
                    .Where(item => item.TryGetProperty("title", out _))
                    .Select(item => item.GetProperty("title").GetString())
                    .Where(title => !string.IsNullOrWhiteSpace(title))
                    .Take(5)
                    .ToList()
                : new List<string?>();

            return titles.Count > 0
                ? string.Join(" | ", titles!)
                : "Khong co issue mo.";
        }

        private static async Task<string> BuildLanguagesTextAsync(HttpResponseMessage response)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return "Khong ro ngon ngu chinh.";
            }

            var languages = document.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Take(6)
                .ToList();

            return languages.Count > 0 ? string.Join(", ", languages) : "Khong ro ngon ngu chinh.";
        }

        private static async Task<string> BuildReadmeSnippetAsync(HttpResponseMessage response)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!document.RootElement.TryGetProperty("content", out var contentElement))
            {
                return "Khong doc duoc README.";
            }

            var encoded = contentElement.GetString();
            if (string.IsNullOrWhiteSpace(encoded))
            {
                return "Khong doc duoc README.";
            }

            var normalized = Regex.Replace(encoded, "\\s+", string.Empty);
            var bytes = Convert.FromBase64String(normalized);
            var text = Encoding.UTF8.GetString(bytes);
            return Regex.Replace(text, "\\s+", " ").Trim()[..Math.Min(1200, Regex.Replace(text, "\\s+", " ").Trim().Length)];
        }

        private static GitHubRepositoryReference? ParseRepository(string repoUrl)
        {
            var match = Regex.Match(repoUrl, "github\\.com/([^/]+)/([^/#?]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            return new GitHubRepositoryReference(
                match.Groups[1].Value,
                match.Groups[2].Value.Replace(".git", "", StringComparison.OrdinalIgnoreCase));
        }

        private static double NormalizeStoryPoint(double storyPoints)
        {
            var allowed = new[] { 1d, 2d, 3d, 5d, 8d, 13d };
            return allowed
                .OrderBy(value => Math.Abs(value - Math.Max(1, storyPoints)))
                .First();
        }

        private static string NormalizeComplexity(string? complexity)
        {
            var value = (complexity ?? string.Empty).Trim();
            if (string.Equals(value, "Low", StringComparison.OrdinalIgnoreCase)) return "Low";
            if (string.Equals(value, "High", StringComparison.OrdinalIgnoreCase)) return "High";
            if (string.Equals(value, "Critical", StringComparison.OrdinalIgnoreCase)) return "Critical";
            return "Medium";
        }

        private long GetMonthlyQuota()
        {
            var value = _configuration["Gemini:MonthlyTokenQuota"];
            return long.TryParse(value, out var quota) && quota > 0 ? quota : 100_000;
        }

        private sealed record GeminiResult(string Text, long TotalTokens);
        private sealed record GitHubRepositoryReference(string Owner, string Repository);
        private sealed record GitHubRepositorySnapshot(
            string Repository,
            string Description,
            string LanguagesText,
            string OpenIssuesText,
            string ReadmeSnippet);
    }
}
