using System;

namespace TaskManagement.Domain.Entities
{
    public class ChannelMessage
    {
        public Guid Id { get; set; }
        public Guid ChannelId { get; set; }
        public Department? Channel { get; set; }
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string? AttachmentUrl { get; set; }
    }
}
