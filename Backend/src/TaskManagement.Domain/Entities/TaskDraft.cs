using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Domain.Entities
{
    public class TaskDraft
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; } // User who created the draft
        public Guid? ProjectId { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        // Can store all the other stuff in a JSON payload for flexibility
        public string? PayloadJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
