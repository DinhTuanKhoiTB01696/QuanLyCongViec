<template>
  <el-dropdown trigger="click" popper-class="user-dropdown-popper" @command="handleCommand" :teleported="true">
    <div class="user-avatar-trigger" :style="{ backgroundColor: avatarColor }">
      {{ initials }}
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-user-menu">
        <!-- User Info Header -->
        <div class="user-menu-header">
          <div class="header-avatar" :style="{ backgroundColor: avatarColor }">{{ initials }}</div>
          <div class="header-info">
            <div class="user-display-name">{{ user.fullName || 'Thành viên' }}</div>
            <div class="user-email">{{ user.email || 'user@example.com' }}</div>
          </div>
        </div>

        <div class="menu-divider"></div>

        <el-dropdown-item command="profile">
          <div class="menu-item-inner">
            <i class="fa-regular fa-user"></i>
            <span>Hồ sơ</span>
          </div>
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
              <div class="theme-option" :class="{ active: currentTheme === 'light' }" @click.stop="selectTheme('light')">
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

              <div class="theme-option" :class="{ active: currentTheme === 'dark' }" @click.stop="selectTheme('dark')">
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
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { currentTheme, toggleTheme } from '@/utils/theme'

const router = useRouter()
const themeSubVisible = ref(false)
const user = ref({ fullName: 'Thành viên', email: 'user@example.com' })

const initials = computed(() => {
  if (!user.value.fullName) return 'U'
  return user.value.fullName.charAt(0).toUpperCase()
})

const avatarColor = computed(() => {
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d']
  const index = (user.value.fullName?.length || 0) % colors.length
  return colors[index]
})

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

onMounted(() => {
  const savedUser = localStorage.getItem('user')
  if (savedUser) {
    user.value = JSON.parse(savedUser)
  }
})
</script>

<style scoped>
.jira-user-menu {
  width: 300px;
  background-color: var(--color-surface);
  border-radius: 2px; /* Sharp UI */
  padding: 8px 0;
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-xl);
}
.user-menu-header { display: flex; padding: 12px 16px; gap: 12px; align-items: center; border-bottom: 1px solid var(--color-border); margin-bottom: 4px; }
.header-avatar { 
  width: 40px; height: 40px; 
  border-radius: 50%; 
  display: flex; align-items: center; justify-content: center; 
  font-size: 16px; font-weight: 700; color: #fff; 
}
.user-display-name { font-size: 14px; font-weight: 700; color: var(--color-text-primary); }
.user-email { font-size: 12px; color: var(--color-text-muted); }

.menu-item-inner { 
  display: flex; 
  align-items: center; 
  gap: 12px; 
  color: var(--color-text-primary) !important; /* Ensure high contrast */
  font-size: 13px;
  font-weight: 500;
}
.menu-item-inner i { width: 16px; text-align: center; color: var(--color-text-muted); font-size: 14px; }

.logout-item i, .logout-item span { color: #ef4444 !important; }
.menu-divider { height: 1px; background-color: var(--color-border); margin: 4px 0; }

.theme-trigger-item { padding: 10px 16px; cursor: pointer; transition: all 0.2s; }
.theme-trigger-item:hover { background-color: var(--color-surface-hover); }

.theme-expanded-menu { 
  margin: 8px 12px; 
  background-color: var(--color-bg); 
  border-radius: 4px; 
  padding: 4px 0; 
  border: 1px solid var(--color-border); 
}
.theme-option { 
  display: flex; 
  align-items: center; 
  padding: 8px 12px; 
  gap: 12px; 
  cursor: pointer; 
  color: var(--color-text-secondary); 
  transition: all 0.2s;
}
.theme-option:hover { 
  background-color: var(--color-surface-hover); 
  color: var(--color-text-primary);
}
.theme-option.active { 
  background-color: color-mix(in srgb, var(--color-accent) 15%, transparent); 
  color: var(--color-accent); 
}
.option-label { font-size: 12px; font-weight: 600; }
.radio-indicator { color: var(--color-accent); font-size: 12px; }
.arrow-icon { margin-left: auto; transition: transform 0.2s; color: var(--color-text-muted); font-size: 10px !important; }
.arrow-icon.rotated { transform: rotate(90deg); }

.theme-preview-box { 
  width: 36px; height: 24px; border-radius: 2px; border: 1px solid var(--color-border); 
  overflow: hidden; display: flex; flex-direction: column; 
}
.light.theme-preview-box { background: #ffffff; }
.dark.theme-preview-box { background: #0f172a; border-color: #334155; }
.p-header { height: 4px; background: #f1f5f9; }
.dark .p-header { background: #1e293b; }
.p-body { flex: 1; display: flex; }
.p-sidebar { width: 10px; background: #f8fafc; border-right: 1px solid #e2e8f0; }
.dark .p-sidebar { background: #020617; border-color: #1e293b; }
.p-content { flex: 1; background: #ffffff; }
.dark .p-content { background: #0f172a; }

.user-avatar-trigger { 
  width: 30px; height: 30px; 
  color: #fff; border-radius: 50%; 
  display: flex; align-items: center; justify-content: center; 
  font-weight: 700; font-size: 11px; cursor: pointer; 
  border: 1px solid rgba(255,255,255,0.1);
}
</style>

<style>
.user-dropdown-popper.el-popper { 
  background: var(--color-surface) !important; 
  border: 1px solid var(--color-border) !important; 
  border-radius: 12px !important; 
  padding: 0 !important; 
  box-shadow: var(--shadow-xl) !important; 
}
.user-dropdown-popper .el-dropdown-menu__item { 
  padding: 10px 16px !important; 
  background-color: transparent !important; 
  color: var(--color-text-primary) !important; 
}
.user-dropdown-popper .el-dropdown-menu__item:hover { 
  background-color: var(--color-surface-hover) !important; 
}
.user-dropdown-popper .el-popper__arrow::before { 
  background: var(--color-surface) !important; 
  border: 1px solid var(--color-border) !important; 
}
</style>
