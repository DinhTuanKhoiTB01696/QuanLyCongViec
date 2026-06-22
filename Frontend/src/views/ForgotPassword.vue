<template>
  <div class="auth-page">
    <header class="auth-navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </router-link>
        <div class="nav-actions" style="display: flex; align-items: center; gap: 16px;">
          <el-button link @click="$router.push('/login')">{{ t('Sign In', 'Đăng nhập') }}</el-button>
          <el-button type="primary" @click="$router.push('/register')">{{ t('Sign Up', 'Đăng ký') }}</el-button>
        </div>
      </div>
    </header>

    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">
          {{ step === 1 ? t('Reset Password', 'Đặt Lại Mật Khẩu') : step === 2 ? t('Verify Email', 'Xác thực Email') : t('New Password', 'Tạo Mật Khẩu Mới') }}
        </h1>
        <p class="auth-subtitle">
          {{
            step === 1 ? t('Enter your email address to receive a verification OTP code.', 'Nhập email của bạn để nhận mã OTP xác thực.') :
            step === 2 ? t('Enter the 6-digit OTP code sent to your email.', 'Nhập mã xác thực gồm 6 chữ số đã được gửi tới email của bạn.') :
            t('Create a new secure password for your account.', 'Tạo mật khẩu an toàn mới cho tài khoản của bạn.')
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
          <el-form-item :label="t('Email Address', 'Địa chỉ Email')" prop="email">
            <el-input v-model="form.email" placeholder="name@company.com" size="large" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('Send OTP Code', 'Gửi mã OTP') }}
          </el-button>

          <p class="auth-footer-text">
            {{ t('Remember password?', 'Đã nhớ mật khẩu?') }} <router-link to="/login">{{ t('Sign In', 'Đăng nhập') }}</router-link>
          </p>
        </el-form>

        <!-- Step 2: OTP Verification -->
        <el-form v-else-if="step === 2" class="auth-form" @submit.prevent="handleVerifyOtp" label-position="top">
          <div class="otp-instruction">
            {{ t('We have sent a verification code to', 'Chúng tôi đã gửi mã xác thực tới') }} <strong>{{ form.email }}</strong>. {{ t('Please check your inbox.', 'Vui lòng kiểm tra hộp thư của bạn.') }}
          </div>
          <el-form-item :label="t('Verification Code', 'Mã xác thực OTP')">
            <el-input v-model="form.otp" placeholder="123456" size="large" maxlength="6" class="otp-input" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('Verify OTP', 'Xác thực Mã') }}
          </el-button>

          <p class="auth-footer-text">
            {{ t('Did not receive code?', 'Không nhận được mã?') }}
            <a href="#" @click.prevent="step = 1">{{ t('Change email', 'Đổi email') }}</a>
            {{ t('or', 'hoặc') }}
            <a href="#" @click.prevent="handleSendOtp">{{ t('Resend OTP', 'Gửi lại OTP') }}</a>
          </p>
        </el-form>

        <!-- Step 3: Complete Reset Password -->
        <el-form
          v-else
          ref="passwordFormRef"
          :model="form"
          :rules="rules"
          class="auth-form"
          label-position="top"
          @submit.prevent="handleResetPassword"
        >
          <el-form-item :label="t('New Password', 'Mật khẩu mới')" prop="password">
            <el-input v-model="form.password" type="password" :placeholder="t('Enter new password', 'Nhập mật khẩu mới')" size="large" show-password />
          </el-form-item>

          <el-form-item :label="t('Confirm Password', 'Xác nhận mật khẩu')" prop="confirmPassword">
            <el-input v-model="form.confirmPassword" type="password" :placeholder="t('Re-enter new password', 'Nhập lại mật khẩu mới')" size="large" show-password />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('Reset Password', 'Đặt Lại Mật Khẩu') }}
          </el-button>
        </el-form>
      </div>

      <div class="auth-small-links">
        <a href="#">{{ t('Terms of Service', 'Điều khoản Dịch vụ') }}</a> &nbsp;•&nbsp; <a href="#">{{ t('Privacy Policy', 'Chính sách Bảo mật') }}</a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'
import { useLocale } from '@/composables/useLocale'
import { ElMessage } from 'element-plus'
import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const { t } = useLocale()

