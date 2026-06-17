using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Department
{
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên phòng ban không quá 200 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public Guid? ManagerId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public string? CoverImage { get; set; }
    }
}
