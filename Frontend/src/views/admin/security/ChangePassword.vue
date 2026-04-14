<template>
  <AdminLayout>
    <div class="admin-page-container">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-shield-halved"></i> Security / Change Password
        </div>
        <h1 class="page-title">Đổi mật khẩu</h1>
        <p class="page-subtitle">Đảm bảo an toàn tài khoản bằng cách tạo một mật khẩu mạnh và duy nhất.</p>
      </div>

      <div class="form-container">
        <div class="settings-card">
          <div style="max-width: 600px;">
            
            <!-- STEP 1: XÁC THỰC EMAIL & OTP -->
            <div v-show="currentStep === 1" class="step-container">
              <div class="form-group mb-24">
                <label class="form-label">Email xác thực tài khoản</label>
                <div style="display: flex; gap: 12px; align-items: center;">
                   <el-input v-model="form.email" placeholder="Nhập địa chỉ email của bạn..." class="glass-input" style="flex: 1" />
                   <el-button type="primary" plain @click="sendChangeOtp" :loading="isSendingOtp" style="height: 38px">Gửi mã xác thực</el-button>
                </div>
              </div>

              <div class="form-group mb-24">
                <label class="form-label">Mã xác nhận (OTP) gửi qua Email</label>
                <el-input v-model="form.otpCode" maxlength="6" placeholder="Nhập 6 số mã OTP..." class="glass-input" />
              </div>

              <div class="action-footer mt-32" style="display: flex; justify-content: flex-end;">
                <el-button type="primary" :disabled="form.otpCode.length < 6" @click="currentStep = 2" style="padding: 20px 24px; font-weight: 500;">
                  Xác nhận & Tiếp tục
                </el-button>
              </div>
            </div>

            <!-- STEP 2: NHẬP MẬT KHẨU MỚI -->
            <div v-show="currentStep === 2" class="step-container">
              <div class="form-group mt-24">
              <label class="form-label">Mật khẩu mới</label>
              <el-input type="password" v-model="form.newPassword" @input="checkStrength" show-password placeholder="Tạo mật khẩu mới..." class="glass-input" />
              
              <!-- Password Strength Indicator -->
              <div class="password-strength mt-12" v-if="form.newPassword">
                <div class="strength-bars">
                   <div class="bar" :class="strength > 0 ? strengthClass : ''"></div>
                   <div class="bar" :class="strength > 1 ? strengthClass : ''"></div>
                   <div class="bar" :class="strength > 2 ? strengthClass : ''"></div>
                   <div class="bar" :class="strength > 3 ? strengthClass : ''"></div>
                </div>
                <span class="strength-text" :class="strengthClass">{{ strengthLabel }}</span>
              </div>

              <div class="password-hints mt-12">
                 <ul>
                   <li :class="{ 'passed': hints.length }"><i class="fa-solid" :class="hints.length ? 'fa-check' : 'fa-circle-dot'"></i> Ít nhất 8 ký tự</li>
                   <li :class="{ 'passed': hints.uppercase }"><i class="fa-solid" :class="hints.uppercase ? 'fa-check' : 'fa-circle-dot'"></i> Ít nhất một chữ hoa</li>
                   <li :class="{ 'passed': hints.number }"><i class="fa-solid" :class="hints.number ? 'fa-check' : 'fa-circle-dot'"></i> Ít nhất một số</li>
                   <li :class="{ 'passed': hints.special }"><i class="fa-solid" :class="hints.special ? 'fa-check' : 'fa-circle-dot'"></i> Có chứa ký tự đặc biệt</li>
                 </ul>
              </div>
            </div>

            <div class="form-group mt-24">
              <label class="form-label">Xác nhận mật khẩu mới</label>
              <el-input type="password" v-model="form.confirmPassword" show-password placeholder="Nhập lại mật khẩu để xác nhận..." class="glass-input" />
              <div v-if="form.confirmPassword && form.newPassword !== form.confirmPassword" class="error-msg mt-2 text-danger">
                Mật khẩu xác nhận không khớp.
              </div>
            </div>

            <div class="divider mt-24 mb-24"></div>

            <!-- Remote Logout Option -->
            <div class="form-group checkbox-group">
              <el-checkbox v-model="form.logoutOthers" class="logout-checkbox">
                <div class="checkbox-content">
                  <span class="checkbox-title">Đăng xuất khỏi tất cả các thiết bị khác</span>
                  <span class="checkbox-desc">Loại bỏ mọi phiên đăng nhập trên các máy tính và thiêt bị di động khác ngoài thiết bị hiện tại của bạn.</span>
                </div>
              </el-checkbox>
            </div>

            <div class="action-footer mt-32" style="display: flex; gap: 12px; justify-content: flex-end;">
              <el-button plain @click="currentStep = 1" style="padding: 20px 24px; font-weight: 500;">
                Quay lại
              </el-button>
              <el-button type="primary" :disabled="!isValid" :loading="isSaving" @click="submitPassword" style="padding: 20px 24px; font-weight: 500;">
                {{ hasPassword ? 'Cập nhật mật khẩu' : 'Tạo mật khẩu an toàn' }}
              </el-button>
            </div>
            
            </div> <!-- Kết thúc step 2 -->
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'

