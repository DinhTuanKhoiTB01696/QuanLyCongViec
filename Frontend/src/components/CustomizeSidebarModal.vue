<template>
  <el-dialog
    v-model="dialogVisible"
    title="Customize your sidebar"
    width="560px"
    class="customize-sidebar-dialog"
    :show-close="true"
    :before-close="handleClose"
  >
    <div class="customize-sidebar-content">
      <p class="desc-text">
        Selected items will always be visible in the sidebar. You can still access unselected items from the <strong>More</strong> menu in the sidebar.
      </p>
      <p class="desc-text mt-2">
        The changes you make here only affect you and not anyone else on your site.
      </p>

      <div class="section-container mt-6">
        <h3 class="section-title">SprintA navigation</h3>
        <p class="section-desc">The following navigation items are available in SprintA.</p>

        <div class="nav-list">
          <!-- Disabled / Fixed item -->
          <div class="nav-item fixed-item">
            <div class="drag-handle invisible"><i class="fa-solid fa-grip-vertical"></i></div>
            <el-checkbox model-value="true" disabled class="custom-checkbox"></el-checkbox>
            <div class="item-icon-box"><i class="fa-solid fa-border-all"></i></div>
            <span class="item-label">Dành cho bạn</span>
          </div>

          <!-- Draggable items -->
          <div class="nav-item" v-for="item in navItems" :key="item.id">
            <div class="drag-handle"><i class="fa-solid fa-grip-vertical"></i></div>
            <el-checkbox v-model="item.checked" class="custom-checkbox"></el-checkbox>
            <div class="item-icon-box"><i :class="item.icon"></i></div>
            <span class="item-label">{{ item.label }}</span>
            <div class="more-actions"><i class="fa-solid fa-ellipsis"></i></div>
          </div>
        </div>
      </div>

      <div class="section-container mt-6">
        <h3 class="section-title">App shortcuts</h3>
        <p class="section-desc">The following apps are available for your organization.</p>

        <div class="nav-list">
          <div class="nav-item" v-for="item in appItems" :key="item.id">
            <div class="drag-handle"><i class="fa-solid fa-grip-vertical"></i></div>
            <el-checkbox v-model="item.checked" class="custom-checkbox"></el-checkbox>
            <div class="item-icon-box"><i :class="item.icon"></i></div>
            <span class="item-label">{{ item.label }}</span>
            <div class="more-actions"><i class="fa-solid fa-ellipsis"></i></div>
          </div>
        </div>
      </div>
    </div>
    
    <template #footer>
      <div class="dialog-footer-actions">
        <el-button @click="handleClose" class="btn-cancel">Cancel</el-button>
        <el-button type="primary" @click="saveChanges" class="btn-save">Save changes</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'saved'])

const dialogVisible = ref(props.visible)

watch(() => props.visible, (newVal) => {
  dialogVisible.value = newVal
})

const navItems = ref([
  { id: 'recent', label: 'Recent', icon: 'fa-regular fa-clock', checked: true },
  { id: 'spaces', label: 'Spaces', icon: 'fa-regular fa-folder-open', checked: true },
  { id: 'ai', label: 'Trợ lý AI', icon: 'fa-solid fa-robot', checked: true },
  { id: 'audit', label: 'Audit Log', icon: 'fa-solid fa-list-check', checked: true },
  { id: 'users', label: 'Quản lý người dùng', icon: 'fa-solid fa-users-gear', checked: true },
])

onMounted(() => {
  const saved = localStorage.getItem('sidebarPreferences')
  if (saved) {
    try {
      const parsed = JSON.parse(saved)
      navItems.value.forEach(item => {
        if (parsed[item.id] !== undefined) {
          item.checked = parsed[item.id]
        }
      })
    } catch (e) {
      console.error(e)
    }
  }
})

watch(() => props.visible, (newVal) => {
  dialogVisible.value = newVal
  if (newVal) {
    const saved = localStorage.getItem('sidebarPreferences')
    if (saved) {
      try {
        const parsed = JSON.parse(saved)
        navItems.value.forEach(item => {
          if (parsed[item.id] !== undefined) {
            item.checked = parsed[item.id]
          }
        })
      } catch (e) {}
    }
  }
})

const appItems = ref([
  { id: 'goals', label: 'Goals', icon: 'fa-solid fa-bullseye', checked: true },
  { id: 'teams', label: 'Teams', icon: 'fa-solid fa-users', checked: true },
  { id: 'projects', label: 'Projects', icon: 'fa-solid fa-rocket', checked: false },
])

