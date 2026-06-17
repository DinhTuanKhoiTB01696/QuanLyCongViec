<template>
  <el-dropdown trigger="click" popper-class="user-dropdown-popper" @command="handleCommand" :teleported="true">
    <div class="user-avatar-trigger" :style="avatarStyle">
      {{ avatarUrl ? '' : userInitial }}
    </div>
    <template #dropdown>
      <el-dropdown-menu class="jira-user-menu">
        <div class="user-menu-header">
          <div class="header-avatar" :style="avatarStyle">{{ avatarUrl ? '' : userInitial }}</div>
          <div class="header-info">
            <div class="user-display-name">{{ userDisplayName }}</div>
            <div class="user-email">{{ userEmail }}</div>
          </div>
        </div>

        <div class="menu-divider"></div>

        <el-dropdown-item command="profile">
          <div class="menu-item-inner">
            <i class="fa-regular fa-user"></i>
            <span>{{ t('userMenu.profile') }}</span>
          </div>
        </el-dropdown-item>

        <el-dropdown-item v-if="canAccessAdmin" command="admin">
          <div class="menu-item-inner">
            <i class="fa-solid fa-shield-halved"></i>
            <span>{{ t('userMenu.projectAdministration') }}</span>
          </div>
        </el-dropdown-item>

        <div class="theme-trigger-item" @click.stop="toggleThemeSub">
          <div class="menu-item-inner">
            <i class="fa-solid fa-circle-half-stroke"></i>
            <span>{{ t('userMenu.theme') }}</span>
            <i class="fa-solid fa-chevron-right arrow-icon" :class="{ rotated: themeSubVisible }"></i>
          </div>

          <transition name="el-zoom-in-top">
            <div v-if="themeSubVisible" class="theme-expanded-menu">
              <div class="theme-option" :class="{ active: currentTheme === 'light' }" @click.stop="selectTheme('light')">
                <div class="radio-indicator">
                  <i v-if="currentTheme === 'light'" class="fa-solid fa-circle-dot"></i>
                  <i v-else class="fa-regular fa-circle"></i>
                </div>
                <div class="theme-preview-box light">
                  <div class="p-header"></div>
                  <div class="p-body"><div class="p-sidebar"></div><div class="p-content"></div></div>
                </div>
                <span class="option-label">{{ t('userMenu.light') }}</span>
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
                <span class="option-label">{{ t('userMenu.dark') }}</span>
              </div>
            </div>
          </transition>
        </div>

        <div class="menu-divider"></div>

        <!-- BUG-HOST-015: Hidden as it's not supported yet -->
        <!-- 
        <el-dropdown-item command="switch">
          <div class="menu-item-inner">
            <i class="fa-solid fa-users-viewfinder"></i>
            <span>Switch account</span>
          </div>
        </el-dropdown-item>
        -->

        <el-dropdown-item command="logout" class="logout-item-wrapper">
          <div class="menu-item-inner logout-item">
            <i class="fa-solid fa-arrow-right-from-bracket"></i>
            <span>{{ t('userMenu.logOut') }}</span>
          </div>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { currentTheme, toggleTheme } from '@/utils/theme'
import { getStoredUser, hasSystemAdminAccess } from '@/utils/permissions'
import { clearAuthSession } from '@/utils/authSession'
import { openNamedAppWindow, PROJECT_ADMIN_WINDOW_NAME } from '@/utils/windowTabs'
import axiosClient from '@/api/axiosClient'
import { useI18n } from '@/composables/useI18n'

const router = useRouter()
const { t } = useI18n()
const themeSubVisible = ref(false)
const profileData = ref(null)

const currentUser = computed(() => profileData.value || getStoredUser())
const canAccessAdmin = computed(() => hasSystemAdminAccess(currentUser.value))
const userDisplayName = computed(() => currentUser.value?.fullName || currentUser.value?.name || currentUser.value?.publicName || currentUser.value?.email?.split('@')?.[0] || t('userMenu.userFallback'))
const userEmail = computed(() => currentUser.value?.email || 'user@example.com')

const getInitials = (name) => {
  if (!name) return '?'
  return name
    .split(' ')
    .filter(Boolean)
    .map((word) => word[0])
    .join('')
    .substring(0, 2)
    .toUpperCase()
}

const userInitial = computed(() => getInitials(userDisplayName.value))

const getBaseUrl = () => import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5136'

const avatarUrl = computed(() => currentUser.value?.avatarUrl || '')

const avatarStyle = computed(() => {
  if (!avatarUrl.value) {
    return { backgroundColor: avatarColor.value }
  }
  return {
    backgroundImage: `url(${getBaseUrl()}${avatarUrl.value})`,
    backgroundSize: 'cover',
    backgroundPosition: 'center',
    color: 'transparent',
    border: '1px solid var(--color-border)'
  }
})

const avatarColor = computed(() => {
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d']
  const name = userDisplayName.value || 'User'
  const index = name.length % colors.length
  return colors[index]
})

const fetchProfile = async () => {
  try {
    const response = await axiosClient.get('/users/me')
    profileData.value = response.data?.data
  } catch (error) {
    console.error('Failed to fetch user profile in dropdown', error)
  }
}

const handleAvatarUpdate = (event) => {
  if (profileData.value) {
    profileData.value.avatarUrl = event.detail.avatarUrl
  }
}

onMounted(() => {
  fetchProfile()
  window.addEventListener('user-avatar-updated', handleAvatarUpdate)
})

onUnmounted(() => {
  window.removeEventListener('user-avatar-updated', handleAvatarUpdate)
})

const toggleThemeSub = () => {
  themeSubVisible.value = !themeSubVisible.value
}

const handleCommand = async (cmd) => {
  if (cmd === 'profile') {
    router.push('/profile')
  } else if (cmd === 'admin') {
    const routeData = router.resolve('/admin')
    openNamedAppWindow(routeData.href, PROJECT_ADMIN_WINDOW_NAME)
  } else if (cmd === 'logout') {
    try {
      await axiosClient.post('/auth/logout')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      clearAuthSession()
      router.push('/login')
    }
  }

  if (cmd !== 'theme') {
    themeSubVisible.value = false
  }
}

const selectTheme = (theme) => {
  toggleTheme(theme)
  themeSubVisible.value = false
}
</script>

<style scoped>
.jira-user-menu {
  width: 300px;
  background-color: var(--color-surface);
  border-radius: 2px;
  padding: 8px 0;
  border: 1px solid var(--color-border);
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
  color: var(--color-text-primary) !important;
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
  box-shadow: none !important;
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
