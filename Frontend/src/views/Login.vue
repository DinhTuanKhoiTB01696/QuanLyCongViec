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
          <h1 class="auth-title">{{ requires2FA ? 'Xác minh OTP' : 'Đăng nhập' }}</h1>
          <p class="auth-subtitle">
            {{ requires2FA 
              ? 'Nhập mã 6 số được gửi đến email của bạn để tiếp tục an toàn.' 
              : 'Tiếp tục không gian làm việc của bạn trên SprintA.' }}
          </p>
        </div>

        <form v-if="requires2FA" class="auth-form" @submit.prevent="handleLogin2FA">
          <div class="form-group mb-5">
            <label class="form-label">MÃ OTP</label>
            <SprintaInput v-model="otpCode" placeholder="Nhập 6 số" size="large" required class="premium-input" />
          </div>

          <SprintaButton variant="primary" type="submit" class="auth-btn premium-btn" size="large" :loading="isLoading">
            Xác thực
          </SprintaButton>

          <div class="auth-links text-center mt-5">
            <button type="button" class="link-btn" @click="requires2FA = false">Quay lại đăng nhập</button>
          </div>
        </form>

        <div class="nav-actions">
          <router-link class="nav-link" to="/login">{{ t('auth.nav.login') }}</router-link>
          <router-link class="nav-primary" to="/register">{{ t('auth.nav.register') }}</router-link>
        </div>
      </div>
    </header>

    <main class="auth-container">
      <section class="auth-card">
        <h1 class="auth-title">{{ requires2FA ? t('auth.otp.title') : t('auth.login.title') }}</h1>
        <p class="auth-subtitle">
          {{ requires2FA
            ? t('auth.otp.subtitle')
            : t('auth.login.subtitle') }}
        </p>

        <el-form v-if="requires2FA" class="auth-form" @submit.prevent="handleLogin2FA" label-position="top">
          <el-form-item :label="t('auth.otp.codeLabel')">
            <el-input v-model="otpCode" :placeholder="t('auth.otp.codePlaceholder')" size="large" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.otp.verify') }}
          </el-button>

          <p class="auth-footer-text">
            {{ t('auth.otp.backPrompt') }} <button type="button" class="link-btn" @click="requires2FA = false">{{ t('auth.otp.backLink') }}</button>
          </p>
        </el-form>


        <form v-else class="auth-form" @submit.prevent="handleLogin">
          <div class="form-group mb-5">
            <label class="form-label">EMAIL</label>
            <SprintaInput v-model="form.email" type="email" placeholder="Nhập email của bạn" size="large" required class="premium-input" />
          </div>


          <div class="form-group mb-5">
            <div class="label-row">
              <label class="form-label">MẬT KHẨU</label>
              <a href="#" class="link-btn forgot-link">Quên mật khẩu?</a>
            </div>
            <SprintaInput

          <el-form-item :label="t('auth.login.passwordLabel')">
            <el-input

              v-model="form.password"
              type="password"
              placeholder="Nhập mật khẩu"
              size="large"
              required
              class="premium-input"
            />
          </div>


          <div class="remember-action mb-6">
            <label class="checkbox-container">
              <input type="checkbox" v-model="form.remember" />
              <span class="checkbox-text">Ghi nhớ phiên đăng nhập</span>
            </label>
          </div>

          <SprintaButton variant="primary" type="submit" class="auth-btn premium-btn" size="large" :loading="isLoading">
            Đăng nhập
          </SprintaButton>
        </form>

        <div v-if="!requires2FA" class="social-section">
          <div class="divider"><span>HOẶC TIẾP TỤC VỚI</span></div>

          <div class="remember-action">
            <el-checkbox v-model="form.remember">{{ t('auth.login.remember') }}</el-checkbox>
          </div>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.login.submit') }}
          </el-button>
        </el-form>

        <div v-if="!requires2FA" class="social-shell">
          <div class="divider"><span>{{ t('auth.login.orContinueWith') }}</span></div>


          <div class="social-grid">
            <GoogleLogin :callback="handleGoogleLogin" popup-type="TOKEN" class="social-btn-wrap">
              <button type="button" class="social-btn">
                <img :src="googleIcon" alt="Google" class="social-icon" />
                <span>Google</span>
              </button>
            </GoogleLogin>

            <div class="social-btn-wrap">
              <button type="button" class="social-btn" @click="handleGitHubLogin">
                <img :src="githubIcon" alt="GitHub" class="social-icon" />
                <span>GitHub</span>
              </button>
            </div>
          </div>


          <p class="auth-footer-text">
            {{ t('auth.login.noAccount') }} <router-link to="/register">{{ t('auth.nav.register') }}</router-link>
          </p>

        </div>
      </main>


      <div class="auth-footer">
        <p class="signup-prompt">
          Chưa có tài khoản? <router-link to="/register" class="link-btn signup-link">Đăng ký ngay</router-link>
        </p>
        <div class="footer-links">
          <span>© 2026 SprintA</span>
          <span class="dot">•</span>
          <a href="#">Bảo mật</a>
          <span class="dot">•</span>
          <a href="#">Hỗ trợ</a>
        </div>
      </div>

    <div class="auth-bottom">
      <p>© 2026 SprintA. {{ t('auth.footer.rights') }}</p>

    </div>
  </div>
