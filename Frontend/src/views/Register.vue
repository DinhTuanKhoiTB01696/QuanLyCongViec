<template>
  <div class="auth-page">
    <header class="auth-navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </router-link>
        <div class="nav-actions" style="display: flex; align-items: center; gap: 16px;">
          <el-button link @click="$router.push('/login')">Đăng nhập</el-button>
          <el-button type="primary" @click="$router.push('/register')">Đăng ký</el-button>
        </div>
      </div>
    </header>

    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">
          {{ step === 1 ? 'Đăng Ký Tài Khoản' : step === 2 ? 'Xác thực Email' : 'Hoàn Tất Đăng Ký' }}
        </h1>
        <p class="auth-subtitle">
          {{ 
            step === 1 ? 'Bắt đầu quản lý dự án và công việc của bạn với SprintA.' : 
            step === 2 ? 'Nhập mã xác thực đã được gửi đến email của bạn.' :
            'Tạo mật khẩu và tên hiển thị cho tài khoản của bạn.'
          }}
        </p>

        <!-- Step 1: Email Input -->
        <el-form 
          v-if="step === 1" 
          ref="emailFormRef"
          :model="form"
          :rules="rules"
          class="auth-form" 
          label-position="top"
          @submit.prevent="handleSendOtp"
        >
          <el-form-item label="Địa chỉ Email" prop="email">
            <el-input v-model="form.email" placeholder="name@company.com" size="large" />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Gửi mã OTP
          </el-button>
          
          <p class="auth-footer-text">
            Đã có tài khoản? <router-link to="/login">Đăng nhập</router-link>
          </p>
        </el-form>

        <!-- Step 2: OTP Verification -->
        <el-form v-else-if="step === 2" class="auth-form" @submit.prevent="handleVerifyOtp" label-position="top">
          <div class="otp-instruction">
            Chúng tôi đã gửi mã xác thực tới <strong>{{ form.email }}</strong>. Vui lòng kiểm tra hộp thư của bạn.
          </div>
          <el-form-item label="Mã xác thực OTP">
            <el-input v-model="form.otp" placeholder="123456" size="large" maxlength="6" class="otp-input" />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Xác thực Mã
          </el-button>
          
          <p class="auth-footer-text">
            Không nhận được mã? <a href="#" @click.prevent="step = 1">Đổi email</a> hoặc <a href="#" @click.prevent="handleSendOtp">Gửi lại OTP</a>
          </p>
        </el-form>

        <!-- Step 3: Complete Profile -->
        <el-form 
          v-else 
          ref="profileFormRef"
          :model="form"
          :rules="rules"
          class="auth-form" 
          label-position="top"
          @submit.prevent="handleRegister"
        >
          <el-form-item label="Họ và Tên" prop="fullName">
            <el-input v-model="form.fullName" placeholder="Nhập họ và tên" size="large" />
          </el-form-item>

          <el-form-item label="Mật khẩu" prop="password">
            <el-input v-model="form.password" type="password" placeholder="Tạo mật khẩu" size="large" show-password />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Hoàn Tất Đăng Ký
          </el-button>
        </el-form>
      </div>
      
      <div class="auth-small-links">
        <a href="#">Điều khoản Dịch vụ</a> &nbsp;•&nbsp; <a href="#">Chính sách Bảo mật</a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'

import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const isLoading = ref(false)
const emailFormRef = ref(null)
const profileFormRef = ref(null)
const step = ref(1)
const verifiedOtpToken = ref('')

const form = reactive({
  email: '',
  otp: '',
  fullName: '',
  password: ''
})

