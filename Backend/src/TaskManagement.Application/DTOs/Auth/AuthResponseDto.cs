namespace TaskManagement.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Các quyền hệ thống (System Roles)
        public string[] SystemRoles { get; set; } = Array.Empty<string>();
    }
}