</template>

<script setup>

import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '../api/axiosClient'
import { saveAuthSession } from '@/utils/authSession'
import { ElMessage } from 'element-plus'
import logoImg from '../assets/logo_QLCV.png'

  import { reactive, ref } from 'vue'
  import { useRouter } from 'vue-router'
  import axiosClient from '../api/axiosClient'
  import { saveAuthSession } from '@/utils/authSession'
  import { useI18n } from '@/composables/useI18n'
  import logoImg from '../assets/logo_QLCV.png'

import googleIcon from '../assets/Icongoogle.png'
import githubIcon from '../assets/Icongithub.png'
import SprintaInput from '@/components/ui/SprintaInput.vue'
import SprintaButton from '@/components/ui/SprintaButton.vue'
import { ArrowLeft } from 'lucide-vue-next'

const router = useRouter()
const { t } = useI18n()

const form = reactive({
  email: '',
  password: '',
  remember: false
})

const isLoading = ref(false)
const requires2FA = ref(false)
const otpCode = ref('')

const getSafeRedirect = () => {
  return '/site-selection'
}

const handleLogin = async () => {
  if (!form.email || !form.password) {

    ElMessage.warning('Vui lòng nhập đầy đủ email và mật khẩu')

    ElMessage.warning(t('auth.messages.missingCredentials'))

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

      ElMessage.success('Tài khoản yêu cầu OTP. Vui lòng kiểm tra email.')

      ElMessage.success(t('auth.messages.requires2fa'))

      return
    }

    saveAuthSession(response.data.data)

    ElMessage.success('Đăng nhập thành công')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Email hoặc mật khẩu không chính xác')

    ElMessage.success(t('auth.messages.loginSuccess'))
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('auth.messages.loginFailed'))

  } finally {
    isLoading.value = false
  }
}

const handleLogin2FA = async () => {
  if (!otpCode.value) {

    ElMessage.warning('Vui lòng nhập mã OTP')

    ElMessage.warning(t('auth.messages.missingOtp'))

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

    ElMessage.success('Đăng nhập thành công')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'OTP không hợp lệ')

    ElMessage.success(t('auth.messages.loginSuccess'))
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('auth.messages.otpInvalid'))

  } finally {
    isLoading.value = false
  }
}

const handleGoogleLogin = async (response) => {
  const token = response?.access_token || response?.credential
  if (!token) {

    ElMessage.error('Không nhận được token từ Google')

    ElMessage.error(t('auth.messages.googleNoToken'))

    return
  }

  isLoading.value = true
  try {
    const res = await axiosClient.post('/auth/google-login', {
      Credential: token
    })

    saveAuthSession(res.data.data)

    ElMessage.success('Đăng nhập bằng Google thành công')
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Không thể đăng nhập bằng Google')

    ElMessage.success(t('auth.messages.googleSuccess'))
    router.push(getSafeRedirect())
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('auth.messages.googleFailed'))

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
  background: rgba(0, 82, 204, 0.12); /* Jira Blue Glow */
  top: -150px;
  left: -150px;
}

.bg-shape-2 {
  width: 500px;
  height: 500px;
  background: rgba(101, 84, 192, 0.1); /* Soft Purple Glow */
  bottom: -100px;
  right: -100px;
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

.label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
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

.checkbox-container {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  margin-top: 4px;
}

.checkbox-container input {
  width: 16px;
  height: 16px;
  accent-color: #0052CC;
  cursor: pointer;
  border-radius: 4px;
}

.checkbox-text {
  font-size: 14px;
  color: #172B4D;
  font-weight: 500;
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

.divider {
  position: relative;
  text-align: center;
  margin: 32px 0 24px;
}

.divider::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  width: 100%;
  height: 1px;
  background-color: #DFE1E6;
  z-index: 0;
}

.divider span {
  position: relative;
  z-index: 1;
  background: #ffffff;
  padding: 0 16px;
  font-size: 11px;
  font-weight: 700;
  color: #8993A4;
  letter-spacing: 0.06em;
}

.social-grid {
  display: flex;
  gap: 16px;
}

.social-btn-wrap {
  flex: 1;
}

.social-btn {
  width: 100%;
  height: 46px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: #ffffff;
  border: 2px solid #DFE1E6;
  border-radius: 6px;
  font-size: 15px;
  font-weight: 600;
  color: #172B4D;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}

.social-btn:hover {
  background: #FAFBFC;
  border-color: #C1C7D0;
}

.social-icon {
  width: 20px;
  height: 20px;
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

.forgot-link {
  font-size: 13px;
  font-weight: 600;
}

.text-center {
  text-align: center;
}

.auth-footer {
  margin-top: 36px;
  text-align: center;
}

.signup-prompt {
  font-size: 15px;
  color: #5E6C84;
  margin: 0 0 20px 0;
}

.signup-link {
  font-size: 15px;
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
.mb-6 { margin-bottom: 24px; }
.mt-4 { margin-top: 16px; }
.mt-5 { margin-top: 20px; }

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
  
  .social-grid {
    flex-direction: column;
  }
}
</style>
