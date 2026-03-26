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
          <el-button type="primary" round @click="$router.push('/register')">Đăng ký</el-button>
        </div>
      </div>
    </header>

    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">{{ step === 1 ? 'Tạo Tài Khoản' : 'Xác thực Email' }}</h1>
        <p class="auth-subtitle">
          {{ step === 1 ? 'Bắt đầu quản lý dự án và công việc của bạn với SprintA.' : 'Nhập mã xác thực đã được gửi đến email của bạn.' }}
        </p>

        <!-- Step 1: Registration Information -->
        <el-form 
          v-if="step === 1" 
          ref="formRef"
          :model="form"
          :rules="rules"
          class="auth-form" 
          label-position="top"
          @submit.prevent="handleNextStep"
        >


          <el-form-item label="Địa chỉ Email" prop="email">
            <el-input v-model="form.email" placeholder="name@company.com" size="large" />
          </el-form-item>
          
          <el-button type="primary" class="auth-btn" size="large" :loading="isLoading" @click="handleNextStep">
            Gửi mã OTP
          </el-button>
          
          <p class="auth-footer-text">
            Đã có tài khoản? <router-link to="/login">Đăng nhập</router-link>
          </p>
        </el-form>

        <!-- Step 2: OTP Verification -->
        <el-form v-else class="auth-form" @submit.prevent="handleRegister" label-position="top">
          <div class="otp-instruction">
            Chúng tôi đã gửi mã xác thực tới <strong>{{ form.email }}</strong>. Vui lòng kiểm tra hộp thư của bạn.
          </div>
          <el-form-item label="Mã xác thực OTP">
            <el-input v-model="form.otp" placeholder="123456" size="large" maxlength="6" class="otp-input" />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Tạo Tài Khoản
          </el-button>
          
          <p class="auth-footer-text">
            Không nhận được mã? <a href="#" @click.prevent="step = 1">Quay lại</a> hoặc <a href="#">Gửi lại</a>
          </p>
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
import { ElMessage } from 'element-plus'
import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const isLoading = ref(false)
const formRef = ref(null)
const step = ref(1)

const form = reactive({
  email: '',
  otp: ''
})

const rules = reactive({
  email: [
    { required: true, message: 'Vui lòng nhập email', trigger: 'blur' },
    { type: 'email', message: 'Email không hợp lệ', trigger: ['blur', 'change'] }
  ]
})

const handleNextStep = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      isLoading.value = true;
      try {
        console.log('Sending OTP to:', form.email)
        ElMessage.success('Đã gửi mã OTP đến email của bạn');
        step.value = 2;
      } catch (error) {
        console.error('Register error:', error)
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
  background-color: #f8fafc;
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
  color: #1a1a1a;
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
  background: white;
  width: 100%;
  max-width: 440px;
  padding: 48px 40px 40px;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.04), 0 1px 3px rgba(0,0,0,0.02);
}

.auth-title {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
  text-align: center;
  margin-bottom: 8px;
}

.auth-subtitle {
  color: #64748b;
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
  color: #334155;
  padding-bottom: 4px;
  font-size: 13px;
}

.auth-btn {
  width: 100%;
  font-weight: 600;
  border-radius: 12px;
  height: 48px;
  font-size: 15px;
  background: linear-gradient(135deg, #0ea5e9, var(--el-color-primary));
  border: none;
  margin-top: 8px;
  margin-bottom: 24px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.auth-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 97, 255, 0.3);
}

.otp-instruction {
  text-align: center;
  font-size: 14px;
  color: #64748b;
  margin-bottom: 24px;
  background: #f1f5f9;
  padding: 12px;
  border-radius: 8px;
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
  color: #64748b;
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
  color: #94a3b8;
}

.auth-small-links a {
  color: #94a3b8;
  text-decoration: none;
  transition: color 0.2s;
}

.auth-small-links a:hover {
  color: #64748b;
}
</style>
