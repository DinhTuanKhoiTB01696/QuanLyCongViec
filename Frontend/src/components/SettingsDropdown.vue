<template>
  <el-dropdown v-if="canAccessAdmin" trigger="click" popper-class="settings-dropdown-popper">
    <div class="settings-nav-icon setting-trigger">
      <i class="fa-solid fa-gear"></i>
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-settings-menu">
        <div class="settings-content-wrapper">
          <div class="settings-list">
            <div class="settings-menu-item" @click="handleCommand('/admin/audit-log')">
              <span class="item-text">Audit Log</span>
            </div>

            <div class="settings-menu-item" @click="handleCommand('/admin/users')">
              <span class="item-text">User Management</span>
            </div>

            <div class="settings-menu-header">
              <span class="item-text">Organization</span>
            </div>
            
            <div class="settings-menu-item indented" @click="handleCommand('/admin/organization/profile')">
              <span class="item-text">Profile</span>
            </div>
            
            <div class="settings-menu-item indented" @click="handleCommand('/admin/organization/contact')">
              <span class="item-text">Contact</span>
            </div>

            <div class="menu-divider"></div>

            <div class="settings-menu-item" @click="handleCommand('/admin/configuration')">
              <span class="item-text">Configuration</span>
            </div>

            <div class="settings-menu-item" @click="handleCommand('/admin/customization')">
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

const router = useRouter()

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')

// Chỉ System Admin, PM, PO mới thấy nút Settings (Admin)
const canAccessAdmin = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('System Admin') || roles.includes('PM') || roles.includes('PO')
})

const handleCommand = (path) => {
  if (!canAccessAdmin.value) {
    ElMessage.warning('Bạn không có quyền truy cập trang quản trị.')
    return
  }
  // Use a named target so the browser reuses the same tab for all Admin links
  const routeData = router.resolve({ path })
  window.open(routeData.href, 'NexusAdminWindow')
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
  color: #64748b;
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
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.settings-nav-icon i {
  font-size: 18px;
}

.settings-nav-icon:hover {
  background-color: var(--hover-bg);
  color: var(--text-primary);
}
</style>

<style>
.el-popper.settings-dropdown-popper {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-color) !important;
  padding: 0 !important;
  z-index: 100001 !important;
  box-shadow: 0 10px 40px rgba(0,0,0,0.2) !important;
}

.el-popper.settings-dropdown-popper .el-popper__arrow::before {
  background: var(--bg-card) !important;
  border-color: var(--border-color) !important;
}
</style>
