<template>
  <div class="auth-page">
    <header class="auth-navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </router-link>
        <div class="nav-actions">
          <router-link class="nav-link" to="/login">Dang nhap</router-link>
          <router-link class="nav-primary" to="/register">Dang ky</router-link>
        </div>
      </div>
    </header>

    <main class="auth-container">
      <section class="auth-card">
        <h1 class="auth-title">{{ requires2FA ? 'Xac minh OTP' : 'Dang nhap vao SprintA' }}</h1>
        <p class="auth-subtitle">
          {{ requires2FA
            ? 'Nhap ma OTP da duoc gui toi email cua ban de tiep tuc.'
            : 'Ban can dang nhap de vao dashboard chinh va su dung cac chuc nang noi bo.' }}
        </p>

        <el-form v-if="requires2FA" class="auth-form" @submit.prevent="handleLogin2FA" label-position="top">
          <el-form-item label="Ma OTP">
            <el-input v-model="otpCode" placeholder="Nhap 6 so" size="large" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Xac thuc
          </el-button>

          <p class="auth-footer-text">
            Muon quay lai? <button type="button" class="link-btn" @click="requires2FA = false">Nhap lai tai khoan</button>
          </p>
        </el-form>

        <el-form v-else class="auth-form" @submit.prevent="handleLogin" label-position="top">
          <el-form-item label="Email">
            <el-input v-model="form.email" placeholder="name@email.com" size="large" />
          </el-form-item>

          <el-form-item label="Mat khau">
            <el-input
              v-model="form.password"
              type="password"
              placeholder="••••••••"
              size="large"
              show-password
            />
          </el-form-item>

          <div class="remember-action">
            <el-checkbox v-model="form.remember">Ghi nho dang nhap</el-checkbox>
          </div>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            Dang nhap
          </el-button>
        </el-form>

        <div v-if="!requires2FA" class="social-shell">
          <div class="divider"><span>HOAC TIEP TUC VOI</span></div>

          <div class="social-login">
            <GoogleLogin :callback="handleGoogleLogin" popup-type="TOKEN" class="social-btn-wrapper">
              <el-button native-type="button" class="social-btn google-btn">
                <img :src="googleIcon" alt="Google" class="social-icon" /> Google
              </el-button>
            </GoogleLogin>

            <el-button native-type="button" class="social-btn github-btn" @click="handleGitHubLogin">
              <img :src="githubIcon" alt="GitHub" class="social-icon" /> GitHub
            </el-button>
          </div>

          <p class="auth-footer-text">
            Chua co tai khoan? <router-link to="/register">Dang ky</router-link>
          </p>
        </div>
      </section>
    </main>

    <div class="auth-bottom">
      <p>© 2026 SprintA. Tat ca quyen duoc bao luu.</p>
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

const router = useRouter()

const form = reactive({
  email: '',
  password: '',
  remember: false
})

const isLoading = ref(false)
const requires2FA = ref(false)
const otpCode = ref('')

const getSafeRedirect = () => {
  const redirect = router.currentRoute.value.query.redirect
  return typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//')
    ? redirect
    : '/dashboard'
}

const saveAuthSession = (payload) => {
  const { accessToken, fullName, email, systemRoles, id } = payload
  localStorage.setItem('accessToken', accessToken)
  localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
}

const handleLogin = async () => {
  if (!form.email || !form.password) {
    ElMessage.warning('Vui long nhap day du email va mat khau')
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
      ElMessage.success('Tai khoan yeu cau OTP. Vui long kiem tra email.')
      return
    }

    saveAuthSession(response.data.data)
    ElMessage.success('Dang nhap thanh cong')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Email hoac mat khau khong chinh xac')
  } finally {
    isLoading.value = false
  }
}

const handleLogin2FA = async () => {
  if (!otpCode.value) {
    ElMessage.warning('Vui long nhap ma OTP')
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/login-2fa', {
      email: form.email,
      password: form.password,
      otpCode: otpCode.value
    })

    saveAuthSession(response.data.data)
    ElMessage.success('Dang nhap thanh cong')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'OTP khong hop le')
  } finally {
    isLoading.value = false
  }
}

