<template>
  <aside class="admin-sidebar shadow-sm">
    <div class="sidebar-header">
      <img :src="logoImg" alt="SprintA Logo" class="nav-logo" />
      <h2>SprintA<span style="font-size: 14px; font-weight: 500; color: #64748b; margin-left: 6px">Admin</span></h2>
    </div>

    <div class="back-link">
      <a href="#" @click.prevent="goBack" class="flex items-center gap-2">
        <i class="fa-solid fa-arrow-left"></i>
        <span>{{ t('Back to App', 'Quay lại Ứng dụng') }}</span>
      </a>
    </div>

    <el-menu
      :default-active="activeMenu"
      class="admin-menu"
      :router="true"
    >
      <el-menu-item index="/admin/audit-log">
        <i class="fa-solid fa-file-lines menu-icon"></i>
        <span>{{ t('Audit Log', 'Nhật ký Hệ thống') }}</span>
      </el-menu-item>

      <el-menu-item index="/admin/users">
        <i class="fa-solid fa-users menu-icon"></i>
        <span>{{ t('User Management', 'Quản lý Người dùng') }}</span>
      </el-menu-item>

      <!-- <el-sub-menu index="/admin/organization">
        <template #title>
          <i class="fa-regular fa-building menu-icon"></i>
          <span>{{ t('Organization', 'Tổ chức') }}</span>
        </template>
        <el-menu-item index="/admin/organization/profile">{{ t('Profile', 'Hồ sơ') }}</el-menu-item>
        <el-menu-item index="/admin/organization/contact">{{ t('Contact', 'Liên hệ') }}</el-menu-item>
      </el-sub-menu> -->

      <el-menu-item index="/admin/configuration">
        <i class="fa-solid fa-gear menu-icon"></i>
        <span>{{ t('Configuration', 'Cấu hình') }}</span>
      </el-menu-item>

      <el-sub-menu index="/admin/security">
        <template #title>
          <i class="fa-solid fa-shield-halved menu-icon"></i>
          <span>{{ t('Security', 'Bảo mật') }}</span>
        </template>
        <el-menu-item index="/admin/security/2fa">{{ t('Two-Factor Auth', 'Xác thực 2 bước') }}</el-menu-item>
        <el-menu-item index="/admin/security/password">{{ t('Change Password', 'Đổi mật khẩu') }}</el-menu-item>
        <el-menu-item index="/admin/security/ip-whitelist">{{ t('IP Whitelist', 'IP cho phép') }}</el-menu-item>
      </el-sub-menu>
      <div style="flex-grow: 1;"></div>
      
      <div class="sidebar-footer">
        <div class="lang-selector" @click.stop="langMenuOpen = !langMenuOpen">
          <div class="lang-current">
            <span class="lang-flag">{{ locale === 'vi' ? '🇻🇳' : '🇺🇸' }}</span>
            <span class="lang-name">{{ locale === 'vi' ? 'Tiếng Việt' : 'English' }}</span>
            <i class="fa-solid fa-chevron-down lang-arrow" :class="{ 'is-open': langMenuOpen }"></i>
          </div>

          <Transition name="lang-dropdown">
            <div v-if="langMenuOpen" class="lang-dropdown">
              <button
                type="button"
                class="lang-option"
                :class="{ active: locale === 'vi' }"
                @click.stop="setLocale('vi')"
              >
                <span class="lang-flag">🇻🇳</span>
                <span>Tiếng Việt</span>
                <i v-if="locale === 'vi'" class="fa-solid fa-check lang-check"></i>
              </button>
              <button
                type="button"
                class="lang-option"
                :class="{ active: locale === 'en' }"
                @click.stop="setLocale('en')"
              >
                <span class="lang-flag">🇺🇸</span>
                <span>English</span>
                <i v-if="locale === 'en'" class="fa-solid fa-check lang-check"></i>
              </button>
            </div>
          </Transition>
        </div>
      </div>
    </el-menu>
  </aside>
</template>

<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import logoImg from '@/assets/logo_QLCV.png'
import { useLocale } from '@/composables/useLocale'

const { locale, toggleLocale, t } = useLocale()

