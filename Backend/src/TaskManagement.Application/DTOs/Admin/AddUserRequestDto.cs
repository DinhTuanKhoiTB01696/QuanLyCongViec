using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Admin
{
    public class AddUserRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is invalid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = "Developer";

        public Guid? ProjectId { get; set; }

        public string ProjectRole { get; set; } = "DEV";

        public string? InviteMessage { get; set; }

        public string[] InviteGroups { get; set; } = Array.Empty<string>();
    }
}
