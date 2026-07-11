using System;

namespace TaskManagement.Domain.Entities
{
    public class CustomFieldValue
    {
        public Guid Id { get; set; }
        public Guid WorkTaskId { get; set; }
        public WorkTask WorkTask { get; set; } = null!;
        public Guid FieldDefinitionId { get; set; }
        public CustomFieldDefinition FieldDefinition { get; set; } = null!;
        public string? Value { get; set; } // Lưu dưới dạng string serialize
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