const handleGoogleLogin = async (response) => {
  const token = response?.access_token || response?.credential
  if (!token) {
    ElMessage.error('Khong nhan duoc token tu Google')
    return
  }

  isLoading.value = true
  try {
    const res = await axiosClient.post('/auth/google-login', {
      Credential: token
    })

    saveAuthSession(res.data.data)
    ElMessage.success('Dang nhap bang Google thanh cong')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong the dang nhap bang Google')
  } finally {
    isLoading.value = false
  }
}

const handleGitHubLogin = () => {
  const clientId = import.meta.env.VITE_GITHUB_CLIENT_ID || 'Ov23liYQdySKrDme697t'
  const redirectUri = `${window.location.origin}/auth/github/callback`
  const githubAuthUrl = `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=user:email`
  window.location.href = githubAuthUrl
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background:
    radial-gradient(circle at top right, rgba(37, 99, 235, 0.08), transparent 26%),
    var(--color-bg);
}

.auth-navbar {
  min-height: 80px;
  display: flex;
  align-items: center;
}

.container {
  width: min(1180px, calc(100% - 32px));
  margin: 0 auto;
}

.nav-content,
.nav-actions,
.social-login {
  display: flex;
  align-items: center;
}

.nav-content {
  justify-content: space-between;
  gap: 24px;
}

.nav-actions,
.social-login {
  gap: 12px;
}

.logo {
  display: inline-flex;
  align-items: center;
  color: inherit;
  text-decoration: none;
}

.custom-logo {
  height: 56px;
}

.logo-text {
  margin-left: -8px;
  font-size: 24px;
  font-weight: 900;
  color: var(--color-text-primary);
}

.nav-link,
.nav-primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 2px;
  text-decoration: none;
  font-weight: 700;
  transition: all 0.2s;
}

.nav-link {
  padding: 10px 14px;
  color: var(--color-text-secondary);
}

.nav-primary {
  padding: 10px 18px;
  color: #ffffff;
  background: var(--color-accent);
}

.auth-container {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 16px 48px;
}

.auth-card {
  width: min(100%, 520px);
  padding: 32px;
  border-radius: 2px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-md);
}

.auth-title {
  margin: 0;
  font-size: 32px;
  color: var(--color-text-primary);
}

.auth-subtitle {
  margin: 10px 0 0;
  color: var(--color-text-secondary);
  line-height: 1.6;
}

.auth-form,
.social-shell {
  margin-top: 24px;
}

.remember-action {
  margin-bottom: 18px;
}

.auth-btn {
  width: 100%;
}

.divider {
  position: relative;
  margin: 22px 0 18px;
  text-align: center;
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.divider::before {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  top: 50%;
  height: 1px;
  background: var(--color-border);
  z-index: 0;
}

.divider span {
  position: relative;
  z-index: 1;
  padding: 0 10px;
  background: var(--color-surface);
}

.social-btn-wrapper,
.social-btn {
  flex: 1;
}

.social-btn {
  width: 100%;
}

.social-icon {
  width: 18px;
  height: 18px;
  margin-right: 8px;
}

.auth-footer-text {
  margin: 20px 0 0;
  color: var(--color-text-secondary);
  text-align: center;
}

.link-btn {
  border: none;
  background: transparent;
  color: #2563eb;
  cursor: pointer;
  font: inherit;
  padding: 0;
}

.auth-bottom {
  padding: 0 0 32px;
  text-align: center;
  color: var(--color-text-muted);
  font-size: 14px;
}

@media (max-width: 640px) {
  .container {
    width: min(100% - 24px, 1180px);
  }

  .nav-content,
  .nav-actions,
  .social-login {
    flex-direction: column;
    align-items: stretch;
  }

  .auth-card {
    padding: 24px;
  }
}
</style>


