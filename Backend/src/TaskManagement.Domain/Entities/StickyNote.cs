using System;

namespace TaskManagement.Domain.Entities
{
    public class StickyNote
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? WorkspaceId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? WorkTaskId { get; set; }
        public Guid? GoalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Color { get; set; } = "#FEF08A";
        public bool IsPinned { get; set; }
        public bool IsFloating { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public string? SourceRoute { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
