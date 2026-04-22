<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-lock"></i>
          <span>Admin / Instance / Authentication</span>
        </div>
        <h1 class="page-title">Authentication management</h1>
        <p class="page-subtitle">
          Control which sign-in methods are exposed at the instance level and keep provider settings organized in one
          place. This scaffold stores state locally until dedicated backend config endpoints are introduced.
        </p>
      </div>

      <div class="layout-grid">
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Login methods</h2>
              <p>Toggle providers without rewriting the admin shell later.</p>
            </div>
            <button type="button" class="primary-btn" @click="saveSettings">Save</button>
          </div>

          <div class="toggle-list">
            <label class="toggle-row">
              <input v-model="settings.emailPassword" type="checkbox" />
              <span>Email and password</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.loginOtp" type="checkbox" />
              <span>Email OTP login</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.github.enabled" type="checkbox" />
              <span>GitHub OAuth</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.google.enabled" type="checkbox" />
              <span>Google OAuth</span>
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>GitHub provider</h2>
              <p>Client values can be filled now and wired to secure storage later.</p>
            </div>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>Client ID</span>
              <input v-model="settings.github.clientId" type="text" placeholder="github-client-id" />
            </label>
            <label class="field">
              <span>Callback URL</span>
              <input v-model="settings.github.callbackUrl" type="text" placeholder="https://app.example.com/auth/github/callback" />
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Google provider</h2>
              <p>Useful for later Plane-style authentication administration.</p>
            </div>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>Client ID</span>
              <input v-model="settings.google.clientId" type="text" placeholder="google-client-id" />
            </label>
            <label class="field">
              <span>Callback URL</span>
              <input v-model="settings.google.callbackUrl" type="text" placeholder="https://app.example.com/auth/google/callback" />
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Policy</h2>
              <p>High-level controls for login safety and tenant onboarding.</p>
            </div>
          </div>

          <div class="toggle-list">
            <label class="toggle-row">
              <input v-model="settings.requireEmailVerification" type="checkbox" />
              <span>Require verified email before first workspace access</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.allowInvitationOnly" type="checkbox" />
              <span>Invitation-only onboarding</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.enforceSsoForAdmins" type="checkbox" />
              <span>Force OAuth or OTP for admin users</span>
            </label>
          </div>
        </section>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import AdminLayout from '@/components/layout/AdminLayout.vue'

const STORAGE_KEY = 'admin-instance-auth-settings'

const settings = ref({
  emailPassword: true,
  loginOtp: true,
  requireEmailVerification: true,
  allowInvitationOnly: false,
  enforceSsoForAdmins: false,
  github: {
    enabled: true,
    clientId: '',
    callbackUrl: ''
  },
  google: {
    enabled: false,
    clientId: '',
    callbackUrl: ''
  }
})

const loadSettings = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return
    const parsed = JSON.parse(raw)
    settings.value = {
      ...settings.value,
      ...parsed,
      github: { ...settings.value.github, ...(parsed.github || {}) },
      google: { ...settings.value.google, ...(parsed.google || {}) }
    }
  } catch {
    ElMessage.warning('Could not restore saved authentication settings.')
  }
}

const saveSettings = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settings.value))
  ElMessage.success('Authentication settings saved locally.')
}

onMounted(loadSettings)
</script>

<style scoped>
.admin-page {
  padding: 32px;
  color: #e5e7eb;
}

.page-header {
  margin-bottom: 24px;
}

.breadcrumb {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #94a3b8;
  font-size: 13px;
  margin-bottom: 8px;
}

.page-title {
  margin: 0;
  font-size: 28px;
}

.page-subtitle {
  margin: 8px 0 0;
  max-width: 820px;
  color: #94a3b8;
  line-height: 1.6;
}

.layout-grid {
  display: grid;
  gap: 16px;
}

.settings-card {
  border: 1px solid #1f2937;
  border-radius: 16px;
  background: #0f172a;
  padding: 20px;
}

.section-head {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-start;
  margin-bottom: 18px;
}

.section-head h2 {
  margin: 0 0 6px;
  font-size: 18px;
}

.section-head p {
  margin: 0;
  color: #94a3b8;
  line-height: 1.5;
}

.toggle-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #e2e8f0;
}

.field-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.field span {
  color: #cbd5e1;
  font-size: 13px;
  font-weight: 600;
}

.field input {
  border: 1px solid #334155;
  border-radius: 10px;
  background: #020617;
  color: #e5e7eb;
  padding: 12px 14px;
  font: inherit;
}

.primary-btn {
  border: none;
  border-radius: 999px;
  background: #38bdf8;
  color: #082f49;
  padding: 10px 16px;
  font-weight: 700;
  cursor: pointer;
}

@media (max-width: 900px) {
  .admin-page {
    padding: 20px;
  }

  .field-grid {
    grid-template-columns: 1fr;
  }
}
</style>
