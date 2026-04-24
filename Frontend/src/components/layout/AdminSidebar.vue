<template>
  <aside class="admin-sidebar shadow-sm">
    <div class="sidebar-header">
      <img :src="logoImg" alt="SprintA Logo" class="nav-logo" />
      <h2>SprintA<span class="admin-badge">Admin</span></h2>
    </div>

    <div class="back-link">
      <a href="#" @click.prevent="goBack" class="flex items-center gap-2">
        <i class="fa-solid fa-arrow-left"></i>
        <span>{{ t('Back to App', 'Back to App') }}</span>
      </a>
    </div>

    <el-menu
      :default-active="activeMenu"
      class="admin-menu"
      :router="true"
    >
      <el-menu-item v-if="canAccessSystemAdmin" index="/admin/audit-log">
        <i class="fa-solid fa-file-lines menu-icon"></i>
        <span>{{ t('Audit Log', 'Audit Log') }}</span>
      </el-menu-item>

      <el-menu-item v-if="canAccessUserDirectory" index="/admin/users">
        <i class="fa-solid fa-users menu-icon"></i>
        <span>{{ t('User Management', 'User Management') }}</span>
      </el-menu-item>

      <el-menu-item v-if="canAccessUserDirectory" index="/admin/roles">
        <i class="fa-solid fa-shield-halved menu-icon"></i>
        <span>{{ t('Role Management', 'Role Management') }}</span>
      </el-menu-item>

      <el-menu-item v-if="canAccessSystemAdmin" index="/admin/configuration">
        <i class="fa-solid fa-gear menu-icon"></i>
        <span>{{ t('Configuration', 'Configuration') }}</span>
      </el-menu-item>

      <el-sub-menu v-if="false" index="/admin/instance">
        <template #title>
          <i class="fa-solid fa-server menu-icon"></i>
          <span>{{ t('Instance', 'Instance') }}</span>
        </template>
        <el-menu-item index="/admin/instance/general">{{ t('General settings', 'General settings') }}</el-menu-item>
      </el-sub-menu>

      <el-sub-menu v-if="canAccessSystemAdmin" index="/admin/security">
        <template #title>
          <i class="fa-solid fa-shield-halved menu-icon"></i>
          <span>{{ t('Security', 'Security') }}</span>
        </template>
        <el-menu-item index="/admin/instance/authentication">{{ t('Authentication', 'Authentication') }}</el-menu-item>
        <el-menu-item index="/admin/instance/email">{{ t('Email / SMTP', 'Email / SMTP') }}</el-menu-item>
        <el-menu-item index="/admin/security/2fa">{{ t('Two-Factor Auth', 'Two-Factor Auth') }}</el-menu-item>
        <el-menu-item index="/admin/security/password">{{ t('Change Password', 'Change Password') }}</el-menu-item>
        <el-menu-item v-if="false" index="/admin/security/ip-whitelist">{{ t('IP Whitelist', 'IP Whitelist') }}</el-menu-item>
      </el-sub-menu>

      <div style="flex-grow: 1;"></div>

      <div class="sidebar-footer">
        <div class="lang-selector" @click.stop="langMenuOpen = !langMenuOpen">
          <div class="lang-current">
            <span class="lang-flag">{{ locale === 'vi' ? 'VN' : 'EN' }}</span>
            <span class="lang-name">{{ locale === 'vi' ? 'Vietnamese' : 'English' }}</span>
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
                <span class="lang-flag">VN</span>
                <span>Vietnamese</span>
                <i v-if="locale === 'vi'" class="fa-solid fa-check lang-check"></i>
              </button>
              <button
                type="button"
                class="lang-option"
                :class="{ active: locale === 'en' }"
                @click.stop="setLocale('en')"
              >
                <span class="lang-flag">EN</span>
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
import { canAccessAdminUserDirectory, getStoredUser, hasSystemAdminAccess } from '@/utils/permissions'

const { locale, toggleLocale, t } = useLocale()

const langMenuOpen = ref(false)

const setLocale = (lang) => {
  if (locale.value !== lang) {
    toggleLocale()
  }
  langMenuOpen.value = false
}

const closeLangMenu = () => {
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
const currentUser = computed(() => getStoredUser())
const canAccessSystemAdmin = computed(() => hasSystemAdminAccess(currentUser.value))
const canAccessUserDirectory = computed(() => canAccessAdminUserDirectory(currentUser.value))

const goBack = () => {
  if (window.history.state && window.history.state.back) {
    router.back()
  } else {
    router.push('/dashboard')
  }
}

const activeMenu = computed(() => route.path)
</script>

<style scoped>
.admin-sidebar {
  width: 250px;
  background-color: var(--sidebar-bg);
  display: flex;
  flex-direction: column;
  border-right: 1px solid var(--color-border);
  z-index: 10;
}

.sidebar-header {
  padding: 32px 24px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.nav-logo {
  height: 32px;
  width: auto;
}

.sidebar-header h2 {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
  display: flex;
  align-items: center;
}

.admin-badge {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-accent);
  background: color-mix(in srgb, var(--color-accent) 15%, transparent);
  padding: 2px 8px;
  border-radius: 4px;
  margin-left: 8px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.back-link {
  padding: 0 24px 24px 24px;
}

.back-link a {
  color: var(--color-text-secondary);
  text-decoration: none;
  font-size: 13px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: color 0.2s;
}

.back-link a:hover {
  color: var(--color-text-primary);
}

.admin-menu {
  border-right: none;
  background-color: transparent;
}

.menu-icon {
  width: 24px;
  text-align: center;
  margin-right: 12px;
  font-size: 18px;
  color: var(--color-text-secondary);
}

::v-deep(.el-menu) {
  background-color: transparent !important;
}

:deep(.el-menu-item), :deep(.el-sub-menu__title) {
  height: 48px;
  line-height: 48px;
  margin: 4px 12px;
  border-radius: 8px;
  color: var(--color-text-secondary) !important;
  background-color: transparent !important;
  font-weight: 500;
  font-size: 14px;
  transition: all 0.2s !important;
}

:deep(.el-menu-item:hover), :deep(.el-sub-menu__title:hover) {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary) !important;
}

:deep(.el-menu-item.is-active) {
  background-color: color-mix(in srgb, var(--color-accent) 10%, transparent) !important;
  color: var(--color-accent) !important;
  font-weight: 600;
}

:deep(.el-menu-item.is-active .menu-icon) {
  color: var(--color-accent) !important;
}

.sidebar-footer {
  padding: 24px;
  margin-top: auto;
  border-top: 1px solid var(--color-border);
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
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface);
  color: var(--color-text-secondary);
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.lang-current:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.lang-flag {
  font-size: 12px;
  font-weight: 700;
  min-width: 18px;
  text-align: center;
}

.lang-name {
  flex: 1;
}

.lang-arrow {
  font-size: 10px;
  color: var(--color-text-muted);
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
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 4px;
  box-shadow: var(--shadow-md);
  z-index: 999;
}

.lang-option {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 10px 12px;
  border: none;
  border-radius: 6px;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 13px;
  cursor: pointer;
  transition: all 0.15s ease;
  text-align: left;
}

.lang-option:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.lang-option.active {
  background: color-mix(in srgb, var(--color-accent) 10%, transparent);
  color: var(--color-accent);
  font-weight: 600;
}

.lang-check {
  margin-left: auto;
  font-size: 11px;
  color: var(--color-accent);
}

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
