namespace TaskManagement.Domain.Entities
{
    public class AiAttachmentChunk
    {
        public Guid Id { get; set; }
        public Guid AttachmentId { get; set; }
        public AiAttachment Attachment { get; set; } = null!;
        public int ChunkIndex { get; set; }
        public string Locator { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int TokenEstimate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