const isSaving = ref(false)
const isSendingOtp = ref(false)
const hasPassword = ref(true)
const hasLoaded = ref(false)
const currentStep = ref(1)

const loadProfile = async () => {
  try {
    const { data } = await axiosClient.get('/users/me')
    hasPassword.value = data.data.hasPassword
    hasLoaded.value = true
  } catch(err) {
    console.error(err)
  }
}

onMounted(() => {
  loadProfile()
})

const form = reactive({
  email: '',
  otpCode: '',
  newPassword: '',
  confirmPassword: '',
  logoutOthers: true
})

const hints = reactive({
  length: false,
  uppercase: false,
  number: false,
  special: false
})

const strength = ref(0)
const strengthClass = ref('')
const strengthLabel = ref('')

const checkStrength = () => {
  const p = form.newPassword;
  
  hints.length = p.length >= 8;
  hints.uppercase = /[A-Z]/.test(p);
  hints.number = /[0-9]/.test(p);
  hints.special = /[^A-Za-z0-9]/.test(p);
  
  let score = 0;
  if(hints.length) score++;
  if(hints.uppercase) score++;
  if(hints.number) score++;
  if(hints.special) score++;
  
  strength.value = score;
  
  if (score <= 1) {
    strengthClass.value = 'strength-weak';
    strengthLabel.value = 'Yếu';
  } else if (score === 2) {
    strengthClass.value = 'strength-fair';
    strengthLabel.value = 'Trung bình';
  } else if (score === 3) {
    strengthClass.value = 'strength-good';
    strengthLabel.value = 'Khá';
  } else {
    strengthClass.value = 'strength-strong';
    strengthLabel.value = 'Rất Tốt';
  }
}

const isValid = computed(() => {
  const commonValid = strength.value === 4 && form.newPassword === form.confirmPassword;
  return form.email && form.otpCode.length >= 6 && commonValid;
})

const sendChangeOtp = async () => {
  if (!form.email) {
    ElMessage.warning('Vui lòng nhập email trước khi gửi mã OTP.');
    return;
  }
  isSendingOtp.value = true;
  try {
    const endpoint = hasPassword.value ? '/users/send-change-password-otp' : '/users/send-set-password-otp';
    const { data } = await axiosClient.post(endpoint, { email: form.email })
    ElMessage.success(data.message)
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Không thể gửi mã OTP')
  } finally {
    isSendingOtp.value = false;
  }
}

