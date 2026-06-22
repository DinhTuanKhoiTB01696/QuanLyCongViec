using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class ProjectUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
        public int? PreviousProgress { get; set; }
        public int? NewProgress { get; set; }
        public DateTime? TargetDate { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProjectUpdateReaction> Reactions { get; set; } = new List<ProjectUpdateReaction>();
        public ICollection<ProjectUpdateAttachment> Attachments { get; set; } = new List<ProjectUpdateAttachment>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
