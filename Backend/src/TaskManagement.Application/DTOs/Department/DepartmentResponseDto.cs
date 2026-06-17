namespace TaskManagement.Application.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public Guid? ParentId { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