const handleClose = () => {
  emit('update:visible', false)
}

const saveChanges = () => {
  // In a real app, save to user preferences
  emit('saved', { navItems: navItems.value, appItems: appItems.value })
  emit('update:visible', false)
}
</script>

<style scoped>
.customize-sidebar-content {
  color: #c1c7d0;
  font-size: 14px;
  line-height: 1.5;
  padding-bottom: 10px;
}

.desc-text {
  margin: 0;
  color: #c1c7d0;
  font-size: 14px;
}
.mt-2 { margin-top: 8px; }
.mt-6 { margin-top: 24px; }

.section-container {
  display: flex;
  flex-direction: column;
}

.section-title {
  color: #f4f5f7;
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 4px 0;
}

.section-desc {
  color: #c1c7d0;
  font-size: 13px;
  margin: 0 0 16px 0;
}

.nav-list {
  display: flex;
  flex-direction: column;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 6px 8px;
  border-radius: 4px;
  transition: background-color 0.1s;
}

.nav-item:hover {
  background-color: #2c333a; /* Darker hover like Jira */
}

.fixed-item {
  opacity: 0.7;
}
.fixed-item:hover {
  background-color: transparent;
}

.drag-handle {
  width: 24px;
  color: #8c9bab;
  font-size: 14px;
  cursor: grab;
  display: flex;
  align-items: center;
  justify-content: center;
  visibility: hidden; /* show on hover */
}
.nav-item:hover .drag-handle {
  visibility: visible;
}
.drag-handle.invisible {
  visibility: hidden !important;
}

:deep(.custom-checkbox .el-checkbox__inner) {
  background-color: #22272b;
  border-color: #738496;
}
:deep(.custom-checkbox.is-checked .el-checkbox__inner) {
  background-color: #579dff;
  border-color: #579dff;
}
:deep(.custom-checkbox .el-checkbox__inner::after) {
  border-color: #1d2125;
}

.item-icon-box {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  margin-left: 8px;
  margin-right: 12px;
  color: #8c9bab;
  font-size: 14px;
}

.item-label {
  color: #f4f5f7;
  font-size: 14px;
  flex: 1;
}

.more-actions {
  color: #8c9bab;
  font-size: 14px;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  cursor: pointer;
  visibility: hidden;
}
.nav-item:hover .more-actions {
  visibility: visible;
}
.more-actions:hover {
  background-color: #3b444b;
  color: #f4f5f7;
}

.dialog-footer-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel {
  background: transparent !important;
  border: none !important;
  color: #b3bac5 !important;
  font-weight: 500;
}
.btn-cancel:hover {
  color: #f4f5f7 !important;
  background-color: rgba(255, 255, 255, 0.08) !important;
}

.btn-save {
  background-color: #579dff !important;
  color: #1d2125 !important;
  border: none !important;
  font-weight: 600;
}
.btn-save:hover {
  background-color: #85b8ff !important;
}
</style>

<style>
/* Global overrides for this specific dialog class */
.customize-sidebar-dialog.el-dialog {
  background-color: #22272b !important;
  border: 1px solid #333c43 !important;
  border-radius: 8px !important;
  padding: 0 !important;
  box-shadow: 0 12px 48px rgba(0,0,0,0.6) !important;
}

.customize-sidebar-dialog .el-dialog__header {
  padding: 24px 24px 16px !important;
  margin-right: 0 !important;
}

.customize-sidebar-dialog .el-dialog__title {
  color: #f4f5f7 !important;
  font-size: 20px !important;
  font-weight: 500 !important;
}

.customize-sidebar-dialog .el-dialog__headerbtn {
  top: 24px !important;
  right: 24px !important;
  font-size: 20px !important;
}
.customize-sidebar-dialog .el-dialog__headerbtn .el-dialog__close {
  color: #94a3b8 !important;
}
.customize-sidebar-dialog .el-dialog__headerbtn:hover .el-dialog__close {
  color: #f4f5f7 !important;
}

.customize-sidebar-dialog .el-dialog__body {
  padding: 0 24px 24px !important;
}

.customize-sidebar-dialog .el-dialog__footer {
  padding: 16px 24px !important;
  border-top: none !important;
}

.customize-sidebar-dialog .el-checkbox__input.is-disabled .el-checkbox__inner {
  background-color: #3b444b !important;
  border-color: #3b444b !important;
}
.customize-sidebar-dialog .el-checkbox__input.is-disabled .el-checkbox__inner::after {
  border-color: #1d2125 !important;
}
</style>
