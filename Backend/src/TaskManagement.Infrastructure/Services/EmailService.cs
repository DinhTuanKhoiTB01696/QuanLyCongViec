using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace TaskManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gửi email chứa mã OTP qua Resend API
        /// API docs: https://resend.com/docs/api-reference/emails/send-email
        /// 
        /// ⚠️ HIỆN TẠI ĐANG COMMENT LẠI PHẦN GỬI EMAIL THẬT
        /// Khi đã verify domain trên Resend, bỏ comment để kích hoạt.
        /// Trong lúc chưa verify, OTP sẽ được in ra Console để test.
        /// </summary>
        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var apiKey = _configuration["Resend:ApiKey"]
                ?? throw new InvalidOperationException("Resend API Key chưa được cấu hình trong appsettings.json");
            var fromEmail = _configuration["Resend:FromEmail"]
                ?? "noreply@sprinta.io.vn";

            // ========== CHẾ ĐỘ DEV: In OTP ra Console ==========
            Console.WriteLine("╔══════════════════════════════════════════╗");
            Console.WriteLine($"║  📧 GỬI OTP ĐẾN: {toEmail}");
            Console.WriteLine($"║  🔑 MÃ OTP: {otpCode}");
            Console.WriteLine("╚══════════════════════════════════════════╝");

            var requestBody = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = "Mã xác thực OTP - SprintA",
                html = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 32px;'>
                        <h2 style='color: #0f172a; text-align: center;'>🔐 Mã xác thực OTP</h2>
                        <p style='color: #64748b; text-align: center;'>Sử dụng mã bên dưới để hoàn tất đăng ký tài khoản SprintA:</p>
                        <div style='background: #f1f5f9; border-radius: 12px; padding: 24px; text-align: center; margin: 24px 0;'>
                            <span style='font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #0ea5e9;'>{otpCode}</span>
                        </div>
                        <p style='color: #94a3b8; font-size: 13px; text-align: center;'>Mã này có hiệu lực trong 5 phút. Không chia sẻ mã này với bất kỳ ai.</p>
                    </div>"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Resend API Error: {errorContent}");
                throw new InvalidOperationException($"Không thể gửi email OTP. Resend trả về lỗi: {errorContent}");
            }

            Console.WriteLine($"✅ Email OTP đã gửi thành công đến {toEmail}");
        }
    }
}
