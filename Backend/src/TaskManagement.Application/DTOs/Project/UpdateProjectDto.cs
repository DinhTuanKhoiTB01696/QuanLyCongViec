using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Project
{
    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc.")]
        [StringLength(300, ErrorMessage = "Tên dự án không quá 300 ký tự.")]
        public string Name { get; set; } = string.Empty;

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
