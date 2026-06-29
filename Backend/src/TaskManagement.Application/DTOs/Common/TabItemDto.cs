using System;

namespace TaskManagement.Application.DTOs.Common
{
    public class TabItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Guid CreatorId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string? CreatorEmail { get; set; }
        public string? CreatorAvatarUrl { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? PreviousStatus { get; set; }
        public int? OldProgress { get; set; }
        public int? NewProgress { get; set; }
        public int? Progress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
