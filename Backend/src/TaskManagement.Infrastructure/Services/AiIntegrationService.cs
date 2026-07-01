using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class AiIntegrationService : IAiIntegrationService
    {
        private const string NotConfiguredMessage = "AI chưa được cấu hình";
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AiIntegrationService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<object> SummarizeInboxItemAsync(Guid inboxItemId, Guid userId)
            => BuildNotConfiguredResponseAsync(inboxItemId, userId, "summarize");

        public Task<object> SuggestTaskFromInboxItemAsync(Guid inboxItemId, Guid userId)
            => BuildNotConfiguredResponseAsync(inboxItemId, userId, "suggest-task");

        public Task<object> SuggestRelatedTaskAsync(Guid inboxItemId, Guid userId)
            => BuildNotConfiguredResponseAsync(inboxItemId, userId, "suggest-related-task");

        private async Task<object> BuildNotConfiguredResponseAsync(Guid inboxItemId, Guid userId, string action)
        {
            var exists = await _context.InboxItems
                .AsNoTracking()
                .AnyAsync(item => item.Id == inboxItemId && item.UserId == userId);

            if (!exists)
            {
                return new { configured = false, message = "Không tìm thấy mục inbox", action };
            }

            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            {
                return new { configured = false, message = NotConfiguredMessage, action };
            }

            return new { configured = false, message = NotConfiguredMessage, action };
        }
    }
}
