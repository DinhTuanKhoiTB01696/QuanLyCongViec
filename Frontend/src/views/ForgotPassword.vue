<template>
  <div class="auth-page">
    <header class="auth-navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </router-link>
        <div class="nav-actions">
          <router-link class="nav-link" to="/login">{{ t('auth.nav.login') }}</router-link>
          <router-link class="nav-primary" to="/register">{{ t('auth.nav.register') }}</router-link>
        </div>
      </div>
    </header>

    <main class="auth-container">
      <section class="auth-card">
        <h1 class="auth-title">{{ getTitle() }}</h1>
        <p class="auth-subtitle">{{ getSubtitle() }}</p>

        <!-- Bước 1: Nhập email -->
        <el-form v-if="step === 1" class="auth-form" @submit.prevent="handleSendOtp" label-position="top">
          <el-form-item :label="t('auth.forgotPassword.emailLabel')">
            <el-input v-model="form.email" :placeholder="t('auth.forgotPassword.emailPlaceholder')" size="large" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.forgotPassword.sendOtp') }}
          </el-button>
        </el-form>

        <!-- Bước 2: Nhập OTP -->
        <el-form v-if="step === 2" class="auth-form" @submit.prevent="handleVerifyOtp" label-position="top">
          <el-form-item :label="t('auth.forgotPassword.otpLabel')">
            <el-input v-model="form.otp" :placeholder="t('auth.forgotPassword.otpPlaceholder')" size="large" />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.forgotPassword.verifyOtpBtn') }}
          </el-button>

          <p class="auth-footer-text">
            {{ t('auth.otp.backPrompt') }} <button type="button" class="link-btn" @click="step = 1">{{ t('auth.otp.backLink') }}</button>
          </p>
        </el-form>

        <!-- Bước 3: Đổi mật khẩu -->
        <el-form v-if="step === 3" class="auth-form" @submit.prevent="handleResetPassword" label-position="top">
          <el-form-item :label="t('auth.forgotPassword.passwordLabel')">
            <el-input
              v-model="form.newPassword"
              type="password"
              :placeholder="t('auth.forgotPassword.passwordPlaceholder')"
              size="large"
              show-password
            />
          </el-form-item>

          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.forgotPassword.resetBtn') }}
          </el-button>
        </el-form>

        <div class="social-shell">
          <div class="divider"><span>{{ t('auth.login.or').toUpperCase() }}</span></div>
          <p class="auth-footer-text">
            <router-link to="/login" class="back-login">{{ t('auth.forgotPassword.backToLogin') }}</router-link>
          </p>
        </div>
      </section>
    </main>

    <div class="auth-bottom">
      <p>© 2026 SprintA. {{ t('auth.footer.rights') }}</p>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '../api/axiosClient'
import { useI18n } from '@/composables/useI18n'
import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const { t } = useI18n()

const step = ref(1)
const isLoading = ref(false)
const otpToken = ref('')

const form = reactive({
  email: '',
  otp: '',
  newPassword: ''
})

const getErrorMessage = (error, defaultMsg) => {
  if (error.response?.data?.errors) {
    const errObj = error.response.data.errors
    const firstKey = Object.keys(errObj)[0]
    if (firstKey && Array.isArray(errObj[firstKey]) && errObj[firstKey].length > 0) {
      return errObj[firstKey][0]
    }
  }
  return error.response?.data?.message || defaultMsg
}

const getTitle = () => {
  if (step.value === 1) return t('auth.forgotPassword.title')
  if (step.value === 2) return t('auth.forgotPassword.verifyOtpTitle')
  if (step.value === 3) return t('auth.forgotPassword.resetTitle')
  return ''
}

const getSubtitle = () => {
  if (step.value === 1) return t('auth.forgotPassword.subtitle')
  if (step.value === 2) return t('auth.forgotPassword.verifyOtpSubtitle')
  if (step.value === 3) return t('auth.forgotPassword.resetSubtitle')
  return ''
}

const handleSendOtp = async () => {
  if (!form.email) {
    ElMessage.warning(t('auth.forgotPassword.missingEmail'))
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/send-otp', {
      email: form.email,
      purpose: 'forgot-password'
    })
    ElMessage.success(response.data?.message || t('auth.forgotPassword.otpSentMessage'))
    step.value = 2
  } catch (error) {
    ElMessage.error(getErrorMessage(error, t('auth.register.messages.sendOtpFailed')))
  } finally {
    isLoading.value = false
  }
}

const handleVerifyOtp = async () => {
  if (!form.otp) {
    ElMessage.warning(t('auth.forgotPassword.missingOtp'))
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/verify-otp', {
      email: form.email,
      otpCode: form.otp
    })
    otpToken.value = response.data?.otpToken
    ElMessage.success(response.data?.message || t('auth.forgotPassword.otpVerifiedSuccess'))
    step.value = 3
  } catch (error) {
    ElMessage.error(getErrorMessage(error, t('auth.messages.otpInvalid')))
  } finally {
    isLoading.value = false
  }
}

const handleResetPassword = async () => {
  if (!form.newPassword) {
    ElMessage.warning(t('auth.forgotPassword.missingPassword'))
    return
  }

  isLoading.value = true
  try {
    const response = await axiosClient.post('/auth/reset-password', {
      email: form.email,
      newPassword: form.newPassword,
      confirmPassword: form.newPassword,
      otpToken: otpToken.value
    })
    ElMessage.success(response.data?.message || t('auth.forgotPassword.successMessage'))
    router.push('/login')
  } catch (error) {
    ElMessage.error(getErrorMessage(error, t('auth.forgotPassword.resetFailed')))
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
.nav-actions {
  display: flex;
  align-items: center;
}

.nav-content {
  justify-content: space-between;
  gap: 24px;
}

.nav-actions {
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
.link-btn:hover {
  text-decoration: underline;
}

.back-login {
  color: #2563eb;
  text-decoration: none;
  font-weight: 600;
}
.back-login:hover {
  text-decoration: underline;
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
  .nav-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .auth-card {
    padding: 24px;
  }
}
</style>
