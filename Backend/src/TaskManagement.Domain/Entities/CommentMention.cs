using System;

namespace TaskManagement.Domain.Entities
{
    public class CommentMention
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public Guid MentionedUserId { get; set; }
        public User MentionedUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
