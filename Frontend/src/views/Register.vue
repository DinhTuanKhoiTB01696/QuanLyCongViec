<template>
  <div class="auth-wrapper">
    <!-- Decorative background elements -->
    <div class="bg-shape bg-shape-1"></div>
    <div class="bg-shape bg-shape-2"></div>

    <!-- Back Button -->
    <router-link to="/" class="back-button">
      <ArrowLeft :size="20" />
      <span>Trang chủ</span>
    </router-link>

    <div class="auth-container">
      <!-- Minimal top brand area -->
      <div class="top-brand">
        <router-link to="/" class="brand-link">
          <img :src="logoImg" alt="SprintA Logo" class="brand-logo" />
          <span class="brand-text">SprintA</span>
        </router-link>
      </div>

      <main class="auth-card">
        <div class="auth-header">
          <h1 class="auth-title">
            {{ step === 1 ? 'Đăng ký tài khoản' : step === 2 ? 'Xác thực Email' : 'Hoàn tất đăng ký' }}
          </h1>
          <p class="auth-subtitle">
            {{ 
              step === 1 ? 'Bắt đầu quản lý dự án và công việc của bạn với SprintA.' : 
              step === 2 ? 'Nhập mã xác thực đã được gửi đến email của bạn.' :
              'Tạo mật khẩu và tên hiển thị cho tài khoản của bạn.'
            }}
          </p>
        </div>

        <!-- Step 1: Email Input -->
        <form 
          v-if="step === 1" 
          class="auth-form" 
          @submit.prevent="handleSendOtp"
        >
          <div class="form-group mb-5">
            <label class="form-label">ĐỊA CHỈ EMAIL</label>
            <SprintaInput v-model="form.email" type="email" placeholder="name@company.com" size="large" required class="premium-input" />
          </div>
          
          <SprintaButton variant="primary" type="submit" class="auth-btn premium-btn" size="large" :loading="isLoading">
            Gửi mã OTP
          </SprintaButton>
          
          <div class="auth-links text-center mt-5">
            <span class="prompt-text">Đã có tài khoản?</span> 
            <router-link to="/login" class="link-btn">Đăng nhập</router-link>
          </div>
        </form>

        <!-- Step 2: OTP Verification -->
        <form v-else-if="step === 2" class="auth-form" @submit.prevent="handleVerifyOtp">
          <div class="otp-instruction">
            Chúng tôi đã gửi mã xác thực tới <strong>{{ form.email }}</strong>. Vui lòng kiểm tra hộp thư của bạn.
          </div>
          <div class="form-group mb-5">
            <label class="form-label">MÃ XÁC THỰC OTP</label>
            <SprintaInput v-model="form.otp" placeholder="123456" size="large" maxlength="6" class="premium-input otp-input" required />
          </div>
          
          <SprintaButton variant="primary" type="submit" class="auth-btn premium-btn" size="large" :loading="isLoading">
            Xác thực Mã
          </SprintaButton>
          
          <div class="auth-links text-center mt-5">
            <span class="prompt-text">Không nhận được mã?</span> 
            <button type="button" class="link-btn" @click="step = 1">Đổi email</button> 
            <span class="prompt-text mx-1">hoặc</span> 
            <button type="button" class="link-btn" @click="handleSendOtp">Gửi lại</button>
          </div>
        </form>

        <!-- Step 3: Complete Profile -->
        <form 
          v-else 
          class="auth-form" 
          @submit.prevent="handleRegister"
        >
          <div class="form-group mb-5">
            <label class="form-label">HỌ VÀ TÊN</label>
            <SprintaInput v-model="form.fullName" placeholder="Nhập họ và tên" size="large" required minlength="2" class="premium-input" />
          </div>

          <div class="form-group mb-5">
            <label class="form-label">MẬT KHẨU</label>
            <SprintaInput v-model="form.password" type="password" placeholder="Tạo mật khẩu" size="large" required minlength="6" class="premium-input" />
          </div>
          
          <SprintaButton variant="primary" type="submit" class="auth-btn premium-btn" size="large" :loading="isLoading">
            Hoàn tất đăng ký
          </SprintaButton>
        </form>
      </main>

      <div class="auth-footer">
        <div class="footer-links">
          <span>© 2026 SprintA</span>
          <span class="dot">•</span>
          <a href="#">Điều khoản Dịch vụ</a>
          <span class="dot">•</span>
          <a href="#">Chính sách Bảo mật</a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'
