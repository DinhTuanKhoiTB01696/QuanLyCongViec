<template>
  <el-dropdown ref="dropdownRef" trigger="click" popper-class="help-dropdown-popper global-help-popper" @command="handleCommand">
    <button class="help-trigger-btn" type="button" aria-label="Open help menu">
      <i class="fa-solid fa-circle-question"></i>
    </button>
    <template #dropdown>
      <el-dropdown-menu class="dark-help-menu">
        <div class="help-header">
          <h3>Tro giup</h3>
          <button class="close-btn" type="button" @click="closeMenu" aria-label="Close help menu">
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>
        <el-dropdown-item command="updates"><i class="fa-solid fa-lightbulb"></i> Cap nhat moi</el-dropdown-item>
        <el-dropdown-item command="navigation"><i class="fa-solid fa-file-lines"></i> Dieu huong moi</el-dropdown-item>
        <el-dropdown-item command="docs"><i class="fa-solid fa-book"></i> Tai lieu huong dan</el-dropdown-item>
        <el-dropdown-item command="learn"><i class="fa-solid fa-desktop"></i> Hoc quan ly du an</el-dropdown-item>
        <el-dropdown-item command="community" divided><i class="fa-solid fa-comment"></i> Cong dong truc tuyen</el-dropdown-item>
        <el-dropdown-item command="support"><i class="fa-solid fa-triangle-exclamation"></i> Lien he ho tro</el-dropdown-item>
        <el-dropdown-item command="feedback"><i class="fa-solid fa-comments"></i> Phan hoi ve SprintA</el-dropdown-item>
        <el-dropdown-item command="shortcuts"><i class="fa-solid fa-keyboard"></i> Phim tat</el-dropdown-item>
        <el-dropdown-item command="mobile"><i class="fa-solid fa-paper-plane"></i> Tai ung dung Mobile</el-dropdown-item>
        <div class="help-footer">
          <button type="button" @click="handleCommand('about')">Gioi thieu</button>
          <button type="button" @click="handleCommand('terms')">Dieu khoan</button>
          <button type="button" @click="handleCommand('privacy')">Bao mat</button>
        </div>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const dropdownRef = ref(null)

const closeMenu = () => {
  dropdownRef.value?.handleClose?.()
}

const handleCommand = (command) => {
  const commandMap = {
    docs: () => router.push('/views'),
    updates: () => router.push('/dashboard'),
    navigation: () => router.push('/spaces'),
    learn: () => router.push('/analytics'),
    shortcuts: () => ElMessage.info('Shortcuts: Ctrl+K search, Esc close modal.'),
    support: () => ElMessage.info('Ho tro dang duoc phat trien. Hay lien he admin he thong.'),
    feedback: () => ElMessage.info('Cam on ban. Form phan hoi se duoc bo sung o ban tiep theo.'),
    community: () => ElMessage.info('Cong dong truc tuyen dang duoc chuan bi.'),
    mobile: () => ElMessage.info('Ung dung mobile chua san sang de tai.'),
    about: () => ElMessage.info('SprintA quan ly cong viec theo space, cycle va module.'),
    terms: () => ElMessage.info('Dieu khoan su dung se duoc cong bo trong trung tam tro giup.'),
    privacy: () => ElMessage.info('Chinh sach bao mat se duoc cong bo trong trung tam tro giup.')
  }

  commandMap[command]?.()
  closeMenu()
}
</script>

<style scoped>
.help-trigger-btn {
  border: 0;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 18px;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  transition: all 0.2s;
}

.help-trigger-btn:hover {
  color: var(--color-text-primary);
  background-color: var(--color-surface-hover);
}

.dark-help-menu {
  background: var(--color-surface);
  border: none;
  width: 320px;
  padding: 0;
}

.help-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  color: var(--color-text-primary);
  border-bottom: 1px solid var(--color-border);
}

.help-header h3 {
  font-size: 14px;
  font-weight: 600;
  margin: 0;
}

.close-btn {
  border: 0;
  background: transparent;
  cursor: pointer;
  font-size: 14px;
  color: var(--color-text-secondary);
}

.close-btn:hover {
  color: var(--color-text-primary);
}

:deep(.el-dropdown-menu__item) {
  color: var(--color-text-secondary) !important;
  padding: 10px 20px !important;
  font-size: 14px !important;
  display: flex;
  align-items: center;
  gap: 12px;
}

:deep(.el-dropdown-menu__item i) {
  font-size: 16px;
  width: 16px;
  text-align: center;
}

:deep(.el-dropdown-menu__item:hover) {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary) !important;
}

:deep(.el-dropdown-menu__item--divided) {
  border-top-color: var(--color-border) !important;
}

.help-footer {
  display: flex;
  justify-content: space-evenly;
  padding: 16px 20px;
  background-color: var(--color-surface);
  border-top: 1px solid var(--color-border);
  margin-top: 8px;
}

.help-footer button {
  border: 0;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 12px;
  text-decoration: none;
  cursor: pointer;
}

.help-footer button:hover {
  color: var(--color-text-primary);
  text-decoration: underline;
}
</style>

<style>
.el-popper.help-dropdown-popper.global-help-popper {
  background: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 8px !important;
  padding: 0 !important;
  box-shadow: 0 8px 16px rgba(0,0,0,0.2) !important;
}

.el-popper.help-dropdown-popper.global-help-popper .el-popper__arrow::before {
  background: var(--color-surface) !important;
  border-color: var(--color-border) !important;
}
</style>


