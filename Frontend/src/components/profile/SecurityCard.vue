<template>
  <div class="settings-card security-card">
    <!-- Overview View -->
    <div v-if="view === 'security'" class="security-view-wrapper">
      <div class="card-header">
        <h2 class="card-title">{{ t('Security Settings', 'Cài đặt Bảo mật') }}</h2>
        <p class="card-subtitle">{{ t('Manage your account security, passwords, 2FA, and monitor active sessions.', 'Quản lý bảo mật tài khoản, mật khẩu, xác thực 2 yếu tố (2FA) và theo dõi các phiên đăng nhập.') }}</p>
      </div>

      <div class="security-sections-list">
        <!-- Password Row -->
        <div class="security-row-item">
          <div class="row-left">
            <span class="row-icon color-blue"><i class="fa-solid fa-lock"></i></span>
            <div class="row-text">
              <h4 class="item-title">{{ t('Password', 'Mật khẩu') }}</h4>
              <p class="item-desc">{{ t('Change your password regularly to keep your account secure.', 'Thay đổi mật khẩu định kỳ để giữ tài khoản của bạn luôn an toàn.') }}</p>
            </div>
          </div>
          <el-button type="primary" plain size="small" @click="$emit('select-tab', 'password')">
            {{ t('Update password', 'Cập nhật mật khẩu') }}
          </el-button>
        </div>

        <!-- 2FA Row -->
        <div class="security-row-item">
          <div class="row-left">
            <span class="row-icon color-green"><i class="fa-solid fa-key"></i></span>
            <div class="row-text">
              <h4 class="item-title">{{ t('Two-factor Authentication', 'Xác thực 2 yếu tố (2FA)') }}</h4>
              <p class="item-desc">{{ t('Add an extra layer of security to your account with Google Authenticator.', 'Thêm một lớp bảo mật bổ sung cho tài khoản bằng ứng dụng Google Authenticator.') }}</p>
            </div>
          </div>
          <el-button type="primary" plain size="small" @click="$emit('select-tab', 'mfa')">
            {{ t('Configure 2FA', 'Cấu hình 2FA') }}
          </el-button>
        </div>

        <!-- Passkeys Row -->
        <div class="security-row-item">
          <div class="row-left">
            <span class="row-icon color-purple"><i class="fa-solid fa-fingerprint"></i></span>
            <div class="row-text">
              <h4 class="item-title">{{ t('Passkeys', 'Khóa bảo mật (Passkeys)') }}</h4>
              <p class="item-desc">{{ t('Use biometric login (fingerprint, face recognition) for passwordless access.', 'Sử dụng vân tay, nhận diện khuôn mặt hoặc khóa bảo mật để đăng nhập không cần mật khẩu.') }}</p>
            </div>
          </div>
          <el-button type="primary" plain size="small" @click="addPasskey">
            {{ t('Add passkey [FLOW REQUIRED]', 'Thêm passkey [CẦN FLOW]') }}
          </el-button>
        </div>

        <!-- Active Sessions Row -->
        <div class="security-row-item">
          <div class="row-left">
            <span class="row-icon color-orange"><i class="fa-solid fa-desktop"></i></span>
            <div class="row-text">
              <h4 class="item-title">{{ t('Active Sessions', 'Các phiên đang hoạt động') }}</h4>
              <p class="item-desc">{{ t('Monitor and log out from your active sessions on other devices.', 'Theo dõi và đăng xuất khỏi các thiết bị đang đăng nhập tài khoản của bạn.') }}</p>
            </div>
          </div>
          <el-button type="primary" plain size="small" @click="$emit('select-tab', 'sessions')">
            {{ t('Manage sessions', 'Quản lý phiên') }}
          </el-button>
        </div>
      </div>

      <hr class="section-divider" />

      <!-- Recent Login Activity -->
      <div class="recent-activity-section">
        <h3 class="section-title"><i class="fa-solid fa-history mr-2"></i> {{ t('Recent Login Activity', 'Hoạt động đăng nhập gần đây') }}</h3>
        <p class="section-desc">{{ t('Review your recent login times, locations, and browser details.', 'Xem lại thời gian, vị trí và trình duyệt đăng nhập gần đây của bạn.') }}</p>
        
        <div class="activity-table-wrapper">
          <table class="activity-table">
            <thead>
              <tr>
                <th>{{ t('Time', 'Thời gian') }}</th>
                <th>{{ t('Device / Browser', 'Thiết bị / Trình duyệt') }}</th>
                <th>IP</th>
                <th>{{ t('Location', 'Vị trí') }}</th>
                <th>{{ t('Status', 'Trạng thái') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="act in recentActivities" :key="act.time">
                <td>{{ act.time }}</td>
                <td>
                  <i :class="act.icon"></i>
                  {{ translateBrowser(act.device.browser) }} {{ t('on', 'trên') }} {{ translateDevice(act.device.device) }}
                </td>
                <td>{{ act.ip }}</td>
                <td>{{ act.location === 'Unknown' ? t('Unknown', 'Không xác định') : act.location }}</td>
                <td>
                  <el-tag :type="act.status === 'Current' ? 'success' : 'info'" size="small" effect="plain">
                    {{ act.status === 'Current' ? t('Current', 'Hiện tại') : t('Success', 'Thành công') }}
                  </el-tag>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Password View -->
    <div v-else-if="view === 'password'" class="security-view-wrapper">
      <div class="card-header">
        <h2 class="card-title">{{ t('Change Password', 'Đổi Mật khẩu') }}</h2>
        <p class="card-subtitle">{{ t('Create a new secure password for your account.', 'Tạo mật khẩu an toàn mới cho tài khoản của bạn.') }}</p>
      </div>

      <div class="password-card-body">
        <!-- Step 1: Verification -->
        <div v-if="passwordStep === 1" class="password-step-box">
          <div class="step-indicator">
            <span class="step-label active">1. {{ t('Verify identity', 'Xác minh danh tính') }}</span>
            <span class="step-arrow"><i class="fa-solid fa-chevron-right"></i></span>
            <span class="step-label">2. {{ t('Create password', 'Tạo mật khẩu') }}</span>
          </div>

          <p class="step-desc">{{ t('Send an OTP code to your email before changing your password.', 'Gửi mã OTP đến email của bạn trước khi thay đổi mật khẩu.') }}</p>

          <div v-if="isPasswordChangeLocked" class="cooldown-alert">
            <i class="fa-solid fa-shield-halved"></i>
            <div>
              <strong>{{ t('Password changes are limited to once every 7 days.', 'Thay đổi mật khẩu bị giới hạn 7 ngày một lần.') }}</strong>
              <p>{{ t('You can change your password again after ', 'Bạn có thể thay đổi mật khẩu lần nữa sau ') + passwordEligibleLabel + t('. If this is urgent, please request admin support.', '. Nếu khẩn cấp, vui lòng gửi yêu cầu hỗ trợ tới Quản trị viên hệ thống.') }}</p>
            </div>
          </div>

          <div class="atlassian-field-group">
            <label class="field-label">{{ t('Email address', 'Địa chỉ email') }}</label>
            <el-input :model-value="profileData.email" disabled>
              <template #prefix>
                <i class="fa-solid fa-at"></i>
              </template>
            </el-input>
          </div>

          <div v-if="passwordOtpSent" class="atlassian-field-group mt-16">
            <label class="field-label">{{ t('OTP code', 'Mã OTP') }}</label>
            <el-input :model-value="passwordForm.otpCode" @input="val => passwordForm.otpCode = val" maxlength="6" :placeholder="t('Enter 6-digit OTP', 'Nhập mã OTP 6 chữ số')">
              <template #prefix>
                <i class="fa-solid fa-key"></i>
              </template>
            </el-input>
            <div class="otp-hint-block">
              <i class="fa-solid fa-circle-info text-blue"></i>
              <span>{{ t('OTP is valid for 5 minutes.', 'Mã OTP có hiệu lực trong 5 phút.') }}</span>
              <a v-if="canResendPasswordOtp" href="#" @click.prevent="$emit('send-otp')">{{ t('Resend code', 'Gửi lại mã') }}</a>
              <span v-else>({{ passwordCountdownText }})</span>
            </div>
          </div>

          <div class="step-actions mt-24">
            <el-button
              v-if="isPasswordChangeLocked"
              type="primary"
              :loading="isRequestingPasswordException"
              @click="$emit('request-exception')"
            >
              {{ t('Request admin support', 'Yêu cầu Quản trị viên hỗ trợ') }}
            </el-button>
            <el-button
              v-else-if="!passwordOtpSent"
              type="primary"
              :loading="isSendingPasswordOtp"
              :disabled="!profileData.email"
              @click="$emit('send-otp')"
            >
              {{ t('Send verification code', 'Gửi mã xác nhận') }}
            </el-button>
            <el-button
              v-else
              type="primary"
              :disabled="!passwordForm.otpCode || passwordForm.otpCode.length < 6"
              @click="$emit('go-to-step-2')"
            >
              {{ t('Verify & continue', 'Xác minh & Tiếp tục') }}
            </el-button>
          </div>
        </div>

        <!-- Step 2: New Password -->
        <div v-else-if="passwordStep === 2" class="password-step-box">
          <div class="step-indicator">
            <span class="step-label done"><i class="fa-solid fa-check mr-1"></i> {{ t('Verified', 'Đã xác minh') }}</span>
            <span class="step-arrow"><i class="fa-solid fa-chevron-right"></i></span>
            <span class="step-label active">2. {{ t('Create password', 'Tạo mật khẩu') }}</span>
          </div>

          <p class="step-desc">{{ t('Use a strong password that you do not use elsewhere.', 'Sử dụng mật khẩu mạnh mà bạn không sử dụng ở nơi khác.') }}</p>

          <div class="atlassian-field-group">
            <label class="field-label">{{ t('New password', 'Mật khẩu mới') }}</label>
            <el-input :model-value="passwordForm.newPassword" @input="onPasswordInput" type="password" show-password :placeholder="t('Create a new password', 'Nhập mật khẩu mới')" />
            
            <div v-if="passwordForm.newPassword" class="password-strength-meter">
              <div class="strength-bars">
                <div class="bar" :class="passwordStrength > 0 ? passwordStrengthClass : ''"></div>
                <div class="bar" :class="passwordStrength > 1 ? passwordStrengthClass : ''"></div>
                <div class="bar" :class="passwordStrength > 2 ? passwordStrengthClass : ''"></div>
                <div class="bar" :class="passwordStrength > 3 ? passwordStrengthClass : ''"></div>
              </div>
              <span class="strength-label" :class="passwordStrengthClass">{{ passwordStrengthLabel }}</span>
            </div>

            <ul class="password-checklist">
              <li :class="{ passed: passwordHints.length }"><i class="fa-solid" :class="passwordHints.length ? 'fa-check-circle' : 'fa-circle-dot'"></i> {{ t('At least 8 characters', 'Tối thiểu 8 ký tự') }}</li>
              <li :class="{ passed: passwordHints.uppercase }"><i class="fa-solid" :class="passwordHints.uppercase ? 'fa-check-circle' : 'fa-circle-dot'"></i> {{ t('One uppercase letter', 'Một chữ cái viết hoa') }}</li>
              <li :class="{ passed: passwordHints.number }"><i class="fa-solid" :class="passwordHints.number ? 'fa-check-circle' : 'fa-circle-dot'"></i> {{ t('One number', 'Một chữ số') }}</li>
              <li :class="{ passed: passwordHints.special }"><i class="fa-solid" :class="passwordHints.special ? 'fa-check-circle' : 'fa-circle-dot'"></i> {{ t('One special character', 'Một ký tự đặc biệt') }}</li>
            </ul>
          </div>

          <div class="atlassian-field-group mt-16">
            <label class="field-label">{{ t('Confirm password', 'Xác nhận mật khẩu') }}</label>
            <el-input :model-value="passwordForm.confirmPassword" @input="val => passwordForm.confirmPassword = val" type="password" show-password :placeholder="t('Re-enter your password', 'Nhập lại mật khẩu mới')" />
            <div v-if="passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword" class="error-text">
              {{ t('Passwords do not match.', 'Mật khẩu xác nhận không khớp.') }}
            </div>
          </div>

          <div class="logout-option mt-16">
            <el-checkbox :model-value="passwordForm.logoutOthers" @change="val => passwordForm.logoutOthers = val">
              <span class="checkbox-title">{{ t('Log out from all other devices', 'Đăng xuất khỏi tất cả các thiết bị khác') }}</span>
            </el-checkbox>
          </div>

          <div class="step-actions between mt-24">
            <el-button @click="$emit('go-to-step-1')">
              <i class="fa-solid fa-arrow-left mr-2"></i> {{ t('Go back', 'Quay lại') }}
            </el-button>
            <el-button type="primary" :disabled="!canSubmitPassword" :loading="isChangingPassword" @click="$emit('change-password')">
              {{ t('Update password', 'Cập nhật mật khẩu') }}
            </el-button>
          </div>
        </div>

        <!-- Step 3: Success -->
        <div v-else class="password-step-box success-box">
          <div class="success-icon-wrapper">
            <i class="fa-solid fa-circle-check"></i>
          </div>
          <h3 class="success-title">{{ t('Password changed successfully!', 'Đổi mật khẩu thành công!') }}</h3>
          <p class="success-desc">{{ t('Your password has been updated securely. You can now use your new password to log in.', 'Mật khẩu của bạn đã được cập nhật an toàn. Bây giờ bạn có thể sử dụng mật khẩu mới để đăng nhập.') }}</p>
          <el-button type="primary" @click="$emit('reset-password-flow')">
            {{ t('Change password again', 'Tiếp tục đổi mật khẩu') }}
          </el-button>
        </div>
      </div>
    </div>

    <!-- MFA View -->
    <div v-else-if="view === 'mfa'" class="security-view-wrapper">
      <div class="card-header">
        <h2 class="card-title">{{ t('Two-factor Authentication', 'Xác thực 2 yếu tố') }}</h2>
        <p class="card-subtitle">{{ t('Two-factor authentication adds an extra layer of protection to your account.', 'Xác thực hai yếu tố giúp bổ sung thêm một lớp bảo vệ vững chắc cho tài khoản.') }}</p>
      </div>

      <div class="mfa-body">
        <div class="mfa-status-card">
          <div class="mfa-status-info">
            <div class="mfa-icon" :class="{ active: profileData.is2FaEnabled }">
              <i class="fa-solid fa-shield-halved"></i>
            </div>
            <div>
              <h4 class="mfa-status-title">
                {{ profileData.is2FaEnabled ? t('2FA is currently Enabled', '2FA hiện đang được bật') : t('2FA is currently Disabled', '2FA hiện đang tắt') }}
              </h4>
              <p class="mfa-status-desc">
                {{ profileData.is2FaEnabled 
                  ? t('Your account is highly secure. You will need to enter Google Authenticator OTP code when logging in.', 'Tài khoản của bạn đang được bảo vệ an toàn. Bạn cần nhập mã OTP từ Google Authenticator khi đăng nhập.') 
                  : t('Protect your account by requiring an OTP code during login.', 'Bảo vệ tài khoản bằng cách yêu cầu mã OTP bổ sung khi đăng nhập.') }}
              </p>
            </div>
          </div>
          <el-switch
            :model-value="profileData.is2FaEnabled"
            active-color="#0c66e4"
            inactive-color="#dc2626"
            @change="toggleMfa"
          />
        </div>

        <div v-if="profileData.is2FaEnabled" class="mfa-setup-instructions">
          <h4 class="instruction-title">{{ t('How to configure Google Authenticator', 'Hướng dẫn cấu hình Google Authenticator') }}</h4>
          <ol class="instruction-list">
            <li>{{ t('Download Google Authenticator app on iOS or Android.', 'Tải ứng dụng Google Authenticator trên thiết bị iOS hoặc Android.') }}</li>
            <li>{{ t('Scan the QR code displayed during login setup or enter the secret key.', 'Quét mã QR hiển thị hoặc nhập khóa bí mật được cung cấp.') }}</li>
            <li>{{ t('Enter the 6-digit OTP code to verify and log in securely.', 'Nhập mã OTP gồm 6 chữ số để xác thực đăng nhập an toàn.') }}</li>
          </ol>
        </div>
      </div>
    </div>

    <!-- Sessions View -->
    <div v-else-if="view === 'sessions'" class="security-view-wrapper">
      <div class="card-header">
        <h2 class="card-title">{{ t('Sessions & Devices', 'Phiên đăng nhập & Thiết bị') }}</h2>
        <p class="card-subtitle">{{ t('See all devices that are currently logged into your account. You can log out of other sessions if needed.', 'Xem tất cả các thiết bị hiện đang đăng nhập vào tài khoản của bạn. Bạn có thể đăng xuất khỏi các phiên khác nếu cần.') }}</p>
      </div>

      <div class="sessions-body">
        <div class="sessions-list">
          <div v-for="s in activeSessions" :key="s.id" class="session-item-card" :class="{ 'is-current': s.isCurrent }">
            <div class="session-icon">
              <i :class="s.icon"></i>
            </div>
            <div class="session-info">
              <div class="session-header-row">
                <h4 class="session-device-name">{{ translateDevice(s.device) }}</h4>
                <el-tag v-if="s.isCurrent" type="success" size="small">{{ t('Current Session', 'Phiên hiện tại') }}</el-tag>
              </div>
              <p class="session-meta">{{ translateBrowser(s.browser) }} &bull; {{ s.ip }}</p>
              <p class="session-location"><i class="fa-solid fa-location-dot"></i> {{ s.location === 'Unknown' ? t('Unknown', 'Không xác định') : s.location }} &bull; {{ s.time }}</p>
            </div>
            <div class="session-action" v-if="!s.isCurrent">
              <el-button type="danger" plain size="small" @click="logoutSession(s.id)">
                {{ t('Log out', 'Đăng xuất') }}
              </el-button>
            </div>
          </div>
        </div>

        <div class="sessions-global-action" v-if="activeSessions.length > 1">
          <el-button type="danger" @click="logoutAllOthers">
            <i class="fa-solid fa-arrow-right-from-bracket mr-2"></i> {{ t('Log out of all other devices', 'Đăng xuất khỏi tất cả các thiết bị khác') }}
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useLocale } from '@/composables/useLocale'
import { ElMessage, ElMessageBox } from 'element-plus'
import axiosClient from '@/api/axiosClient'

const { t } = useLocale()

const props = defineProps({
  view: {
    type: String,
    default: 'security'
  },
  profileData: {
    type: Object,
    required: true
  },
  passwordStep: {
    type: Number,
    default: 1
  },
  passwordOtpSent: {
    type: Boolean,
    default: false
  },
  isSendingPasswordOtp: {
    type: Boolean,
    default: false
  },
  isChangingPassword: {
    type: Boolean,
    default: false
  },
  isRequestingPasswordException: {
    type: Boolean,
    default: false
  },
  canResendPasswordOtp: {
    type: Boolean,
    default: true
  },
  passwordCountdownText: {
    type: String,
    default: '0:00'
  },
  passwordForm: {
    type: Object,
    required: true
  },
  passwordHints: {
    type: Object,
    required: true
  },
  passwordStrength: {
    type: Number,
    default: 0
  },
  passwordStrengthClass: {
    type: String,
    default: ''
  },
  passwordStrengthLabel: {
    type: String,
    default: ''
  },
  canSubmitPassword: {
    type: Boolean,
    default: false
  },
  isPasswordChangeLocked: {
    type: Boolean,
    default: false
  },
  passwordEligibleLabel: {
    type: String,
    default: ''
  }
})

const emit = defineEmits([
  'select-tab',
  'send-otp',
  'request-exception',
  'change-password',
  'reset-password-flow',
  'toggle-2fa',
  'go-to-step-1',
  'go-to-step-2',
  'check-password-strength'
])

const recentActivities = ref([])
const activeSessions = ref([])
const isLoading = ref(false)

const parseUserAgent = (uaString) => {
  if (!uaString) return { device: 'Unknown Device', browser: 'Unknown Browser', icon: 'fa-solid fa-desktop' }
  const ua = uaString.toLowerCase()
  
  let device = 'Desktop PC'
  let icon = 'fa-solid fa-desktop'
  if (ua.includes('iphone') || ua.includes('ipad') || ua.includes('android') || ua.includes('mobile')) {
    device = ua.includes('iphone') ? 'iPhone' : ua.includes('ipad') ? 'iPad' : ua.includes('android') ? 'Android Device' : 'Mobile Device'
    icon = 'fa-solid fa-mobile-screen-button'
  } else if (ua.includes('macintosh') || ua.includes('mac os')) {
    device = 'MacBook / Mac'
    icon = 'fa-solid fa-laptop'
  } else if (ua.includes('windows')) {
    device = 'Windows PC'
    icon = 'fa-solid fa-desktop'
  } else if (ua.includes('linux')) {
    device = 'Linux PC'
    icon = 'fa-solid fa-desktop'
  }

  let browser = 'Unknown Browser'
  if (ua.includes('firefox')) browser = 'Firefox'
  else if (ua.includes('chrome') && !ua.includes('chromium')) browser = 'Chrome'
  else if (ua.includes('safari') && !ua.includes('chrome')) browser = 'Safari'
  else if (ua.includes('edge')) browser = 'Edge'
  else if (ua.includes('opera')) browser = 'Opera'

  return { device, browser, icon }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  try {
    return new Date(dateStr).toLocaleString()
  } catch (e) {
    return dateStr
  }
}

const loadSecurityData = async () => {
  if (isLoading.value) return
  isLoading.value = true
  try {
    const [sessionsRes, activitiesRes] = await Promise.all([
      axiosClient.get('/users/sessions'),
      axiosClient.get('/users/login-activity')
    ])

    if (sessionsRes.data && sessionsRes.data.data) {
      activeSessions.value = sessionsRes.data.data.map(session => {
        const parsed = parseUserAgent(session.device)
        return {
          id: session.id,
          device: parsed.device,
          icon: parsed.icon,
          browser: parsed.browser,
          ip: session.ip || 'Unknown',
          location: 'Unknown',
          time: session.isCurrent ? t('Active now', 'Đang hoạt động') : formatDate(session.createdAt),
          isCurrent: session.isCurrent
        }
      })
    }

    if (activitiesRes.data && activitiesRes.data.data) {
      recentActivities.value = activitiesRes.data.data.map(act => {
        let parsedUA = { device: 'Unknown Device', browser: 'Unknown Browser', icon: 'fa-solid fa-desktop' }
        try {
          if (act.details) {
            const detailsObj = typeof act.details === 'string' ? JSON.parse(act.details) : act.details
            parsedUA = parseUserAgent(detailsObj.userAgent)
          }
        } catch (e) {
          console.error(e)
        }

        return {
          time: formatDate(act.createdAt),
          device: { browser: parsedUA.browser, device: parsedUA.device },
          icon: parsedUA.icon,
          ip: act.ipAddress || 'Unknown',
          location: 'Unknown',
          status: act.status
        }
      })
    }
  } catch (error) {
    console.error('Failed to load security data:', error)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadSecurityData()
})

const translateDevice = (device) => {
  switch (device) {
    case 'Desktop PC': return t('Desktop PC', 'Máy tính để bàn')
    case 'Android Device': return t('Android Device', 'Thiết bị Android')
    case 'Mobile Device': return t('Mobile Device', 'Thiết bị di động')
    case 'Windows PC': return t('Windows PC', 'Máy tính Windows')
    case 'Linux PC': return t('Linux PC', 'Máy tính Linux')
    case 'Unknown Device': return t('Unknown Device', 'Thiết bị không xác định')
    default: return device
  }
}

const translateBrowser = (browser) => {
  if (browser === 'Unknown Browser') return t('Unknown Browser', 'Trình duyệt không xác định')
  return browser
}

const addPasskey = () => {
  ElMessage.warning(t('Passkey enrollment is not supported by the server yet. [FLOW REQUIRED]', 'Đăng ký Passkey chưa được hỗ trợ bởi máy chủ. [CẦN FLOW]'))
}

const onPasswordInput = (val) => {
  props.passwordForm.newPassword = val
  emit('check-password-strength')
}

const toggleMfa = (val) => {
  emit('toggle-2fa', val)
}

const logoutSession = async (id) => {
  try {
    await axiosClient.delete(`/users/sessions/${id}`)
    activeSessions.value = activeSessions.value.filter(s => s.id !== id)
    ElMessage.success(t('Session terminated.', 'Đã đăng xuất phiên đăng nhập.'))
  } catch (error) {
    ElMessage.error(t('Failed to terminate session.', 'Không thể đăng xuất phiên đăng nhập.'))
  }
}

const logoutAllOthers = () => {
  ElMessageBox.confirm(
    t('Are you sure you want to log out from all other devices?', 'Bạn có chắc chắn muốn đăng xuất khỏi tất cả các thiết bị khác không?'),
    t('Log out other devices', 'Đăng xuất thiết bị khác'),
    {
      confirmButtonText: t('Log out', 'Đăng xuất'),
      cancelButtonText: t('Cancel', 'Hủy'),
      type: 'warning'
    }
  ).then(async () => {
    try {
      await axiosClient.delete('/users/sessions/others')
      activeSessions.value = activeSessions.value.filter(s => s.isCurrent)
      ElMessage.success(t('Successfully logged out of all other devices.', 'Đã đăng xuất thành công khỏi tất cả các thiết bị khác.'))
    } catch (error) {
      ElMessage.error(t('Failed to log out of other devices.', 'Không thể đăng xuất khỏi các thiết bị khác.'))
    }
  }).catch(() => {})
}
</script>

<style scoped>
.security-card {
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px !important;
  padding: 32px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.card-header {
  margin-bottom: 24px;
}

.card-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: 8px;
}

.card-subtitle {
  font-size: 14px;
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
}

.security-sections-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.security-row-item {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 16px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  background-color: var(--color-surface);
}

.row-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.row-icon {
  font-size: 20px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.color-blue { background-color: rgba(9, 30, 66, 0.04); color: #0052cc; }
.color-green { background-color: rgba(16, 185, 129, 0.1); color: #10b981; }
.color-purple { background-color: rgba(147, 51, 234, 0.1); color: #a855f7; }
.color-orange { background-color: rgba(249, 115, 22, 0.1); color: #f97316; }

.row-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.item-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.item-desc {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.4;
}

.section-divider {
  border: none;
  border-top: 1px solid var(--color-border);
  margin: 32px 0;
}

.recent-activity-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.section-desc {
  font-size: 14px;
  color: var(--color-text-secondary);
  margin: 0;
}

.activity-table-wrapper {
  overflow-x: auto;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  margin-top: 8px;
}

.activity-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.activity-table th, .activity-table td {
  padding: 12px 16px;
  font-size: 13px;
  border-bottom: 1px solid var(--color-border);
}

.activity-table th {
  background-color: var(--color-surface-hover);
  color: var(--color-text-secondary);
  font-weight: 600;
}

.activity-table td {
  color: var(--color-text-primary);
}

.activity-table tr:last-child td {
  border-bottom: none;
}

.activity-table td i {
  margin-right: 6px;
  color: var(--color-text-secondary);
}

/* Password step view styles */
.password-step-box {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.step-indicator {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--color-border);
}

.step-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-muted);
}

.step-label.active {
  color: #0052cc;
  font-weight: 600;
}

.step-label.done {
  color: #10b981;
}

.step-arrow {
  color: var(--color-text-muted);
  font-size: 12px;
}

.step-desc {
  font-size: 14px;
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
}

.cooldown-alert {
  display: flex;
  gap: 16px;
  padding: 16px;
  background-color: var(--color-danger-bg);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 8px;
  color: var(--color-danger);
}

.cooldown-alert i {
  font-size: 20px;
}

.cooldown-alert p {
  font-size: 13px;
  margin-top: 4px;
  color: var(--color-text-primary);
}

.atlassian-field-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.otp-hint-block {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.otp-hint-block a {
  color: #0052cc;
  text-decoration: none;
  font-weight: 600;
}

.step-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

.step-actions.between {
  justify-content: space-between;
}

.password-strength-meter {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 10px;
}

.strength-bars {
  display: flex;
  gap: 4px;
  width: 160px;
}

.strength-bars .bar {
  flex: 1;
  height: 5px;
  background-color: var(--color-border);
  border-radius: 3px;
}

.bar.strength-weak { background-color: #ef4444; }
.bar.strength-fair { background-color: #f59e0b; }
.bar.strength-good { background-color: #3b82f6; }
.bar.strength-strong { background-color: #10b981; }

.strength-label {
  font-size: 12px;
  font-weight: 600;
}

.strength-label.strength-weak { color: #ef4444; }
.strength-label.strength-fair { color: #f59e0b; }
.strength-label.strength-good { color: #3b82f6; }
.strength-label.strength-strong { color: #10b981; }

.password-checklist {
  list-style: none;
  padding: 0;
  margin: 12px 0 0;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.password-checklist li {
  font-size: 13px;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  gap: 6px;
}

.password-checklist li.passed {
  color: #10b981;
}

.error-text {
  color: #ef4444;
  font-size: 12px;
  margin-top: 4px;
}

.success-box {
  text-align: center;
  padding: 24px 0;
}

.success-icon-wrapper {
  width: 72px;
  height: 72px;
  background-color: rgba(16, 185, 129, 0.1);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  color: #10b981;
  font-size: 36px;
}

.success-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: 8px;
}

.success-desc {
  font-size: 14px;
  color: var(--color-text-secondary);
  max-width: 400px;
  margin: 0 auto 24px;
  line-height: 1.5;
}

/* MFA View styles */
.mfa-body {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.mfa-status-card {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: var(--color-surface);
}

.mfa-status-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.mfa-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  font-size: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mfa-icon.active {
  background-color: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.mfa-status-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 4px;
}

.mfa-status-desc {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.4;
  max-width: 480px;
}

.mfa-setup-instructions {
  background-color: var(--color-surface-hover);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--color-border);
}

.instruction-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: 12px;
}

.instruction-list {
  padding-left: 20px;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.instruction-list li {
  font-size: 13.5px;
  color: var(--color-text-secondary);
  line-height: 1.5;
}

/* Sessions styles */
.sessions-body {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sessions-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.session-item-card {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  position: relative;
  background-color: var(--color-surface);
}

.session-item-card.is-current {
  border-left: 4px solid #10b981;
}

.session-icon {
  font-size: 24px;
  color: var(--color-text-secondary);
  width: 32px;
  display: flex;
  justify-content: center;
}

.session-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.session-header-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.session-device-name {
  font-size: 14.5px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.session-meta {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  margin: 0;
}

.session-location {
  font-size: 12px;
  color: var(--color-text-muted);
  margin: 0;
}

.session-location i {
  margin-right: 4px;
}

.sessions-global-action {
  margin-top: 8px;
}

.mr-2 {
  margin-right: 8px;
}

:deep(.el-input__wrapper) {
  padding: 8px 12px !important;
  height: 40px !important;
  border-radius: 6px !important;
  background-color: var(--color-input-bg) !important;
  border: 1px solid var(--color-input-border) !important;
  box-shadow: none !important;
}

:deep(.el-input__inner) {
  border: none !important;
  background: transparent !important;
  padding: 0 !important;
  height: auto !important;
}

@media (max-width: 640px) {
  .security-row-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
  
  .security-row-item .el-button {
    align-self: flex-end;
  }

  .session-item-card {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .session-action {
    align-self: flex-end;
  }
}
</style>
