using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Interfaces;
using System.Security.Cryptography;

namespace TaskManagement.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;

        // Bảng ký tự dùng để tạo OTP: chỉ dùng chữ số (0-9) để có mã OTP 6 số
        private const string OtpCharacters = "0123456789";
        private const int OtpLength = 6;
        private const int OtpExpirationMinutes = 5; // OTP hết hạn sau 5 phút

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Tạo mã OTP ngẫu nhiên 6 ký tự (chữ + số) sử dụng RandomNumberGenerator (an toàn hơn Random)
        /// </summary>
        public string GenerateOtp()
        {
            var otpChars = new char[OtpLength];
            var randomBytes = new byte[OtpLength];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < OtpLength; i++)
            {
                otpChars[i] = OtpCharacters[randomBytes[i] % OtpCharacters.Length];
            }

            return new string(otpChars);
        }

        /// <summary>
        /// Lưu OTP vào MemoryCache với key là email, hết hạn sau 5 phút
        /// </summary>
        public void StoreOtp(string email, string otp)
        {
            var cacheKey = $"OTP_{email.Trim().ToLowerInvariant()}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(OtpExpirationMinutes));

            _cache.Set(cacheKey, otp, cacheOptions);
        }

        /// <summary>
        /// Xác thực OTP: so sánh mã nhập vào với mã đã lưu trong cache
        /// Sau khi xác thực thành công, xóa OTP khỏi cache (chỉ dùng 1 lần)
        /// </summary>
        public bool ValidateOtp(string email, string otp)
        {
            var cacheKey = $"OTP_{email.Trim().ToLowerInvariant()}";

            if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                if (string.Equals(storedOtp?.Trim(), otp?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // OTP hợp lệ → xóa khỏi cache (dùng 1 lần)
                    _cache.Remove(cacheKey);
                    return true;
                }
            }

            return false;
        }
    }
}