const rules = reactive({
  email: [
    { required: true, message: 'Vui lòng nhập email', trigger: 'blur' },
    { type: 'email', message: 'Email không hợp lệ', trigger: ['blur', 'change'] }
  ],
  fullName: [
    { required: true, message: 'Vui lòng nhập họ và tên', trigger: 'blur' },
    { min: 2, message: 'Tên phải có ít nhất 2 ký tự', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Vui lòng tạo mật khẩu', trigger: 'blur' },
    { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự', trigger: 'blur' },
    { 
      pattern: /^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/, 
      message: 'Mật khẩu phải có ít nhất 1 chữ hoa, 1 số và 1 ký tự đặc biệt', 
      trigger: 'blur' 
    }
  ]
})

const handleSendOtp = async () => {
  if (step.value === 1 && !emailFormRef.value) return;
  
  const validatePromise = step.value === 1 
    ? emailFormRef.value.validate() 
    : Promise.resolve(true);

  try {
    const valid = await validatePromise;
    if (valid) {
      isLoading.value = true;
      try {
        const response = await axiosClient.post('/auth/send-otp', {
          email: form.email,
          purpose: 'register'
        });
        ElMessage.success(response.data.message || 'Đã gửi mã OTP đến email của bạn');
        form.otp = ''; // Reset OTP field
        step.value = 2;
      } catch (error) {
        console.error('Send OTP error:', error);
        ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra khi gửi OTP');
      } finally {
        isLoading.value = false;
      }
    }
  } catch (err) {
    // validation failed
  }
}

const handleVerifyOtp = async () => {
  if (!form.otp || form.otp.length !== 6) {
    ElMessage.error('Vui lòng nhập mã OTP 6 ký tự');
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
      verifiedOtpToken.value = response.data.otpToken; // Save token for register step
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
  if (!profileFormRef.value) return;
  
  await profileFormRef.value.validate(async (valid) => {
    if (valid) {
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
  });
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--color-bg);
}

.auth-navbar {
  height: 80px;
  display: flex;
  align-items: center;
  background: transparent;
}

.container {
  max-width: 1440px;
  width: 100%;
  margin: 0 auto;
  padding: 0 24px;
}

.nav-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Logo Styles */
.logo {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 800;
  font-size: 24px;
  color: var(--color-text-primary);
  text-decoration: none;
}

.custom-logo {
  height: 48px;
  width: auto;
  object-fit: contain;
}

.nav-actions {
  display: flex;
  gap: 16px;
}

.auth-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
}

.auth-card {
  background: var(--color-surface);
  width: 100%;
  max-width: 440px;
  padding: 48px 40px 40px;
  border-radius: 2px;
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-md);
}

.auth-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--color-text-primary);
  text-align: center;
  margin-bottom: 8px;
}

.auth-subtitle {
  color: var(--color-text-secondary);
  text-align: center;
  font-size: 13px;
  margin-bottom: 32px;
  line-height: 1.5;
}

.auth-form {
  width: 100%;
}

:deep(.el-form-item__label) {
  font-weight: 600;
  color: var(--color-text-secondary);
  padding-bottom: 4px;
  font-size: 13px;
}

.auth-btn {
  width: 100%;
  font-weight: 600;
  border-radius: 2px;
  height: 48px;
  font-size: 15px;
  background: var(--color-accent);
  border: none;
  margin-top: 8px;
  margin-bottom: 24px;
  transition: transform 0.2s, box-shadow 0.2s;
  color: #ffffff;
}

.auth-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 97, 255, 0.3);
}

.otp-instruction {
  text-align: center;
  font-size: 14px;
  color: var(--color-text-secondary);
  margin-bottom: 24px;
  background: var(--color-surface-hover);
  padding: 12px;
  border-radius: 2px;
  line-height: 1.6;
}

.otp-input :deep(input) {
  text-align: center;
  letter-spacing: 8px;
  font-size: 20px;
  font-weight: 700;
}

@media (max-width: 640px) {
  .logo-text {
    display: none;
  }
  .auth-navbar {
    height: 60px;
  }
  .auth-card {
    padding: 32px 20px;
    border-radius: 12px;
  }
  .auth-title {
    font-size: 20px;
  }
}



.auth-footer-text {
  text-align: center;
  font-size: 14px;
  color: var(--color-text-secondary);
  margin: 0;
}

.auth-footer-text a {
  color: var(--el-color-primary);
  text-decoration: none;
  font-weight: 600;
}

.auth-small-links {
  margin-top: 24px;
  font-size: 12px;
  color: var(--color-text-muted);
}

.auth-small-links a {
  color: var(--color-text-muted);
  text-decoration: none;
  transition: color 0.2s;
}

.auth-small-links a:hover {
  color: var(--color-text-primary);
}
</style>