const langMenuOpen = ref(false)

const setLocale = (lang) => {
  if (locale.value !== lang) {
    toggleLocale()
  }
  langMenuOpen.value = false
}

const closeLangMenu = (e) => {
  langMenuOpen.value = false
}

onMounted(() => {
  document.addEventListener('click', closeLangMenu)
})

onUnmounted(() => {
  document.removeEventListener('click', closeLangMenu)
})

const route = useRoute()
const router = useRouter()

const goBack = () => {
  if (window.history.state && window.history.state.back) {
    router.back()
  } else {
    router.push('/dashboard')
  }
}

const activeMenu = computed(() => {
  return route.path
})
</script>

<style scoped>
.admin-sidebar {
  width: 250px;
  background-color: #0d0f11;
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  display: flex;
  flex-direction: column;
  border-right: 1px solid #1e2025;
  z-index: 10;
}

.sidebar-header {
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.nav-logo {
  height: 28px;
  width: auto;
}

.sidebar-header h2 {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
  display: flex;
  align-items: baseline;
}

.back-link {
  padding: 0 24px 24px 24px;
}

.back-link a {
  color: var(--text-secondary);
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
}

.back-link a:hover {
  color: var(--text-primary);
}

.admin-menu {
  border-right: none;
  background-color: transparent;
}

.menu-icon {
  width: 24px;
  text-align: center;
  margin-right: 8px;
  font-size: 16px;
  color: var(--text-secondary);
}

:deep(.el-menu) {
  background-color: transparent !important;
}

:deep(.el-menu-item) {
  height: 44px;
  line-height: 44px;
  margin: 4px 12px;
  border-radius: 6px;
  color: var(--text-primary) !important;
  background-color: transparent !important;
}

:deep(.el-sub-menu__title) {
  height: 44px;
  line-height: 44px;
  margin: 4px 12px;
  border-radius: 6px;
  color: var(--text-primary) !important;
  background-color: transparent !important;
}

:deep(.el-menu-item.is-active) {
  background-color: color-mix(in srgb, var(--bg-layout) 15%, var(--text-primary) 85%) !important;
  color: var(--bg-layout) !important;
  font-weight: 700;
}

:deep(.el-menu-item.is-active .menu-icon) {
  color: var(--bg-layout) !important;
}

:deep(.el-menu-item:hover), :deep(.el-sub-menu__title:hover) {
  background-color: var(--bg-hover) !important;
}

.sidebar-footer {
  padding: 16px 24px 24px;
  margin-top: auto;
}

.lang-selector {
  position: relative;
  cursor: pointer;
}

.lang-current {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.lang-current:hover {
  background: rgba(255, 255, 255, 0.06);
  border-color: rgba(255, 255, 255, 0.15);
  color: var(--text-primary);
}

.lang-flag {
  font-size: 18px;
  line-height: 1;
}

.lang-name {
  flex: 1;
}

.lang-arrow {
  font-size: 11px;
  color: #64748b;
  transition: transform 0.2s ease;
}

.lang-arrow.is-open {
  transform: rotate(180deg);
}

.lang-dropdown {
  position: absolute;
  bottom: calc(100% + 8px);
  left: 0;
  right: 0;
  background: #1a1d23;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 6px;
  box-shadow: 0 -8px 32px rgba(0, 0, 0, 0.4);
  z-index: 999;
}

.lang-option {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 10px 12px;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: #a1a1aa;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.15s ease;
  text-align: left;
}

.lang-option:hover {
  background: rgba(255, 255, 255, 0.06);
  color: #e4e4e7;
}

.lang-option.active {
  background: rgba(59, 130, 246, 0.12);
  color: #60a5fa;
  font-weight: 600;
}

.lang-check {
  margin-left: auto;
  font-size: 12px;
  color: #3b82f6;
}

/* Dropdown animation */
.lang-dropdown-enter-active {
  transition: all 0.2s ease;
}
.lang-dropdown-leave-active {
  transition: all 0.15s ease;
}
.lang-dropdown-enter-from,
.lang-dropdown-leave-to {
  opacity: 0;
  transform: translateY(6px);
}
</style>
