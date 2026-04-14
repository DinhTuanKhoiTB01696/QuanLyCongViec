<template>
  <div class="auth-page" data-theme="light">
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
        <h1 class="auth-title">{{ requires2FA ? 'Xác minh 2 bước' : 'Chào mừng trở lại' }}</h1>
        <p class="auth-subtitle">{{ requires2FA ? 'Nhập mã 6 số (OTP) vừa được gửi đến email của bạn để tiếp tục.' : 'Đăng nhập để tiếp tục quản lý công việc của bạn với SprintA.' }}</p>
        
        <!-- 2FA OTP Form -->
        <el-form v-if="requires2FA" class="auth-form" @submit.prevent="handleLogin2FA" label-position="top">
           <el-form-item label="Mã bảo mật (OTP)">
             <el-input v-model="otpCode" placeholder="Nhập 6 số..." size="large" />
           </el-form-item>
           
           <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">Xác thực OTP</el-button>
           
           <div class="divider"></div>
           <p class="auth-footer-text">
             Chưa nhận được mã? <a href="#" @click.prevent="handleLogin">Gửi lại</a> hoặc <a href="#" @click.prevent="requires2FA = false">Quay lại</a>
           </p>
        </el-form>

        <el-form v-else class="auth-form" @submit.prevent="handleLogin" label-position="top">
          <el-form-item label="Email">
            <el-input v-model="form.email" placeholder="name@email.com" size="large" />
          </el-form-item>
          
          <el-form-item class="password-item">
            <template #label>
              <div class="password-label">
                <span>Mật khẩu</span>
                <a href="#" class="forgot-pwd">Quên mật khẩu?</a>
              </div>
            </template>
            <el-input 
              v-model="form.password" 
              type="password" 
              placeholder="••••••••" 
              size="large" 
              show-password 
            />
          </el-form-item>
          
          <div class="remember-action">
            <el-checkbox v-model="form.remember">Ghi nhớ đăng nhập</el-checkbox>
          </div>
          
          <el-button 
            type="primary" 
            native-type="submit" 
            class="auth-btn" 
            size="large"
            :loading="isLoading"
          >
            Đăng nhập
          </el-button>
        </el-form>

        <div v-if="!requires2FA">
          <div class="divider">
            <span>HOẶC TIẾP TỤC VỚI</span>
          </div>
          
          <div class="social-login">
            <!-- Sử dụng slot custom để thiết kế nút Google GIỐNG HỆT nút GitHub -->
            <GoogleLogin :callback="handleGoogleLogin" popup-type="TOKEN" class="social-btn-wrapper">
              <el-button native-type="button" class="social-btn google-btn">
                <img :src="googleIcon" alt="Google" class="social-icon" /> Google
              </el-button>
            </GoogleLogin>
            
            <el-button native-type="button" class="social-btn github-btn" @click="handleGitHubLogin">
              <img :src="githubIcon" alt="GitHub" class="social-icon" /> GitHub
            </el-button>
          </div>

          <!-- Dev Login Button -->
          <el-button 
            type="warning" 
            class="auth-btn" 
            size="large" 
            style="margin-bottom: 16px; background: linear-gradient(135deg, #f59e0b, #d97706); border: none;"
            :loading="isLoading"
            @click="handleDevLogin"
          >
            🚀 Dev Login (Bỏ qua đăng nhập)
          </el-button>
          
          <p class="auth-footer-text">
            Chưa có tài khoản? <router-link to="/register">Đăng ký</router-link>
          </p>
        </div>
      </div>
    </div>
    
    <div class="auth-bottom">
      <p>© 2024 SprintA Inc. Đã đăng ký bản quyền.</p>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import logoImg from '../assets/logo_QLCV.png'
import googleIcon from '../assets/Icongoogle.png'
import githubIcon from '../assets/Icongithub.png'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'


const form = reactive({
  email: '',
  password: '',
  remember: false
})

const router = useRouter()

const isLoading = ref(false)
const requires2FA = ref(false)
const otpCode = ref('')

const handleLogin = async () => {
  if (!form.email || !form.password) {
    ElMessage.warning('Vui lòng nhập đầy đủ email và mật khẩu')
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/login', {
      email: form.email,
      password: form.password
    })

    if (response.data.requires2FA) {
      requires2FA.value = true
      ElMessage.success('Tài khoản được bảo vệ. Vui lòng kiểm tra email để lấy mã OTP.')
      return
    }

    const { accessToken, fullName, email, systemRoles, id } = response.data.data
    
    // Store in localStorage
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    
    ElMessage.success('Đăng nhập thành công!')
    
    // Check if there's a redirect query param
    const redirect = router.currentRoute.value.query.redirect
    router.push(redirect || '/dashboard')
  } catch (error) {
    console.error('Login error:', error)
    let errorMsg = error.response?.data?.message || 'Email hoặc mật khẩu không chính xác'
    ElMessage.error(errorMsg)
  } finally {
    isLoading.value = false
  }
}

