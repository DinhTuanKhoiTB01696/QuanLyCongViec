using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Auth
{
    public class AcceptInviteTokenRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Password { get; set; }
    }
}
