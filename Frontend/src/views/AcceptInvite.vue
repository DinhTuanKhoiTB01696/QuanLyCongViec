<template>
  <div class="invite-page" data-theme="light">
    <!-- Blurred skeleton background to mimic Jira/SprintA dashboard -->
    <div class="skeleton-bg">
      <div class="sk-topbar"></div>
      <div class="sk-body">
        <div class="sk-sidebar"></div>
        <div class="sk-content">
          <div class="sk-header"></div>
          <div class="sk-board">
            <div class="sk-col">
              <div class="sk-card"></div>
              <div class="sk-card"></div>
              <div class="sk-card"></div>
            </div>
            <div class="sk-col">
              <div class="sk-card"></div>
            </div>
            <div class="sk-col">
              <div class="sk-card"></div>
              <div class="sk-card"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Modal -->
    <div class="invite-card">
      <div class="brand-row">
        <img :src="logoImg" alt="SprintA" />
        <span>SprintA</span>
      </div>

      <div v-if="loading" class="state-block">
        <i class="fa-solid fa-spinner fa-spin"></i>
        <h1>Đang kiểm tra thông tin...</h1>
        <p>SprintA đang xác minh liên kết của bạn.</p>
      </div>

      <div v-else-if="error" class="state-block">
        <i class="fa-solid fa-circle-xmark danger"></i>
        <h1>Liên kết không hợp lệ</h1>
        <p>{{ error }}</p>
        <el-button type="primary" class="primary-btn" @click="router.push('/login')">
          Về trang đăng nhập
        </el-button>
      </div>

      <div v-else-if="success" class="state-block">
        <i class="fa-solid fa-circle-check success"></i>
        <h1>Đã thiết lập tài khoản</h1>
        <p v-if="requiresLogin">
          Tài khoản đã sẵn sàng cho <strong>{{ invite.email }}</strong>.
          Hãy đăng nhập để tiếp tục.
        </p>
        <p v-else>
          Tài khoản của bạn đã sẵn sàng. Bạn sẽ được chuyển tới bảng điều khiển.
        </p>
        <el-button type="primary" class="primary-btn mt-6" @click="continueAfterSuccess">
          {{ requiresLogin ? 'Đăng nhập' : 'Vào SprintA' }}
        </el-button>
      </div>

      <template v-else>
        <!-- STEP 1: OTP VERIFICATION -->
        <div v-if="step === 1" class="step-container text-center">
          <h1>Mã xác thực đã được gửi</h1>
          <p class="subtitle">
            Để hoàn tất việc thiết lập tài khoản, vui lòng nhập mã xác thực gồm 6 chữ số mà chúng tôi đã gửi tới:<br/>
            <strong>{{ maskedEmail }}</strong>
          </p>

          <div class="otp-inputs">
            <input 
              v-for="(digit, index) in otpDigits" 
              :key="index" 
              ref="otpRefs"
              v-model="otpDigits[index]" 
              maxlength="1" 
              class="otp-box"
              @input="onOtpInput(index)"
              @keydown.delete="onOtpDelete($event, index)"
              @paste.prevent="onOtpPaste"
            />
          </div>

          <el-button
            type="primary"
            class="primary-btn"
            size="large"
            :loading="isVerifyingOtp"
            :disabled="!isOtpComplete"
            @click="verifyOtp"
          >
            Xác thực
          </el-button>

          <div class="resend-link">
             <a href="#" @click.prevent="sendOtp">Chưa nhận được mã? Gửi lại email</a>
          </div>
        </div>

        <!-- STEP 2: FINISH SETTING UP ACCOUNT / REGISTERED USERS -->
        <div v-if="step === 2" class="step-container">
          <h1 class="text-center">{{ invite.isRegistered ? 'Tài khoản đã tồn tại' : 'Hoàn tất thiết lập tài khoản' }}</h1>

          <div class="verified-email">
            <span class="label">Địa chỉ email</span> <i class="fa-solid fa-circle-check text-success"></i>
            <br>
            <strong>{{ invite.email }}</strong>
          </div>

          <p v-if="invite.isRegistered" class="subtitle text-center mt-12">
            Bạn đã có tài khoản trên hệ thống. Chỉ cần chấp nhận lời mời này để tham gia dự án.
          </p>

          <el-form
            v-else
            ref="inviteFormRef"
            :model="form"
            :rules="rules"
            class="invite-form"
            label-position="top"
            @submit.prevent="acceptInvite"
          >
            <el-form-item label="Họ và tên *" prop="fullName">
              <el-input v-model="form.fullName" size="large" placeholder="Nhập họ tên của bạn" />
            </el-form-item>

            <el-form-item label="Mật khẩu *" prop="password">
              <el-input
                v-model="form.password"
                type="password"
                size="large"
                placeholder="Tạo mật khẩu"
                show-password
              />
              <div class="password-hint">Mật khẩu phải có ít nhất 6 ký tự</div>
            </el-form-item>

            <div class="terms-note">
              Bằng cách nhấp vào "Tiếp tục", bạn đồng ý với các <a href="#">Điều khoản Dịch vụ</a> của SprintA và xác nhận <a href="#">Chính sách Bảo mật</a>.
            </div>
          </el-form>

          <el-button
            type="primary"
            class="primary-btn"
            size="large"
            :loading="isSubmitting"
            @click="acceptInvite"
          >
            Tiếp tục
          </el-button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, computed, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import logoImg from '@/assets/logo_QLCV.png'

