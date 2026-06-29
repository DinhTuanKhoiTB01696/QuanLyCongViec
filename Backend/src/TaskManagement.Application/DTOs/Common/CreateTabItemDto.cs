using System;

namespace TaskManagement.Application.DTOs.Common
{
    public class CreateTabItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Status { get; set; }
        public int? Progress { get; set; }
        // Optionally, severity for Risk
        public string? Severity { get; set; }
    }
}
