namespace TaskManagement.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);

        Task SendInviteEmailAsync(
            string toEmail,
            string inviteeName,
            string inviterName,
            string organizationName,
            string? projectName,
            string acceptUrl,
            string? personalMessage);
    }
}
