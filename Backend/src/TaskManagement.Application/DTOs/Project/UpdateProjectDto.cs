using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Project
{
    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc.")]
        [StringLength(300, ErrorMessage = "Tên dự án không quá 300 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã dự án là bắt buộc.")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "Mã dự án phải có từ 2 đến 24 ký tự.")]
        [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Mã dự án chỉ gồm chữ, số, dấu gạch ngang hoặc gạch dưới.")]
        public string Identifier { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Why { get; set; }
        public string? SuccessCriteria { get; set; }
        public string? TrackedLinkUrl { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public Guid? DepartmentId { get; set; }
    }
}
