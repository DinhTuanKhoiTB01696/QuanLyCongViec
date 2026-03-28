namespace TaskManagement.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email chứa mã OTP đến người dùng
        /// </summary>
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }
}
