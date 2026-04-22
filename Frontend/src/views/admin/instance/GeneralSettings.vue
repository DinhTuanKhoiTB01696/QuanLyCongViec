<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-server"></i>
          <span>Admin / Instance / General</span>
        </div>
        <h1 class="page-title">Instance general settings</h1>
        <p class="page-subtitle">
          Central place for system-wide identity, admin contact rules, and rollout defaults. This page is scaffolded
          locally so we can keep the admin shell aligned with the Plane-style direction while backend APIs are filled in.
        </p>
      </div>

      <div class="settings-grid">
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Identity</h2>
              <p>Branding values used across instance-level pages and emails.</p>
            </div>
            <button type="button" class="primary-btn" @click="saveSettings">Save</button>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>Instance name</span>
              <input v-model="settings.instanceName" type="text" placeholder="SprintA" />
            </label>
            <label class="field">
              <span>Primary domain</span>
              <input v-model="settings.primaryDomain" type="text" placeholder="app.example.com" />
            </label>
            <label class="field">
              <span>Support URL</span>
              <input v-model="settings.supportUrl" type="text" placeholder="https://status.example.com" />
            </label>
            <label class="field">
              <span>Default timezone</span>
              <input v-model="settings.defaultTimezone" type="text" placeholder="Asia/Saigon" />
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Admin contacts</h2>
              <p>Fallback contacts shown in system banners, invitations, and escalation flows.</p>
            </div>
          </div>

          <div class="field-grid">
            <label class="field">
              <span>Operations email</span>
              <input v-model="settings.operationsEmail" type="email" placeholder="ops@example.com" />
            </label>
            <label class="field">
              <span>Security email</span>
              <input v-model="settings.securityEmail" type="email" placeholder="security@example.com" />
            </label>
            <label class="field full">
              <span>Incident banner</span>
              <textarea v-model="settings.incidentBanner" rows="4" placeholder="Scheduled maintenance notifications or global alerts"></textarea>
            </label>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>Rollout defaults</h2>
              <p>Safe defaults for onboarding and self-service access.</p>
            </div>
          </div>

          <div class="toggle-list">
            <label class="toggle-row">
              <input v-model="settings.allowWorkspaceSignup" type="checkbox" />
              <span>Allow self-service workspace signup</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.requireInviteApproval" type="checkbox" />
              <span>Require admin approval for org invites</span>
            </label>
            <label class="toggle-row">
              <input v-model="settings.showPublicStatusPage" type="checkbox" />
              <span>Expose public status page link</span>
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

const STORAGE_KEY = 'admin-instance-general-settings'

const settings = ref({
  instanceName: 'SprintA',
  primaryDomain: 'localhost',
  supportUrl: '',
  defaultTimezone: 'Asia/Saigon',
  operationsEmail: '',
  securityEmail: '',
  incidentBanner: '',
  allowWorkspaceSignup: false,
  requireInviteApproval: true,
  showPublicStatusPage: false
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
    ElMessage.warning('Could not restore saved instance settings.')
  }
}

const saveSettings = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settings.value))
  ElMessage.success('Instance general settings saved locally.')
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

.field input,
.field textarea {
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
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #e2e8f0;
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