const route = useRoute()
const router = useRouter()

const token = ref('')
const invite = ref({})
const loading = ref(true)
const error = ref('')
const success = ref(false)
const requiresLogin = ref(false)
const redirectPath = ref('/dashboard')

// Step 1: OTP, Step 2: Form/Accept
const step = ref(0) 

// OTP States
const otpDigits = ref(['', '', '', '', '', ''])
const otpRefs = ref([])
const isVerifyingOtp = ref(false)

const maskedEmail = computed(() => {
  const email = invite.value?.email
  if (!email) return ''
  const parts = email.split('@')
  if (parts.length !== 2) return email
  const name = parts[0]
  if (name.length <= 2) return email
  return name.substring(0, 2) + '*'.repeat(name.length - 2) + '@' + parts[1]
})

const isOtpComplete = computed(() => otpDigits.value.every(d => d.trim() !== ''))

const onOtpInput = (index) => {
  const val = otpDigits.value[index]
  if (val && index < 5) {
    otpRefs.value[index + 1]?.focus()
  } else if (!val && index > 0) {
    // Prevent backward jump on input logic (handled in delete logic)
  }
}

const onOtpDelete = (e, index) => {
  if (!otpDigits.value[index] && index > 0) {
    otpRefs.value[index - 1]?.focus()
    otpDigits.value[index - 1] = ''
  }
}

const onOtpPaste = (e) => {
  const paste = (e.clipboardData || window.clipboardData).getData('text').trim()
  if (!paste) return
  for (let i = 0; i < 6; i++) {
    if (paste[i]) {
      otpDigits.value[i] = paste[i]
    }
  }
  const lastIndex = Math.min(paste.length - 1, 5)
  otpRefs.value[lastIndex]?.focus()
}

const sendOtp = async () => {
  try {
    const loadingMessage = ElMessage({
      message: 'Đang gửi mã xác nhận...',
      type: 'info',
      duration: 0
    })
    await axiosClient.post('/auth/send-otp', { email: invite.value.email })
    loadingMessage.close()
    ElMessage.success('Đã gửi mã xác nhận đến email của bạn.')
    // Clear old OTP digits if regenerating
    otpDigits.value = ['', '', '', '', '', '']
    nextTick(() => otpRefs.value[0]?.focus())
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Không thể gửi mã xác nhận.')
  }
}

const verifyOtp = async () => {
  if (!isOtpComplete.value) return
  isVerifyingOtp.value = true
  const otpCode = otpDigits.value.join('')
  try {
    await axiosClient.post('/auth/verify-otp', {
      email: invite.value.email,
      otpCode: otpCode
    })
    // Successfully verified OTP, proceed to Full Name / Password form
    step.value = 2
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Mã xác nhận không hợp lệ hoặc đã hết hạn.')
    otpDigits.value = ['', '', '', '', '', '']
    nextTick(() => otpRefs.value[0]?.focus())
  } finally {
    isVerifyingOtp.value = false
  }
}

