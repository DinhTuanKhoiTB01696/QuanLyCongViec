<template>
  <div class="auth-page">
    <header class="auth-navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </router-link>
        <div class="nav-actions" style="display: flex; align-items: center; gap: 16px;">
          <el-button link @click="$router.push('/login')">{{ t('auth.nav.login') }}</el-button>
          <el-button type="primary" @click="$router.push('/register')">{{ t('auth.nav.register') }}</el-button>
        </div>
      </div>
    </header>

    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">
          {{ step === 1 ? t('auth.register.titleStep1') : step === 2 ? t('auth.register.titleStep2') : t('auth.register.titleStep3') }}
        </h1>
        <p class="auth-subtitle">
          {{ 
            step === 1 ? t('auth.register.subtitleStep1') :
            step === 2 ? t('auth.register.subtitleStep2') :
            t('auth.register.subtitleStep3')
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
          <el-form-item :label="t('auth.register.emailLabel')" prop="email">
            <el-input v-model="form.email" :placeholder="t('auth.register.emailPlaceholder')" size="large" />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.register.sendOtpBtn') }}
          </el-button>
          
          <p class="auth-footer-text">
            {{ t('auth.register.hasAccount') }} <router-link to="/login">{{ t('auth.nav.login') }}</router-link>
          </p>
        </el-form>

        <!-- Step 2: OTP Verification -->
        <el-form v-else-if="step === 2" class="auth-form" @submit.prevent="handleVerifyOtp" label-position="top">
          <div class="otp-instruction" v-html="t('auth.register.otpInstructionHtml', { email: '<strong>' + form.email + '</strong>' })">
          </div>
          <el-form-item :label="t('auth.register.otpLabel')">
            <el-input v-model="form.otp" :placeholder="t('auth.register.otpPlaceholder')" size="large" maxlength="6" class="otp-input" />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.register.verifyOtpBtn') }}
          </el-button>
          
          <p class="auth-footer-text">
            {{ t('auth.register.noOtpCode') }}
            <a href="#" @click.prevent="step = 1">{{ t('auth.register.changeEmail') }}</a>
            {{ t('auth.register.or') }}
            <a href="#" @click.prevent="handleSendOtp">{{ t('auth.register.resendOtp') }}</a>
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
          <el-form-item :label="t('auth.register.fullNameLabel')" prop="fullName">
            <el-input v-model="form.fullName" :placeholder="t('auth.register.fullNamePlaceholder')" size="large" />
          </el-form-item>

          <el-form-item :label="t('auth.register.passwordLabel')" prop="password">
            <el-input v-model="form.password" type="password" :placeholder="t('auth.register.passwordPlaceholder')" size="large" show-password />
          </el-form-item>
          
          <el-button type="primary" native-type="submit" class="auth-btn" size="large" :loading="isLoading">
            {{ t('auth.register.submitBtn') }}
          </el-button>
        </el-form>
      </div>
      
      <div class="auth-small-links">
        <a href="#">{{ t('auth.register.terms') }}</a> &nbsp;•&nbsp; <a href="#">{{ t('auth.register.privacy') }}</a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '../api/axiosClient'
import { useI18n } from '@/composables/useI18n'
import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const { t } = useI18n()

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

const rules = computed(() => ({
  email: [
    { required: true, message: t('auth.register.rules.emailRequired'), trigger: 'blur' },
    { type: 'email', message: t('auth.register.rules.emailInvalid'), trigger: ['blur', 'change'] }
  ],
  fullName: [
    { required: true, message: t('auth.register.rules.nameRequired'), trigger: 'blur' },
    { min: 2, message: t('auth.register.rules.nameMin'), trigger: 'blur' }
  ],
  password: [
    { required: true, message: t('auth.register.rules.passwordRequired'), trigger: 'blur' },
    { min: 6, message: t('auth.register.rules.passwordMin'), trigger: 'blur' },
    { 
      pattern: /^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/, 
      message: t('auth.register.rules.passwordComplexity'),
      trigger: 'blur' 
    }
  ]
}))

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
        ElMessage.success(response.data.message || t('auth.forgotPassword.otpSentMessage'));
        form.otp = ''; // Reset OTP field
        step.value = 2;
      } catch (error) {
        console.error('Send OTP error:', error);
        ElMessage.error(error.response?.data?.message || t('auth.register.messages.sendOtpFailed'));
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
    ElMessage.error(t('auth.register.messages.invalidOtpFormat'));
    return;
  }
  
  isLoading.value = true;
  try {
    const response = await axiosClient.post('/auth/verify-otp', { 
      email: form.email, 
      otpCode: form.otp 
    });
    
    if (response.data.verified) {
      ElMessage.success(t('auth.register.messages.otpVerified'));
      verifiedOtpToken.value = response.data.otpToken; // Save token for register step
      step.value = 3;
    }
  } catch (error) {
    console.error('Verify OTP error:', error);
    ElMessage.error(error.response?.data?.message || t('auth.messages.otpInvalid'));
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
        
        ElMessage.success(t('auth.register.messages.registerSuccess'));
        router.push('/login');
      } catch (error) {
        console.error('Register error:', error);
        ElMessage.error(error.response?.data?.message || t('auth.register.messages.registerFailed'));
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

