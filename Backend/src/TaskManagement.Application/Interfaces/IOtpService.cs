namespace TaskManagement.Application.Interfaces
{
    public interface IOtpService
    {
        /// <summary>
        /// Tạo mã OTP ngẫu nhiên 6 ký tự (chữ + số)
        /// </summary>
        string GenerateOtp();

        /// <summary>
        /// Lưu OTP vào cache với thời gian hết hạn 5 phút
        /// </summary>
        void StoreOtp(string email, string otp);

        /// <summary>
        /// Xác thực OTP - so sánh mã nhập vào với mã đã lưu
        /// </summary>
        bool ValidateOtp(string email, string otp);
    }
}
