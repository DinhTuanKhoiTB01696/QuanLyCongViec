using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public int RiskLevel { get; set; }
        public string? DependencyJson { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
