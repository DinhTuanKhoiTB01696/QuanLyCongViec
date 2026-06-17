<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-desktop"></i>
          <span>{{ t('System', 'System') }} / {{ t('General configuration', 'General configuration') }}</span>
        </div>
        <h1 class="text-hero">{{ t('General configuration', 'General configuration') }}</h1>
        <p class="text-desc">
          {{ t('Configure general options for this instance such as title, mode, base URL, and user preferences.', 'Configure general options for this instance such as title, mode, base URL, and user preferences.') }}
        </p>
      </div>

      <div class="settings-grid">
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="text-section">{{ t('Jira options', 'Jira options') }}</h2>
              <p class="text-small">{{ t('Global settings for the instance branding and identity.', 'Global settings for the instance branding and identity.') }}</p>
            </div>
            <button type="button" class="primary-btn" @click="saveSettings">
              <i class="fa-solid fa-floppy-disk" style="margin-right: 6px;"></i>{{ t('Save Changes', 'Save Changes') }}
            </button>
          </div>

          <div class="field-grid">
            <div class="field">
              <span class="field-label">{{ t('Title', 'Title') }}</span>
              <input v-model="settings.instanceName" type="text" placeholder="SprintA" />
              <span class="field-help">{{ t('The title of this application instance.', 'The title of this application instance.') }}</span>
            </div>

            <div class="field">
              <span class="field-label">{{ t('Mode', 'Mode') }}</span>
              <select v-model="settings.mode" class="jira-select">
                <option value="Public">Public</option>
                <option value="Private">Private</option>
              </select>
              <span class="field-help">{{ t('Public mode allows anyone to sign up, while Private mode requires invitation.', 'Public mode allows anyone to sign up, while Private mode requires invitation.') }}</span>
            </div>

            <div class="field">
              <span class="field-label">{{ t('Base URL', 'Base URL') }}</span>
              <input v-model="settings.primaryDomain" type="text" placeholder="http://localhost:5173" />
              <span class="field-help">{{ t('The root URL of this instance, used to build links in emails.', 'The root URL of this instance, used to build links in emails.') }}</span>
            </div>

            <div class="field">
              <span class="field-label">{{ t('Default language', 'Default language') }}</span>
              <select v-model="settings.defaultLanguage" class="jira-select">
                <option value="vi">Tiếng Việt (Vietnamese)</option>
                <option value="en">English (US)</option>
              </select>
              <span class="field-help">{{ t('Default language applied to new sessions.', 'Default language applied to new sessions.') }}</span>
            </div>

            <div class="field full">
              <span class="field-label">{{ t('Contact administrators message', 'Contact administrators message') }}</span>
              <textarea v-model="settings.contactAdminsMessage" rows="3" placeholder="Enter a message to show on the contact administrators form..."></textarea>
              <span class="field-help">{{ t('Message display to users on the Contact Administrators form.', 'Message display to users on the Contact Administrators form.') }}</span>
            </div>

            <div class="field">
              <span class="field-label">{{ t('Contact administrators form', 'Contact administrators form') }}</span>
              <select v-model="settings.contactAdminsForm" class="jira-select">
                <option value="ON">ON</option>
                <option value="OFF">OFF</option>
              </select>
              <span class="field-help">{{ t('Toggle visibility of the contact administrators link/form to logged out users.', 'Toggle visibility of the contact administrators link/form to logged out users.') }}</span>
            </div>

            <div class="field">
              <span class="field-label">{{ t('Support URL', 'Support URL') }}</span>
              <input v-model="settings.supportUrl" type="text" placeholder="https://status.example.com" />
              <span class="field-help">{{ t('The custom help/support URL for user queries.', 'The custom help/support URL for user queries.') }}</span>
            </div>
          </div>
        </section>

        <!-- System Options -->
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="text-section">{{ t('User permissions & options', 'User permissions & options') }}</h2>
              <p class="text-small">{{ t('Define default behavior for voting, watching, and invite rules.', 'Define default behavior for voting, watching, and invite rules.') }}</p>
            </div>
          </div>

          <div class="toggle-list">
            <label class="toggle-row">
              <input v-model="settings.allowWorkspaceSignup" type="checkbox" />
              <div class="toggle-content">
                <span class="toggle-title">{{ t('Allow self-service workspace signup', 'Allow self-service workspace signup') }}</span>
                <span class="toggle-desc">{{ t('Let anyone register an account and start creating project boards.', 'Let anyone register an account and start creating project boards.') }}</span>
              </div>
            </label>

            <label class="toggle-row">
              <input v-model="settings.requireInviteApproval" type="checkbox" />
              <div class="toggle-content">
                <span class="toggle-title">{{ t('Require invite approval', 'Require invite approval') }}</span>
                <span class="toggle-desc">{{ t('New user invites must be approved by a system administrator.', 'New user invites must be approved by a system administrator.') }}</span>
              </div>
            </label>

            <label class="toggle-row">
              <input v-model="settings.allowWatching" type="checkbox" />
              <div class="toggle-content">
                <span class="toggle-title">{{ t('Allow users to watch work items', 'Allow users to watch work items') }}</span>
                <span class="toggle-desc">{{ t('Users can subscribe to issue updates to receive notification pings.', 'Users can subscribe to issue updates to receive notification pings.') }}</span>
              </div>
            </label>

            <label class="toggle-row">
              <input v-model="settings.allowVoting" type="checkbox" />
              <div class="toggle-content">
                <span class="toggle-title">{{ t('Allow users to vote on work items', 'Allow users to vote on work items') }}</span>
                <span class="toggle-desc">{{ t('Enable the upvote system on work tasks to gauge community interest.', 'Enable the upvote system on work tasks to gauge community interest.') }}</span>
              </div>
            </label>

            <label class="toggle-row">
              <input v-model="settings.showPublicStatusPage" type="checkbox" />
              <div class="toggle-content">
                <span class="toggle-title">{{ t('Expose public status page link', 'Expose public status page link') }}</span>
                <span class="toggle-desc">{{ t('Display service status dashboard link in the layout footers.', 'Display service status dashboard link in the layout footers.') }}</span>
              </div>
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
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const STORAGE_KEY = 'admin-instance-general-settings'

