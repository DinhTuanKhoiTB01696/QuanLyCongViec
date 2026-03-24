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
        <h1 class="auth-title">Tạo Tài Khoản</h1>
        <p class="auth-subtitle">Bắt đầu quản lý dự án và công việc của bạn với SprintA.</p>
        
        <div class="social-login">
          <GoogleLogin :callback="handleGoogleLogin" class="social-btn-wrapper">
            <el-button plain class="social-btn">
              <img :src="googleIcon" alt="Google" class="social-icon" /> Google
            </el-button>
          </GoogleLogin>
          <el-button plain class="social-btn">
            <img :src="githubIcon" alt="GitHub" class="social-icon" /> GitHub
          </el-button>
        </div>
        
        <div class="divider">
          <span>hoặc</span>
        </div>
        
        <el-form ref="formRef" :model="form" :rules="rules" class="auth-form" @submit.prevent="handleRegister" label-position="top">
          <el-form-item label="Họ và Tên" prop="name">
            <el-input v-model="form.name" placeholder="John Doe" size="large" />
          </el-form-item>
          
          <el-form-item label="Địa chỉ Email" prop="email">
            <el-input v-model="form.email" placeholder="name@company.com" size="large" />
          </el-form-item>
          
          <el-form-item label="Mật khẩu" prop="password">
            <el-input v-model="form.password" type="password" placeholder="••••••••" size="large" show-password />
          </el-form-item>
          
          <el-form-item label="Xác nhận Mật khẩu" prop="confirmPassword">
            <el-input v-model="form.confirmPassword" type="password" placeholder="••••••••" size="large" show-password />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">Tạo Tài Khoản</el-button>
          
          <p class="auth-footer-text">
            Đã có tài khoản? <router-link to="/login">Đăng nhập</router-link>
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
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'
import logoImg from '../assets/logo_QLCV.png'
import googleIcon from '../assets/Icongoogle.png'
import githubIcon from '../assets/Icongithub.png'
import { ElMessage } from 'element-plus'

const router = useRouter()
const isLoading = ref(false)
const formRef = ref(null)

const form = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const validatePass = (rule, value, callback) => {
  if (value === '') {
    callback(new Error('Vui lòng nhập mật khẩu'))
  } else if (!/^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/.test(value)) {
    callback(new Error('Mật khẩu cần ít nhất 6 ký tự, 1 chữ hoa, 1 số, 1 ký tự đặc biệt'))
  } else {
    if (form.confirmPassword !== '') {
      if (!formRef.value) return
      formRef.value.validateField('confirmPassword', () => null)
    }
    callback()
  }
}

const validatePass2 = (rule, value, callback) => {
  if (value === '') {
    callback(new Error('Vui lòng nhập lại mật khẩu'))
  } else if (value !== form.password) {
    callback(new Error('Mật khẩu không khớp!'))
  } else {
    callback()
  }
}

const rules = reactive({
  name: [{ required: true, message: 'Vui lòng nhập họ tên', trigger: 'blur' }],
  email: [
    { required: true, message: 'Vui lòng nhập email', trigger: 'blur' },
    { type: 'email', message: 'Email không hợp lệ', trigger: ['blur', 'change'] }
  ],
  password: [{ validator: validatePass, trigger: 'blur' }],
  confirmPassword: [{ validator: validatePass2, trigger: 'blur' }]
})

const handleRegister = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      isLoading.value = true;
      try {
        const payload = {
          fullName: form.name,
          email: form.email,
          password: form.password
        }
        const response = await axiosClient.post('/auth/register', payload);
        ElMessage.success(response.data.message || 'Đăng ký thành công!');
        router.push('/login');
      } catch (error) {
        let errorMsg = error.response?.data?.message || 'Có lỗi xảy ra khi đăng ký.'
        const errors = error.response?.data?.errors
        if (errors) {
          const firstKey = Object.keys(errors)[0]
          errorMsg = errors[firstKey][0]
        }
        ElMessage.error(errorMsg)
        console.error('Register error:', error)
      } finally {
        isLoading.value = false;
      }
    }
  });
}

const handleGoogleLogin = async (response) => {
  isLoading.value = true
  try {
    const res = await axiosClient.post('/auth/google-login', {
      credential: response.credential
    })
    
    const { accessToken, fullName, email, systemRoles, id } = res.data.data
    
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    
    ElMessage.success('Đăng ký bằng Google thành công!')
    
    const redirect = router.currentRoute.value.query.redirect
    router.push(redirect || '/dashboard')
  } catch (error) {
    console.error('Google register error:', error)
    const errorMsg = error.response?.data?.message || 'Không thể xác thực với Google'
    ElMessage.error(errorMsg)
  } finally {
    isLoading.value = false
  }
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

.social-login {
  display: flex;
  gap: 16px;
  margin-bottom: 32px;
}

@media (max-width: 640px) {
  .social-login {
    flex-direction: column;
  }
}

:deep(.social-btn-wrapper) {
  flex: 1;
  display: flex;
}

:deep(.social-btn-wrapper > *) {
  width: 100%;
}

.social-btn {
  flex: 1;
  height: 44px;
  border-radius: 10px;
  font-weight: 500;
  color: #334155;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.social-icon {
  width: 20px;
  height: 20px;
  object-fit: contain;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 24px 0;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #e2e8f0;
}

.divider span {
  padding: 0 16px;
  color: #94a3b8;
  font-size: 12px;
  font-style: italic;
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
