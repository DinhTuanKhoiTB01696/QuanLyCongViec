using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class CustomFieldDefinition
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Text, Number, Date, Select, Checkbox
        public bool IsRequired { get; set; }
        public string? OptionsJson { get; set; } // ["Thấp", "Trung bình", "Cao"]
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}
