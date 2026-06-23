<template>
  <el-dropdown trigger="click" popper-class="settings-dropdown-popper">
    <div class="settings-nav-icon setting-trigger">
      <i class="fa-solid fa-gear"></i>
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-settings-menu">
        <div class="settings-content-wrapper">
          <div class="settings-list">
            <!-- Personal Settings -->
            <div class="settings-section-header">{{ t('Personal Jira settings') }}</div>
            <div class="settings-menu-item" @click="handleCommand('/profile')">
              <i class="fa-regular fa-user"></i>
              <div class="item-info">
                <span class="item-title">{{ t('General settings') }}</span>
                <span class="item-desc">{{ t('Manage language, time zone, and other personal preferences') }}</span>
              </div>
            </div>

            <div class="settings-menu-item" @click="handleCommand('/your-work')">
              <i class="fa-regular fa-bell"></i>
              <div class="item-info">
                <span class="item-title">{{ t('Notification settings') }}</span>
                <span class="item-desc">{{ t('Manage email and in-app notifications from Jira') }}</span>
              </div>
            </div>

            <div class="menu-divider"></div>

            <!-- Jira Admin Settings -->
            <div class="settings-section-header">{{ t('Jira admin settings') }}</div>

            <div v-if="canAccessAdmin" class="settings-menu-item" @click="handleCommand('/admin/audit-log')">
              <i class="fa-solid fa-desktop"></i>
              <div class="item-info">
                <span class="item-title">{{ t('System') }}</span>
                <span class="item-desc">{{ t('Manage general configuration, security, audit logs, and more') }}</span>
              </div>
            </div>

            <div v-if="canAccessUserDirectory" class="settings-menu-item" @click="handleCommand('/admin/users')">
              <i class="fa-solid fa-users"></i>
              <div class="item-info">
                <span class="item-title">{{ t('Jira apps') }}</span>
                <span class="item-desc">{{ t('Manage access, settings, and integrations across Jira') }}</span>
              </div>
            </div>

            <div class="settings-menu-item" @click="handleCommand('/spaces')">
              <i class="fa-solid fa-rocket"></i>
              <div class="item-info">
                <span class="item-title">{{ t('Spaces') }}</span>
                <span class="item-desc">{{ t('Manage space settings, categories, and more') }}</span>
              </div>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item" @click="handleCommand('/admin/configuration')">
              <i class="fa-regular fa-folder-open"></i>
              <div class="item-info">
                <span class="item-title">{{ t('Work items') }}</span>
                <span class="item-desc">{{ t('Configure work types, workflows, screens, fields, and more') }}</span>
              </div>
            </div>
          </div>
        </div>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { canAccessAdminUserDirectory, getStoredUser, hasSystemAdminAccess } from '@/utils/permissions'
import { openNamedAppWindow, PROJECT_ADMIN_WINDOW_NAME } from '@/utils/windowTabs'
import { useI18nStore } from '@/store/useI18nStore'

const router = useRouter()
const i18nStore = useI18nStore()
const t = i18nStore.t

const currentUser = computed(() => getStoredUser())
const canAccessAdmin = computed(() => hasSystemAdminAccess(currentUser.value))
const canAccessUserDirectory = computed(() => canAccessAdminUserDirectory(currentUser.value))

const handleCommand = (path) => {
  const canOpenPath = !path.startsWith('/admin') || canAccessAdmin.value || (['/admin/users', '/admin/roles'].includes(path) && canAccessUserDirectory.value)
  if (!canOpenPath) {
    ElMessage.warning(t('You do not have permission to access admin settings.'))
    return
  }

  router.push(path)
}
</script>

<style scoped>
.jira-settings-menu {
  width: 320px !important;
  background-color: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 8px !important;
  padding: 12px 0 !important;
  box-shadow: 0 10px 40px rgba(0,0,0,0.3) !important;
}

.settings-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.settings-section-header {
  padding: 8px 16px 4px 16px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.settings-menu-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 8px 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.settings-menu-item:hover {
  background-color: var(--color-surface-hover);
}

.settings-menu-item i {
  font-size: 16px;
  color: var(--color-text-muted);
  margin-top: 2px;
}

.settings-menu-item.indented {
  padding-left: 44px;
}

.item-info {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.item-title {
  color: var(--color-text-primary);
  font-size: 13.5px;
  font-weight: 500;
}

.item-desc {
  color: var(--color-text-muted);
  font-size: 11px;
  line-height: 1.4;
  margin-top: 2px;
}

.sub-links {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.sub-link {
  color: var(--color-accent);
  font-size: 11.5px;
  font-weight: 600;
  cursor: pointer;
}

.sub-link:hover {
  text-decoration: underline;
}

.menu-divider {
  height: 1px;
  background-color: var(--color-border);
  margin: 8px 0;
}

.settings-nav-icon {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.settings-nav-icon i {
  font-size: 18px;
}

.settings-nav-icon:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}
</style>
