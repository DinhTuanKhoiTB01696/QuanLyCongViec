<template>
  <el-dropdown trigger="click" popper-class="user-dropdown-popper" @command="handleCommand" :teleported="true">
    <div class="user-avatar-trigger">
      U
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-user-menu">
        <!-- User Info Header -->
        <div class="user-menu-header">
          <div class="header-avatar">U</div>
          <div class="header-info">
            <div class="user-display-name">Người dùng</div>
            <div class="user-email">user@example.com</div>
          </div>
        </div>

        <div class="menu-divider"></div>

        <el-dropdown-item command="profile">
          <router-link to="/profile" style="text-decoration: none; color: inherit; display: block; width: 100%;">
            <div class="menu-item-inner">
              <i class="fa-regular fa-user"></i>
              <span>Hồ sơ</span>
            </div>
          </router-link>
        </el-dropdown-item>

        <!-- Theme Sub-menu -->
        <div class="theme-trigger-item" @click.stop="toggleThemeSub">
          <div class="menu-item-inner">
            <i class="fa-solid fa-circle-half-stroke"></i>
            <span>Chủ đề</span>
            <i class="fa-solid fa-chevron-right arrow-icon" :class="{ rotated: themeSubVisible }"></i>
          </div>

          <!-- expansion area -->
          <transition name="el-zoom-in-top">
            <div class="theme-expanded-menu" v-if="themeSubVisible">
              <div class="theme-option" :class="{ active: currentTheme === 'light' }" @click.stop="currentTheme = 'light'">
                <div class="radio-indicator">
                  <i v-if="currentTheme === 'light'" class="fa-solid fa-circle-dot"></i>
                  <i v-else class="fa-regular fa-circle"></i>
                </div>
                <div class="theme-preview-box light">
                  <div class="p-header"></div>
                  <div class="p-body"><div class="p-sidebar"></div><div class="p-content"></div></div>
                </div>
                <span class="option-label">Sáng</span>
              </div>

              <div class="theme-option" :class="{ active: currentTheme === 'dark' }" @click.stop="currentTheme = 'dark'">
                <div class="radio-indicator">
                  <i v-if="currentTheme === 'dark'" class="fa-solid fa-circle-dot"></i>
                  <i v-else class="fa-regular fa-circle"></i>
                </div>
                <div class="theme-preview-box dark">
                  <div class="p-header"></div>
                  <div class="p-body"><div class="p-sidebar"></div><div class="p-content"></div></div>
                </div>
                <span class="option-label">Tối</span>
              </div>
            </div>
          </transition>
        </div>

        <div class="menu-divider"></div>

        <el-dropdown-item command="switch">
          <div class="menu-item-inner">
            <i class="fa-solid fa-users-viewfinder"></i>
            <span>Chuyển tài khoản</span>
          </div>
        </el-dropdown-item>

        <el-dropdown-item command="logout" class="logout-item-wrapper">
          <div class="menu-item-inner logout-item">
            <i class="fa-solid fa-arrow-right-from-bracket"></i>
            <span>Đăng xuất</span>
          </div>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { currentTheme, toggleTheme } from '@/utils/theme'

const router = useRouter()
const themeSubVisible = ref(false)

const toggleThemeSub = () => {
  themeSubVisible.value = !themeSubVisible.value
}

const handleCommand = async (cmd) => {
  if (cmd === 'profile') {
    router.push('/profile')
  } else if (cmd === 'logout') {
    try {
      const { default: axiosClient } = await import('@/api/axiosClient')
      await axiosClient.post('/auth/logout')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      localStorage.removeItem('accessToken')
      localStorage.removeItem('user')
      router.push('/login')
    }
  }

  if (cmd !== 'theme') {
    themeSubVisible.value = false
  }
}

const selectTheme = (theme) => {
  toggleTheme(theme)
}
</script>

<style scoped>
.jira-user-menu {
  width: 320px;
  background-color: var(--bg-content);
  border-radius: 8px;
  padding: 12px 0;
  border: 1px solid var(--border-color);
  box-shadow: 0 12px 48px rgba(0,0,0,0.5);
}
.user-menu-header { display: flex; padding: 8px 16px 20px; gap: 16px; align-items: center; }
.header-avatar { width: 50px; height: 50px; background-color: #f59e0b; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 18px; font-weight: 700; color: #1d2125; }
.user-display-name { font-size: 16px; font-weight: 600; color: var(--text-primary); }
.user-email { font-size: 12px; color: var(--text-secondary); }
.menu-item-inner { display: flex; align-items: center; gap: 12px; color: var(--text-primary); }
.menu-item-inner i { width: 20px; text-align: center; color: var(--text-secondary); font-size: 16px; }
.logout-item i, .logout-item span { color: #f87171; }
.menu-divider { height: 1px; background-color: var(--border-color); margin: 8px 0; }
.theme-trigger-item { padding: 10px 16px; cursor: pointer; transition: all 0.2s; }
.theme-trigger-item:hover { background-color: var(--hover-bg); }
.arrow-icon { margin-left: auto; transition: transform 0.2s; }
.arrow-icon.rotated { transform: rotate(90deg); }
.theme-expanded-menu { margin-top: 8px; background-color: var(--bg-secondary); border-radius: 6px; padding: 4px 0; border: 1px solid var(--border-color); }
.theme-option { display: flex; align-items: center; padding: 10px 16px; gap: 16px; cursor: pointer; color: var(--text-secondary); }
.theme-option:hover { background-color: var(--hover-bg); }
.theme-option.active { background-color: var(--active-bg); color: #579dff; }
.theme-preview-box { width: 48px; height: 32px; border-radius: 3px; border: 1px solid var(--border-color); overflow: hidden; display: flex; flex-direction: column; }
.light.theme-preview-box { background: #fff; border-color: #ddd; }
.dark.theme-preview-box { background: #0d1117; border-color: #21262d; }
.p-header { height: 8px; background: #ebecf0; }
.dark .p-header { background: #21262d; }
.p-body { flex: 1; display: flex; }
.p-sidebar { width: 12px; background: #f4f5f7; border-right: 1px solid #eee; }
.dark .p-sidebar { background: #161b22; border-color: #30363d; }
.p-content { flex: 1; background: #fff; }
.dark .p-content { background: #0d1117; }
.user-avatar-trigger { width: 32px; height: 32px; background: #f59e0b; color: #1d2125; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 12px; cursor: pointer; }
</style>

<style>
.user-dropdown-popper.el-popper { background: var(--bg-content) !important; border: 1px solid var(--border-color) !important; border-radius: 8px !important; padding: 0 !important; box-shadow: 0 12px 48px rgba(0,0,0,0.6) !important; }
.user-dropdown-popper .el-dropdown-menu__item { padding: 10px 16px !important; background-color: transparent !important; }
.user-dropdown-popper .el-dropdown-menu__item:hover { background-color: var(--hover-bg) !important; }
.user-dropdown-popper .el-popper__arrow::before { background: var(--bg-content) !important; border: 1px solid var(--border-color) !important; }
</style>

