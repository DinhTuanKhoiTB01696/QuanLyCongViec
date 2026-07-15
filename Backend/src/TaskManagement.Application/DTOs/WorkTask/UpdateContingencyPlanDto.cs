using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class UpdateContingencyPlanDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Risk level is required")]
        public string RiskLevel { get; set; } = "Low";

        [MaxLength(500)]
        public string? RiskDescription { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
