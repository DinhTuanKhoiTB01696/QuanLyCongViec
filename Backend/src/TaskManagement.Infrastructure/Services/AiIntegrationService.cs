using System;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class AiIntegrationService : IAiIntegrationService
    {
        private const string NotConfiguredMessage = "AI chưa được cấu hình";
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public AiIntegrationService(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<object> SummarizeInboxItemAsync(Guid inboxItemId, Guid userId)
        {
            var item = await GetInboxItemAsync(inboxItemId, userId);
            var notReady = ValidateAiConfiguration("summary");
            if (notReady != null) return notReady;

            var prompt = $"""
            You are SprintA AI. Summarize this integration inbox item in Vietnamese with full Vietnamese diacritics.
            Return 3 short bullet points: context, action needed, risk/deadline if any.
            Do not invent facts.

            Inbox item:
            {BuildInboxContext(item)}
            """;

            var summary = await GenerateTextAsync(userId, "integration-inbox-summary", prompt);
            return new { configured = true, action = "summary", summary };
        }

        public async Task<object> SuggestTaskFromInboxItemAsync(Guid inboxItemId, Guid userId)
        {
            var item = await GetInboxItemAsync(inboxItemId, userId);
            var notReady = ValidateAiConfiguration("suggest-task");
            if (notReady != null) return notReady;

            var prompt = $$"""
            You are SprintA AI. Convert this inbox signal into one actionable SprintA task.
            Use Vietnamese with full Vietnamese diacritics for title, description, and reason.
            Return STRICT JSON only with this schema:
            {
              "title": "short task title",
              "description": "clear task description in Vietnamese",
              "priority": 1,
              "reason": "why this task should be created"
            }
            Priority: 1 urgent, 2 high, 3 medium, 4 low.
            Do not invent facts.

            Inbox item:
            {{BuildInboxContext(item)}}
            """;

            var text = await GenerateTextAsync(userId, "integration-inbox-suggest-task", prompt, forceJson: true);
            return new { configured = true, action = "suggest-task", suggestedTask = DeserializeJsonObject(text) };
        }

        public async Task<object> SuggestRelatedTaskAsync(Guid inboxItemId, Guid userId)
        {
            var item = await GetInboxItemAsync(inboxItemId, userId);
            var notReady = ValidateAiConfiguration("suggest-related-task");
            if (notReady != null) return notReady;

            var candidates = await _context.WorkTasks
                .AsNoTracking()
                .Where(task => !task.IsDeleted
                    && _context.ProjectMembers.Any(member => member.ProjectId == task.ProjectId && member.UserId == userId && member.Status))
                .OrderByDescending(task => task.UpdatedAt)
                .Take(12)
                .Select(task => new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.Priority,
                    ProjectName = task.Project.Name,
                    StatusName = task.TaskStatus.Name
                })
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return new { configured = true, action = "suggest-related-task", message = "Chưa có task nào trong project của bạn để AI liên kết." };
            }

            var prompt = $$"""
            You are SprintA AI. Pick the most related existing task for this inbox item.
            Return STRICT JSON only:
            {
              "id": "task guid",
              "title": "existing task title",
              "reason": "short Vietnamese reason"
            }
            Use Vietnamese with full Vietnamese diacritics for reason.
            If none are related, return {"id": "", "title": "", "reason": "Không tìm thấy task phù hợp"}.

            Inbox item:
            {{BuildInboxContext(item)}}

            Existing tasks:
            {{JsonSerializer.Serialize(candidates, _jsonOptions)}}
            """;

            var text = await GenerateTextAsync(userId, "integration-inbox-related-task", prompt, forceJson: true);
            return new { configured = true, action = "suggest-related-task", relatedTask = DeserializeJsonObject(text) };
        }

        private async Task<InboxItem> GetInboxItemAsync(Guid inboxItemId, Guid userId)
        {
            var item = await _context.InboxItems
                .AsNoTracking()
                .FirstOrDefaultAsync(inboxItem => inboxItem.Id == inboxItemId && inboxItem.UserId == userId);

            return item ?? throw new InvalidOperationException("Không tìm thấy mục inbox");
        }

        private object? ValidateAiConfiguration(string action)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey)
                || apiKey.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
                || apiKey.Contains("PASTE_YOUR_GEMINI_API_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            {
                return new { configured = false, message = NotConfiguredMessage, action };
            }

            return null;
        }

        private async Task<string> GenerateTextAsync(Guid userId, string featureCode, string prompt, bool forceJson = false)
        {
            var apiKey = _configuration["Gemini:ApiKey"]!;
            var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
            var payload = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "Follow the requested output format exactly. Be concise and do not invent private data." } }
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
                    temperature = forceJson ? 0.2 : 0.4,
                    responseMimeType = forceJson ? "application/json" : "text/plain"
                }
            };

            using var response = await _httpClient.PostAsJsonAsync(endpoint, payload, _jsonOptions);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API loi {(int)response.StatusCode}: {responseBody}");
            }

            var (text, tokens) = ParseGeminiResponse(responseBody);
            _context.AITokenUsages.Add(new AITokenUsage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureCode = featureCode,
                TokensUsed = tokens > 0 ? tokens : Math.Max(1, (prompt.Length + text.Length) / 4),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return text.Trim();
        }

        private static (string Text, long TotalTokens) ParseGeminiResponse(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var text = "";
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];
                if (first.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts))
                {
                    text = string.Join("", parts.EnumerateArray()
                        .Where(part => part.TryGetProperty("text", out _))
                        .Select(part => part.GetProperty("text").GetString()));
                }
            }

            long totalTokens = 0;
            if (root.TryGetProperty("usageMetadata", out var usage)
                && usage.TryGetProperty("totalTokenCount", out var totalTokenCount))
            {
                totalTokens = totalTokenCount.GetInt64();
            }

            return (text, totalTokens);
        }

        private static string BuildInboxContext(InboxItem item)
            => string.Join(Environment.NewLine, new[]
            {
                $"Title: {item.Title}",
                $"Source: {item.Provider}/{item.Source}",
                item.StartsAt.HasValue ? $"Starts: {item.StartsAt.Value:O}" : null,
                item.EndsAt.HasValue ? $"Ends: {item.EndsAt.Value:O}" : null,
                !string.IsNullOrWhiteSpace(item.Location) ? $"Location: {item.Location}" : null,
                $"Content: {(string.IsNullOrWhiteSpace(item.Content) ? "(empty)" : item.Content)}"
            }.Where(line => line != null));

        private static object DeserializeJsonObject(string text)
        {
            using var doc = JsonDocument.Parse(text);
            return JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? new { };
        }
    }
}
