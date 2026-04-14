using System;

namespace TaskManagement.Domain.Entities
{
    /// <summary>
    /// File/image attached to a comment (Facebook-style comment attachments)
    /// </summary>
    public class CommentAttachment
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
        public Guid UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
