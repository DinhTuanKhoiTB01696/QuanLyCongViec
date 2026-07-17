namespace TaskManagement.Domain.Entities
{
    public class AiAttachment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid ConversationId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Kind { get; set; } = "document";
        public string Sha256 { get; set; } = string.Empty;
        public string Status { get; set; } = "Processing";
        public string? ErrorMessage { get; set; }
        public long FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<AiAttachmentChunk> Chunks { get; set; } = new List<AiAttachmentChunk>();
    }
}
