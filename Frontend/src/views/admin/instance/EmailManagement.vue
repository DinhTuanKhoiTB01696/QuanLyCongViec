<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-envelope"></i>
          <span>Admin / Instance / Email</span>
        </div>
        <h1 class="page-title">Email and SMTP management</h1>
        <p class="page-subtitle">
          Keep sender identity, SMTP transport, and test-delivery controls in a dedicated admin module. This scaffold
          stores settings locally for now, but the route and form structure are ready for real APIs.
        </p>
      </div>

      <div class="settings-grid">
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Sender identity</h2>
              <p>Values used in invitation emails, OTP messages, and operational notices.</p>
            </div>
            <button type="button" class="primary-btn" @click="saveSettings">Save</button>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>From name</span>
              <input v-model="settings.fromName" type="text" placeholder="SprintA Notifications" />
            </label>
            <label class="field">
              <span>From email</span>
              <input v-model="settings.fromEmail" type="email" placeholder="no-reply@example.com" />
            </label>
            <label class="field full">
              <span>Reply-to email</span>
              <input v-model="settings.replyToEmail" type="email" placeholder="support@example.com" />
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>SMTP transport</h2>
              <p>Connection settings for outbound mail delivery.</p>
            </div>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>SMTP host</span>
              <input v-model="settings.smtpHost" type="text" placeholder="smtp.example.com" />
            </label>
            <label class="field">
              <span>Port</span>
              <input v-model="settings.smtpPort" type="number" min="1" placeholder="587" />
            </label>
            <label class="field">
              <span>Username</span>
              <input v-model="settings.smtpUsername" type="text" placeholder="smtp-user" />
            </label>
            <label class="field">
              <span>Password placeholder</span>
              <input v-model="settings.smtpSecretLabel" type="text" placeholder="smtp-secret-ref" />
            </label>
          </div>

          <div class="toggle-list">
            <label class="toggle-row">
              <input v-model="settings.useTls" type="checkbox" />
              <span>Use TLS</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.useStartTls" type="checkbox" />
              <span>Use STARTTLS</span>
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Test delivery</h2>
              <p>Quick smoke check for the scaffolded mail configuration.</p>
            </div>
            <button type="button" class="neutral-btn" @click="runTest">Run test</button>
          </div>

          <div class="field-grid">
            <label class="field full">
              <span>Recipient email</span>
              <input v-model="settings.testRecipient" type="email" placeholder="qa@example.com" />
            </label>
          </div>

          <div class="status-banner">
            <strong>Stored locally</strong>
            <span>Use this page as the admin shell now; replace local persistence with secure backend settings later.</span>
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

const STORAGE_KEY = 'admin-instance-email-settings'

const settings = ref({
  fromName: 'SprintA Notifications',
  fromEmail: '',
  replyToEmail: '',
  smtpHost: '',
  smtpPort: 587,
  smtpUsername: '',
  smtpSecretLabel: '',
  useTls: true,
  useStartTls: true,
  testRecipient: ''
})

const loadSettings = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return
    settings.value = {
      ...settings.value,
      ...JSON.parse(raw)
    }
  } catch {
    ElMessage.warning('Could not restore saved email settings.')
  }
}

const saveSettings = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settings.value))
  ElMessage.success('Email settings saved locally.')
}

const runTest = () => {
  if (!settings.value.smtpHost || !settings.value.fromEmail || !settings.value.testRecipient) {
    ElMessage.warning('Fill SMTP host, sender email, and test recipient first.')
    return
  }

  ElMessage.success(`SMTP test queued for ${settings.value.testRecipient} (scaffold mode).`)
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

.settings-grid {
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

.field.full {
  grid-column: 1 / -1;
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

.toggle-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 16px;
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #e2e8f0;
}

.status-banner {
  margin-top: 18px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  border: 1px solid #334155;
  border-radius: 12px;
  background: #111827;
  padding: 14px 16px;
  color: #cbd5e1;
}

.primary-btn,
.neutral-btn {
  border-radius: 999px;
  padding: 10px 16px;
  font-weight: 700;
  cursor: pointer;
}

.primary-btn {
  border: none;
  background: #38bdf8;
  color: #082f49;
}

.neutral-btn {
  border: 1px solid #334155;
  background: #111827;
  color: #e2e8f0;
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