import { ElMessage } from 'element-plus'
import { ArrowLeft } from 'lucide-vue-next'

import logoImg from '../assets/logo_QLCV.png'
import SprintaInput from '@/components/ui/SprintaInput.vue'
import SprintaButton from '@/components/ui/SprintaButton.vue'

const router = useRouter()
const isLoading = ref(false)
const step = ref(1)
const verifiedOtpToken = ref('')

const form = reactive({
  email: '',
  otp: '',
  fullName: '',
  password: ''
})

const handleSendOtp = async () => {
  if (step.value === 1 && !form.email) return;
  isLoading.value = true;
  try {
    const response = await axiosClient.post('/auth/send-otp', {
      email: form.email,
      purpose: 'register'
    });
    ElMessage.success(response.data.message || 'Đã gửi mã OTP đến email của bạn');
    form.otp = ''; 
    step.value = 2;
  } catch (error) {
    console.error('Send OTP error:', error);
    ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra khi gửi OTP');
  } finally {
    isLoading.value = false;
  }
}

const handleVerifyOtp = async () => {
  if (!form.otp || form.otp.length !== 6) {
    ElMessage.warning('Vui lòng nhập mã OTP 6 ký tự');
    return;
  }
  
  isLoading.value = true;
  try {
    const response = await axiosClient.post('/auth/verify-otp', { 
      email: form.email, 
      otpCode: form.otp 
    });
    
    if (response.data.verified) {
      ElMessage.success('Xác thực email thành công');
      verifiedOtpToken.value = response.data.otpToken;
      step.value = 3;
    }
  } catch (error) {
    console.error('Verify OTP error:', error);
    ElMessage.error(error.response?.data?.message || 'Mã OTP không hợp lệ');
  } finally {
    isLoading.value = false;
  }
}

const handleRegister = async () => {
  if (!form.fullName || !form.password) return;
  isLoading.value = true;
  try {
    const response = await axiosClient.post('/auth/register', { 
      email: form.email,
      fullName: form.fullName,
      password: form.password,
      otpCode: verifiedOtpToken.value
    });
    
    ElMessage.success('Tạo tài khoản thành công! Khởi tạo phiên làm việc bằng cách đăng nhập.');
    router.push('/login');
  } catch (error) {
    console.error('Register error:', error);
    ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra khi tạo tài khoản');
  } finally {
    isLoading.value = false;
  }
}
</script>

<style scoped>
.auth-wrapper {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #FAFBFC;
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  overflow: hidden;
}

.back-button {
  position: absolute;
  top: 32px;
  left: 32px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #5E6C84;
  text-decoration: none;
  font-weight: 600;
  font-size: 14px;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.2s ease;
  z-index: 10;
}

.back-button:hover {
  background: #ffffff;
  color: #172B4D;
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.05);
}

/* Premium Pastel Blobs */
.bg-shape {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  z-index: 0;
  opacity: 0.6;
}

.bg-shape-1 {
  width: 600px;
  height: 600px;
  background: rgba(0, 82, 204, 0.12);
  top: -150px;
  left: -150px;
}

.bg-shape-2 {
  width: 500px;
  height: 500px;
  background: rgba(101, 84, 192, 0.1);
  bottom: -100px;
  right: -100px;
}

.auth-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 460px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.top-brand {
  margin-bottom: 32px;
}

.brand-link {
  display: flex;
  align-items: center;
  gap: 10px;
  text-decoration: none;
}

.brand-logo {
  height: 36px;
  object-fit: contain;
}

