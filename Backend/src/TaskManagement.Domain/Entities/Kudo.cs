using System;

namespace TaskManagement.Domain.Entities
{
    public class Kudo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public Guid? ReceiverId { get; set; } // Nullable because it can be for a whole team
        public User? Receiver { get; set; }

        public Guid? DepartmentId { get; set; } // Target team
        public Department? Department { get; set; }

        public string Message { get; set; } = string.Empty;
        public string? Icon { get; set; } // Emoji or Gif URL

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