const handleLogin2FA = async () => {
  if (!otpCode.value) {
    ElMessage.warning('Vui lòng nhập mã OTP')
    return
  }
  
  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/login-2fa', {
      email: form.email,
      password: form.password,
      otpCode: otpCode.value
    })

    const { accessToken, fullName, email, systemRoles, id } = response.data.data
    
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    
    ElMessage.success('Đăng nhập thành công!')
    
    const redirect = router.currentRoute.value.query.redirect
    router.push(redirect || '/dashboard')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'OTP không hợp lệ')
  } finally {
    isLoading.value = false
  }
}

const handleGoogleLogin = async (response) => {
  console.log('Google Response:', response) // Debug log 
  isLoading.value = true
  try {
    const token = response.access_token || response.credential
    if (!token) {
      ElMessage.error('Không nhận được token từ Google. Vui lòng thử lại.')
      isLoading.value = false
      return
    }
    const res = await axiosClient.post('/auth/google-login', {
      Credential: token
    })
    
    const { accessToken, fullName, email, systemRoles, id } = res.data.data
    
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    
    ElMessage.success('Đăng nhập bằng Google thành công!')
    
    const redirect = router.currentRoute.value.query.redirect
    router.push(redirect || '/dashboard')
  } catch (error) {
    console.error('Google login error:', error)
    const errorMsg = error.response?.data?.message || 'Không thể đăng nhập bằng Google'
    ElMessage.error(errorMsg)
  } finally {
    isLoading.value = false
  }
}

const handleGitHubLogin = () => {
  const clientId = import.meta.env.VITE_GITHUB_CLIENT_ID || 'Ov23liYQdySKrDme697t'
  const redirectUri = window.location.origin + '/auth/github/callback'
  const githubAuthUrl = `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=user:email`
  
  window.location.href = githubAuthUrl
}

const handleDevLogin = async () => {
  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/dev-login')
    const { accessToken, fullName, email, systemRoles, id } = response.data.data
    
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    
    ElMessage.success('Dev Login thành công!')
    router.push('/dashboard')
  } catch (error) {
    console.error('Dev login error:', error)
    ElMessage.error(error.response?.data?.message || 'Dev login thất bại')
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
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
}

.auth-card {
  background: white;
  width: 100%;
  max-width: 440px;
  padding: 48px 40px;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.04), 0 1px 3px rgba(0,0,0,0.02);
}

.auth-title {
  font-size: 28px;
  font-weight: 700;
  color: #0f172a;
  text-align: center;
  margin-bottom: 12px;
}

.auth-subtitle {
  color: #64748b;
  text-align: center;
  font-size: 14px;
  margin-bottom: 40px;
}

.auth-form {
  width: 100%;
}

:deep(.el-form-item__label) {
  font-weight: 600;
  color: #334155;
  padding-bottom: 4px;
}

.password-label {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

:deep(.password-item .el-form-item__label) {
  width: 100%;
}

.forgot-pwd {
  color: var(--el-color-primary);
  text-decoration: none;
  font-weight: 500;
  font-size: 13px;
}

.remember-action {
  margin-bottom: 24px;
  margin-top: -10px;
}

/* Beautified button */
.auth-btn {
  width: 100%;
  font-weight: 600;
  border-radius: 12px;
  height: 48px;
  font-size: 15px;
  background: linear-gradient(135deg, #0f4c81, var(--el-color-primary));
  border: none;
  transition: transform 0.2s, box-shadow 0.2s;
}

.auth-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 97, 255, 0.3);
}

.demo-btn {
  width: 100%;
  margin-top: 12px;
  height: 48px;
  border-radius: 12px;
  font-weight: 600;
  border: 1px dashed #cbd5e1;
  color: #475569;
}

.demo-btn:hover {
  background-color: #f1f5f9;
  border-color: #94a3b8;
  color: #1e293b;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 32px 0;
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
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.social-login {
  display: flex;
  gap: 16px;
  margin-bottom: 32px;
}

:deep(.social-btn-wrapper) {
  flex: 1;
  display: flex;
}

:deep(.social-btn-wrapper > *) {
  width: 100%;
}

@media (max-width: 640px) {
  .logo-text {
    display: none;
  }
  .auth-card {
    padding: 32px 24px;
    border-radius: 12px;
  }
  .auth-title {
    font-size: 24px;
  }
  .social-login {
    flex-direction: column;
  }
}

.social-btn {
  flex: 1;
  height: 48px;
  border-radius: 10px;
  font-weight: 500;
  font-size: 15px;
  color: #334155;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.google-btn {
  background-color: #ffffff;
  border: 1px solid #60a5fa; /* Viền xanh dương nhạt như ảnh */
}

.google-btn:hover {
  background-color: #eff6ff;
  border-color: #3b82f6;
  color: #1e3a8a;
}

.github-btn {
  background-color: #ffffff;
  border: 1px solid #cbd5e1; /* Viền xám nhạt như ảnh */
}

.github-btn:hover {
  background-color: #f8fafc;
  border-color: #94a3b8;
  color: #0f172a;
}

.social-icon {
  width: 20px;
  height: 20px;
  object-fit: contain;
}

.auth-footer-text {
  text-align: center;
  font-size: 14px;
  color: #64748b;
}

.auth-footer-text a {
  color: var(--el-color-primary);
  text-decoration: none;
  font-weight: 600;
}

.auth-bottom {
  text-align: center;
  padding: 24px;
  color: #94a3b8;
  font-size: 12px;
}
</style>