const settings = ref({
  instanceName: 'SprintA',
  mode: 'Public',
  primaryDomain: 'localhost',
  supportUrl: '',
  defaultTimezone: 'Asia/Saigon',
  defaultLanguage: 'vi',
  contactAdminsMessage: 'Vui lòng liên hệ quản trị viên qua email nếu bạn cần hỗ trợ tài khoản.',
  contactAdminsForm: 'ON',
  allowWorkspaceSignup: false,
  requireInviteApproval: true,
  allowWatching: true,
  allowVoting: true,
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
    ElMessage.warning(t('Could not restore saved settings.', 'Could not restore saved settings.'))
  }
}

const saveSettings = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settings.value))
  ElMessage.success(t('General system configurations saved successfully.', 'General system configurations saved successfully.'))
}

onMounted(loadSettings)
</script>

<style scoped>
.breadcrumb {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 8px;
}

.jira-select {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  width: 100%;
  transition: border-color 0.2s;
}

.jira-select:focus {
  border-color: var(--color-accent);
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field.full {
  grid-column: 1 / -1;
}

.field-help {
  font-size: 11.5px;
  color: var(--color-text-muted);
}

.toggle-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-top: 16px;
}

.toggle-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  cursor: pointer;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid transparent;
  transition: all 0.2s ease;
}

.toggle-row:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
}

.toggle-row input[type="checkbox"] {
  width: 16px;
  height: 16px;
  margin-top: 2px;
  cursor: pointer;
}

.toggle-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.toggle-title {
  font-size: 13.5px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.toggle-desc {
  font-size: 11.5px;
  color: var(--color-text-muted);
  line-height: 1.4;
}

@media (max-width: 900px) {
  .field-grid {
    grid-template-columns: 1fr;
  }
}
</style>
