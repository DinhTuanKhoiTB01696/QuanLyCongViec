using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid? WorkTaskId { get; set; }
        public WorkTask? WorkTask { get; set; }

        public Guid? GoalId { get; set; }
        public Goal? Goal { get; set; }

        public Guid? GoalUpdateId { get; set; }
        public GoalUpdate? GoalUpdate { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public ICollection<Comment> ChildComments { get; set; } = new List<Comment>();
        public ICollection<CommentAttachment> CommentAttachments { get; set; } = new List<CommentAttachment>();
    }
}
