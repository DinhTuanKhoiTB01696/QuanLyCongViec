using System;

namespace TaskManagement.Application.DTOs.Collaboration
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid UserUserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public Guid WorkTaskId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }

    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
