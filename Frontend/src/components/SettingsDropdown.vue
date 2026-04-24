<template>
  <el-dropdown v-if="canAccessAdmin || canAccessUserDirectory" trigger="click" popper-class="settings-dropdown-popper">
    <div class="settings-nav-icon setting-trigger">
      <i class="fa-solid fa-gear"></i>
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-settings-menu">
        <div class="settings-content-wrapper">
          <div class="settings-list">
            <div v-if="canAccessAdmin" class="settings-menu-item" @click="handleCommand('/admin/audit-log')">
              <span class="item-text">Audit Log</span>
            </div>

            <div v-if="canAccessUserDirectory" class="settings-menu-item" @click="handleCommand('/admin/users')">
              <span class="item-text">User Management</span>
            </div>

            <div v-if="canAccessUserDirectory" class="settings-menu-item" @click="handleCommand('/admin/roles')">
              <span class="item-text">Role Management</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-header">
              <span class="item-text">Organization</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item indented" @click="handleCommand('/admin/organization/profile')">
              <span class="item-text">Profile</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item indented" @click="handleCommand('/admin/organization/contact')">
              <span class="item-text">Contact</span>
            </div>

            <div v-if="canAccessAdmin" class="menu-divider"></div>

            <div v-if="canAccessAdmin" class="settings-menu-item" @click="handleCommand('/admin/configuration')">
              <span class="item-text">Configuration</span>
            </div>

            <div v-if="false" class="settings-menu-header">
              <span class="item-text">Instance</span>
            </div>

            <div v-if="false" class="settings-menu-item indented" @click="handleCommand('/admin/instance/general')">
              <span class="item-text">General settings</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-header">
              <span class="item-text">Security</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item indented" @click="handleCommand('/admin/instance/authentication')">
              <span class="item-text">Authentication</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item indented" @click="handleCommand('/admin/instance/email')">
              <span class="item-text">Email / SMTP</span>
            </div>

            <div v-if="canAccessAdmin" class="settings-menu-item" @click="handleCommand('/admin/customization')">
              <span class="item-text">Customization</span>
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

const router = useRouter()
const currentUser = computed(() => getStoredUser())
const canAccessAdmin = computed(() => hasSystemAdminAccess(currentUser.value))
const canAccessUserDirectory = computed(() => canAccessAdminUserDirectory(currentUser.value))

const handleCommand = (path) => {
  const canOpenPath = canAccessAdmin.value || (['/admin/users', '/admin/roles'].includes(path) && canAccessUserDirectory.value)
  if (!canOpenPath) {
    ElMessage.warning('You do not have permission to access admin settings.')
    return
  }

  const routeData = router.resolve({ path })
  openNamedAppWindow(routeData.href, PROJECT_ADMIN_WINDOW_NAME)
}
</script>

<style scoped>
.jira-settings-menu {
  width: 240px !important;
  background-color: #ffffff !important;
  border: 1px solid #e2e8f0 !important;
  border-radius: 8px !important;
  padding: 8px 0 !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1) !important;
}

.settings-list {
  display: flex;
  flex-direction: column;
}

.settings-menu-item {
  padding: 10px 20px;
  cursor: pointer;
  color: #1e293b;
  font-size: 14px;
  font-weight: 400;
}

.settings-menu-item:hover {
  background-color: #f1f5f9;
}

.settings-menu-header {
  padding: 10px 20px 4px 20px;
  color: var(--color-text-muted);
  font-size: 14px;
  font-weight: 400;
}

.settings-menu-item.indented {
  padding-left: 36px;
}

.menu-divider {
  height: 1px;
  background-color: #f1f5f9;
  margin: 6px 0;
}

.item-text {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, "Fira Sans", "Droid Sans", "Helvetica Neue", sans-serif;
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

<style>
.el-popper.settings-dropdown-popper {
  background: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  padding: 0 !important;
  z-index: 100001 !important;
  box-shadow: 0 10px 40px rgba(0,0,0,0.2) !important;
}

.el-popper.settings-dropdown-popper .el-popper__arrow::before {
  background: var(--color-surface) !important;
  border-color: var(--color-border) !important;
}
</style>