const submitPassword = async () => {
  try {
    isSaving.value = true;
    
    if (hasPassword.value) {
      await axiosClient.put('/users/change-password-with-otp', {
        email: form.email,
        otpCode: form.otpCode,
        newPassword: form.newPassword,
        logoutOthers: form.logoutOthers
      })
      ElMessage.success('Đổi mật khẩu thành công.');
    } else {
      await axiosClient.post('/users/set-password-with-otp', {
        otpCode: form.otpCode,
        newPassword: form.newPassword
      })
      ElMessage.success('Tạo mật khẩu mới thành công! Ghi nhận vào tài khoản.');
      hasPassword.value = true;
    }

    form.otpCode = '';
    form.newPassword = '';
    form.confirmPassword = '';
    form.email = '';
    currentStep.value = 1;
    strength.value = 0;
    hints.length = false;
    hints.uppercase = false;
    hints.number = false;
    hints.special = false;
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Thao tác thất bại. Vui lòng kiểm tra lại mã xác thực.');
    currentStep.value = 1;
  } finally {
    isSaving.value = false;
  }
}
</script>

<style scoped>
.page-header {
  margin-bottom: 24px;
}

.breadcrumb {
  font-size: 13px;
  color: #8b949e;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.page-subtitle {
  font-size: 14px;
  color: #8b949e;
}

.settings-card {
  background-color: var(--bg-card);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 32px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
}

.form-label {
  display: block;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.mt-24 { margin-top: 24px; }
.mt-12 { margin-top: 12px; }
.mt-2 { margin-top: 4px; }
.mb-24 { margin-bottom: 24px; }
.mt-32 { margin-top: 32px; }
.text-danger { color: #ef4444; font-size: 13px;}

.divider {
  height: 1px;
  background-color: var(--border-color);
  margin: 24px 0;
}

.password-strength {
  display: flex;
  align-items: center;
  gap: 12px;
}

.strength-bars {
  display: flex;
  gap: 4px;
  flex: 1;
  max-width: 200px;
}

.strength-bars .bar {
  flex: 1;
  height: 6px;
  background-color: var(--border-color);
  border-radius: 3px;
  transition: background-color 0.3s ease;
}

.bar.strength-weak { background-color: #ef4444; }
.bar.strength-fair { background-color: #f59e0b; }
.bar.strength-good { background-color: #3b82f6; }
.bar.strength-strong { background-color: #10b981; }

.strength-text {
  font-size: 12px;
  font-weight: 600;
}

.strength-text.strength-weak { color: #ef4444; }
.strength-text.strength-fair { color: #f59e0b; }
.strength-text.strength-good { color: #3b82f6; }
.strength-text.strength-strong { color: #10b981; }

.password-hints ul {
  list-style: none;
  padding: 0;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.password-hints li {
  font-size: 13px;
  color: #8b949e;
  display: flex;
  align-items: center;
  gap: 6px;
}

.password-hints li i {
  font-size: 11px;
}

.password-hints li.passed {
  color: #10b981;
}

:deep(.logout-checkbox) {
  display: block;
  height: auto;
  white-space: normal;
}

:deep(.logout-checkbox .el-checkbox__label) {
  padding-left: 12px;
}

.checkbox-content {
  display: flex;
  flex-direction: column;
}

.checkbox-title {
  font-weight: 600;
  color: var(--text-primary);
  font-size: 14px;
}

.checkbox-desc {
  font-size: 13px;
  color: #8b949e;
  margin-top: 2px;
}

:deep(.glass-input .el-input__wrapper) {
  background-color: rgba(255,255,255,0.02);
  border-radius: 8px;
  padding: 8px 12px;
}

/* Fix browser autofill stuck white background */
:deep(.el-input__inner:-webkit-autofill),
:deep(.el-input__inner:-webkit-autofill:hover),
:deep(.el-input__inner:-webkit-autofill:focus),
:deep(.el-input__inner:-webkit-autofill:active) {
  -webkit-text-fill-color: var(--text-primary) !important;
  transition: background-color 5000s ease-in-out 0s;
}
</style>