const isLoading = ref(false)
const emailFormRef = ref(null)
const passwordFormRef = ref(null)
const step = ref(1)
const verifiedOtpToken = ref('')

const form = reactive({
  email: '',
  otp: '',
  password: '',
  confirmPassword: ''
})

const validateConfirmPassword = (rule, value, callback) => {
  if (value === '') {
    callback(new Error(t('Please confirm your password', 'Vui lòng xác nhận mật khẩu')))
  } else if (value !== form.password) {
    callback(new Error(t('Passwords do not match', 'Mật khẩu xác nhận không khớp')))
  } else {
    callback()
  }
}

const rules = reactive({
  email: [
    { required: true, message: t('Please input email address', 'Vui lòng nhập email'), trigger: 'blur' },
    { type: 'email', message: t('Invalid email format', 'Email không đúng định dạng'), trigger: ['blur', 'change'] }
  ],
  password: [
    { required: true, message: t('Please create a password', 'Vui lòng tạo mật khẩu'), trigger: 'blur' },
    { min: 6, message: t('Password must be at least 6 characters', 'Mật khẩu phải có ít nhất 6 ký tự'), trigger: 'blur' },
    {
      pattern: /^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/,
      message: t('Must contain at least 1 uppercase letter, 1 number, and 1 special character', 'Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 số và 1 ký tự đặc biệt'),
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    { required: true, message: t('Please confirm your password', 'Vui lòng xác nhận mật khẩu'), trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' }
  ]
})

const handleSendOtp = async () => {
  if (step.value === 1 && !emailFormRef.value) return

  const validatePromise = step.value === 1
    ? emailFormRef.value.validate()
    : Promise.resolve(true)

  try {
    const valid = await validatePromise
    if (valid) {
      isLoading.value = true
      try {
        const response = await axiosClient.post('/auth/send-otp', {
          email: form.email,
          purpose: 'forgot-password'
        })
        ElMessage.success(response.data?.message || t('OTP code has been sent to your email', 'Đã gửi mã OTP đến email của bạn'))
        form.otp = ''
        step.value = 2
      } catch (error) {
        console.error('Send OTP error:', error)
        ElMessage.error(error.response?.data?.message || t('Failed to send OTP code', 'Có lỗi xảy ra khi gửi OTP'))
      } finally {
        isLoading.value = false
      }
    }
  } catch (err) {
    // validation failed
  }
}

const handleVerifyOtp = async () => {
  if (!form.otp || form.otp.length !== 6) {
    ElMessage.error(t('Please input 6-digit OTP code', 'Vui lòng nhập mã OTP 6 ký tự'))
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/verify-otp', {
      email: form.email,
      otpCode: form.otp
    })

    if (response.data?.verified) {
      ElMessage.success(t('Email verification succeeded', 'Xác thực email thành công'))
      verifiedOtpToken.value = response.data?.otpToken || ''
      step.value = 3
    } else {
      ElMessage.error(t('Invalid OTP code', 'Mã OTP không hợp lệ hoặc đã hết hạn'))
    }
  } catch (error) {
    console.error('Verify OTP error:', error)
    ElMessage.error(error.response?.data?.message || t('Invalid OTP code', 'Mã OTP không hợp lệ hoặc đã hết hạn'))
  } finally {
    isLoading.value = false
  }
}

const handleResetPassword = async () => {
  if (!passwordFormRef.value) return

  await passwordFormRef.value.validate(async (valid) => {
    if (valid) {
      isLoading.value = true
      try {
        // Gọi API reset password thật
        const response = await axiosClient.post('/auth/reset-password', {
          email: form.email,
          otpToken: verifiedOtpToken.value,
          newPassword: form.password
        })

        // Chỉ thông báo thành công khi API thành công
        ElMessage.success(response.data?.message || t('Password reset succeeded. Please sign in.', 'Đặt lại mật khẩu thành công! Vui lòng đăng nhập.'))
        router.push('/login')
      } catch (error) {
        console.error('Reset password error:', error)
        // Hiển thị lỗi thật từ API
        ElMessage.error(error.response?.data?.message || t('Failed to reset password. API endpoint might be missing.', 'Lỗi khi đặt lại mật khẩu. API endpoint có thể chưa được hỗ trợ.'))
      } finally {
        isLoading.value = false
      }
    }
  })
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--color-bg);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
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