.brand-text {
  font-size: 26px;
  font-weight: 800;
  color: #172B4D;
  letter-spacing: -0.02em;
}

.auth-card {
  width: 100%;
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid rgba(9, 30, 66, 0.08);
  box-shadow: 0 12px 32px rgba(9, 30, 66, 0.04), 0 0 2px rgba(9, 30, 66, 0.08);
  padding: 48px 40px;
}

.auth-header {
  text-align: center;
  margin-bottom: 36px;
}

.auth-title {
  font-size: 26px;
  font-weight: 700;
  color: #172B4D;
  margin: 0 0 10px 0;
  letter-spacing: -0.01em;
}

.auth-subtitle {
  font-size: 15px;
  color: #5E6C84;
  margin: 0;
  line-height: 1.5;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  font-size: 11px;
  font-weight: 700;
  color: #5E6C84;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0;
}

/* Premium Input Overrides */
:deep(.premium-input .el-input__wrapper) {
  height: 46px !important;
  border-radius: 6px !important;
  border: 2px solid #DFE1E6 !important;
  background-color: #FAFBFC !important;
  box-shadow: none !important;
  transition: all 0.2s ease;
  padding: 0 12px !important;
}

:deep(.premium-input .el-input__wrapper:hover) {
  background-color: #EBECF0 !important;
}

:deep(.premium-input.is-focus .el-input__wrapper),
:deep(.premium-input .el-input__wrapper.is-focus) {
  border-color: #0052CC !important;
  background-color: #ffffff !important;
}

:deep(.premium-input .el-input__inner) {
  font-size: 15px !important;
  color: #172B4D !important;
}

:deep(.premium-input .el-input__inner::placeholder) {
  color: #A5ADBA !important;
}

/* Primary Button */
.premium-btn {
  height: 48px !important;
  width: 100%;
  font-size: 16px !important;
  font-weight: 600 !important;
  border-radius: 6px !important;
  background-color: #0052CC !important;
  border: none !important;
  color: #ffffff !important;
  transition: background-color 0.2s, transform 0.1s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.premium-btn:hover {
  background-color: #0047B3 !important;
}

.premium-btn:active {
  transform: scale(0.98);
}

.link-btn {
  background: none;
  border: none;
  color: #0052CC;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  text-decoration: none;
  padding: 0;
}

.link-btn:hover {
  text-decoration: underline;
}

.text-center {
  text-align: center;
}

.prompt-text {
  color: #5E6C84; 
  font-size: 14px;
}
.mx-1 {
  margin: 0 4px;
}

.auth-footer {
  margin-top: 36px;
  text-align: center;
}

.footer-links {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  font-size: 13px;
  color: #8993A4;
}

.footer-links a {
  color: #8993A4;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: #172B4D;
}

.dot {
  font-size: 10px;
  opacity: 0.5;
}

.mb-5 { margin-bottom: 20px; }
.mt-5 { margin-top: 20px; }

/* Register specific styles */
.otp-instruction {
  text-align: center;
  font-size: 14px;
  color: #5E6C84;
  margin-bottom: 24px;
  background: #EBECF0;
  padding: 12px;
  border-radius: 6px;
  line-height: 1.6;
}

:deep(.otp-input .el-input__inner) {
  text-align: center !important;
  letter-spacing: 8px !important;
  font-size: 20px !important;
  font-weight: 700 !important;
}

@media (max-width: 480px) {
  .auth-card {
    padding: 40px 24px;
    box-shadow: 0 4px 12px rgba(9, 30, 66, 0.05);
    border: 1px solid #DFE1E6;
  }
  
  .auth-container {
    padding: 16px;
    max-width: 100%;
  }

  .auth-wrapper {
    background: #FAFBFC;
  }
  
  .bg-shape {
    opacity: 0.3;
    filter: blur(60px);
  }
  
  .back-button {
    top: 16px;
    left: 16px;
    padding: 6px 12px;
  }
}
</style>