// Form logic for Setup Account
const isSubmitting = ref(false)
const inviteFormRef = ref(null)

const form = reactive({
  fullName: '',
  password: ''
})

const rules = {
  fullName: [
    { required: true, message: 'Vui lòng nhập họ và tên', trigger: 'blur' },
    { min: 2, message: 'Tên phải có ít nhất 2 ký tự', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Vui lòng tạo mật khẩu', trigger: 'blur' },
    { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự', trigger: 'blur' }
  ]
}

const loadInvite = async () => {
  token.value = String(route.query.token || '')
  if (!token.value) {
    error.value = 'Thiếu mã xác nhận trong đường dẫn.'
    loading.value = false
    return
  }

  loading.value = true
  try {
    const response = await axiosClient.get('/auth/invite-info', {
      params: { token: token.value }
    })
    invite.value = response.data?.data || {}
    form.fullName = invite.value.fullName || ''
    
    // Auto-trigger OTP step if NOT registered
    if (!invite.value.isRegistered) {
      await sendOtp()
      step.value = 1
    } else {
      // If already registered, skip OTP Verification and go straight to Acceptance screen
      step.value = 2
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Liên kết mời không tồn tại hoặc đã hết hạn.'
  } finally {
    loading.value = false
  }
}

const acceptInvite = async () => {
  if (!invite.value.isRegistered && inviteFormRef.value) {
    const valid = await inviteFormRef.value.validate().catch(() => false)
    if (!valid) return
  }

  isSubmitting.value = true
  try {
    const payload = {
      token: token.value,
      fullName: invite.value.isRegistered ? null : form.fullName,
      password: invite.value.isRegistered ? null : form.password
    }

    const response = await axiosClient.post('/auth/accept-invite-token', payload)
    const data = response.data?.data || {}
    requiresLogin.value = data.requiresLogin === true
    redirectPath.value = data.redirectPath || '/dashboard'

    if (data.auth?.accessToken) {
      const { accessToken, fullName, email, systemRoles, id } = data.auth
      localStorage.setItem('accessToken', accessToken)
      localStorage.setItem('user', JSON.stringify({ id, fullName, email, systemRoles }))
    }

    success.value = true
  } catch (err) {
    error.value = err.response?.data?.message || 'Không thể thiết lập tài khoản lúc này.'
  } finally {
    isSubmitting.value = false
  }
}

const continueAfterSuccess = () => {
  if (requiresLogin.value) {
    router.push({
      path: '/login',
      query: { redirect: redirectPath.value }
    })
    return
  }
  router.push(redirectPath.value)
}

onMounted(loadInvite)
</script>

<style scoped>
.invite-page {
  position: relative;
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f4f5f7;
  padding: 24px;
  font-family: Inter, system-ui, sans-serif;
  overflow: hidden;
}

/* Skeleton Dashboard Blur Logic */
.skeleton-bg {
  position: absolute;
  inset: 0;
  z-index: 0;
  display: flex;
  flex-direction: column;
  background: #ffffff;
  pointer-events: none;
  filter: blur(8px);
  opacity: 0.6;
}
.sk-topbar {
  height: 56px;
  background: #dfe1e6;
  border-bottom: 1px solid #c1c7d0;
}
.sk-body {
  display: flex;
  flex: 1;
}
.sk-sidebar {
  width: 240px;
  background: #fafbfc;
  border-right: 1px solid #dfe1e6;
}
.sk-content {
  flex: 1;
  padding: 32px 40px;
}
.sk-header {
  height: 40px;
  width: 300px;
  background: #dfe1e6;
  border-radius: 4px;
  margin-bottom: 24px;
}
.sk-board {
  display: flex;
  gap: 16px;
}
.sk-col {
  flex: 1;
  background: #f4f5f7;
  border-radius: 6px;
  padding: 12px;
  min-height: 400px;
}
.sk-card {
  height: 80px;
  background: #ffffff;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  margin-bottom: 12px;
  box-shadow: 0 1px 2px rgba(9, 30, 66, 0.1);
}

/* Modal styles */
.invite-card {
  position: relative;
  z-index: 10;
  width: min(480px, 100%);
  background: #ffffff;
  border-radius: 6px;
  padding: 48px;
  box-shadow: 0 8px 32px rgba(9, 30, 66, 0.25);
  display: flex;
  flex-direction: column;
  align-items: center;
}

.brand-row {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-bottom: 32px;
  color: #172b4d;
  font-size: 26px;
  font-weight: 800;
}

.brand-row img {
  width: 40px;
  height: 40px;
  object-fit: contain;
}

.step-container {
  width: 100%;
}

.text-center {
  text-align: center;
}

h1 {
  margin: 0 0 12px;
  color: #172b4d;
  font-size: 24px;
  font-weight: 700;
  line-height: 1.25;
}

.subtitle {
  color: #44546f;
  font-size: 14px;
  line-height: 1.6;
  margin-bottom: 32px;
}

.otp-inputs {
  display: flex;
  justify-content: center;
  gap: 12px;
  margin-bottom: 36px;
}

.otp-box {
  width: 52px;
  height: 60px;
  font-size: 28px;
  font-weight: 600;
  text-align: center;
  border: 2px solid #dfe1e6;
  border-radius: 6px;
  outline: none;
  background: #ffffff;
  color: #172b4d;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.otp-box:focus {
  border-color: #0c66e4;
  box-shadow: 0 0 0 2px rgba(12, 102, 228, 0.2);
}

.resend-link {
  margin-top: 32px;
  text-align: center;
}

.resend-link a {
  color: #0c66e4;
  font-size: 14px;
  text-decoration: none;
  font-weight: 500;
}

.resend-link a:hover {
  text-decoration: underline;
}

.verified-email {
  margin-bottom: 32px;
  font-size: 14px;
}

.verified-email .label {
  color: #44546f;
  font-weight: 600;
}

.verified-email strong {
  display: block;
  margin-top: 6px;
  color: #172b4d;
  font-size: 16px;
  font-weight: 700;
}

.text-success {
  color: #22a06b;
  margin-left: 4px;
}

.mt-12 {
  margin-top: 12px;
}

.mt-6 {
  margin-top: 24px;
}

.invite-form {
  margin-top: 24px;
}

.password-hint {
  font-size: 12px;
  color: #626f86;
  margin-top: 6px;
}

.terms-note {
  font-size: 12px;
  color: #626f86;
  line-height: 1.5;
  margin: 32px 0 24px;
  text-align: center;
}

.terms-note a {
  color: #0c66e4;
  text-decoration: none;
}

.terms-note a:hover {
  text-decoration: underline;
}

.primary-btn {
  width: 100%;
  min-height: 48px;
  border-radius: 4px;
  background: #0c66e4 !important;
  border-color: #0c66e4 !important;
  font-weight: 600;
  font-size: 15px;
}

.primary-btn:disabled {
  background: #b3d4ff !important;
  border-color: #b3d4ff !important;
  cursor: not-allowed;
}

.state-block {
  text-align: center;
}

.state-block i {
  color: #0c66e4;
  font-size: 48px;
  margin-bottom: 24px;
}

.state-block i.success {
  color: #22a06b;
}

.state-block i.danger {
  color: #e2483d;
}

:deep(.el-form-item__label) {
  color: #44546f;
  font-weight: 700;
  padding-bottom: 6px;
  font-size: 12px;
}

:deep(.el-input__wrapper) {
  border-radius: 4px;
  box-shadow: 0 0 0 2px #dfe1e6 inset;
}

:deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px #0c66e4 inset;
}

@media (max-width: 560px) {
  .invite-card {
    padding: 32px 24px;
  }
  .otp-box {
    width: 44px;
    height: 52px;
    font-size: 24px;
  }
}
</style>
