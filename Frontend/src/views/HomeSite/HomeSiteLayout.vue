<template>
  <div class="home-site-container">
    <AppTopBar @toggle-create="showCreateModal = true" />

    <div class="home-content-wrapper">
      <!-- Sidebar Jira Style -->
      <aside class="sidebar">
        <div class="sidebar-header">
          <div class="site-switcher" @click="goToSiteSelection">
            <div class="site-icon">S</div>
            <div class="site-info">
              <span class="site-name">SprintA Home</span>
              <span class="site-subtitle">{{ t('Site Management') }}</span>
            </div>
            <span class="dropdown-icon">▾</span>
          </div>
        </div>
        
        <nav class="sidebar-nav">
          <router-link to="/sites" class="nav-link" :class="{ 'active-nav': $route.path === '/sites' }">
            <CircleUser class="nav-icon" />
            <span>{{ t('For you', 'Dành cho bạn') }}</span>
          </router-link>
          <router-link to="/home/recent" class="nav-link" :class="{ 'active-nav': $route.path === '/home/recent' }">
            <Clock class="nav-icon" />
            <span>{{ t('Recent', 'Gần đây') }}</span>
          </router-link>
          <router-link to="/home/starred" class="nav-link" :class="{ 'active-nav': isModule('starred') }">
            <Star class="nav-icon" :class="{'fill-current': isModule('starred')}" />
            <span>{{ t('Starred', 'Có gắn sao') }}</span>
          </router-link>
          <router-link to="/home/notifications" class="nav-link" :class="{ 'active-nav': isModule('notifications') }">
            <Bell class="nav-icon" />
            <span>{{ t('Notifications', 'Thông báo') }}</span>
          </router-link>
          <router-link to="/home/status" class="nav-link" :class="{ 'active-nav': isModule('status') }">
            <Megaphone class="nav-icon" />
            <span>{{ t('Status', 'Cập nhật trạng thái') }}</span>
          </router-link>
          
          <div class="nav-divider"></div>
          
          <router-link to="/site-selection" class="nav-link">
            <Hexagon class="nav-icon text-accent" />
            <span>SprintA</span>
          </router-link>
          <router-link to="/home/teams" class="nav-link" :class="{ 'active-nav': isModule('teams') }">
            <Users class="nav-icon" />
            <span>{{ t('Teams') }}</span>
          </router-link>
          <router-link to="/home/goals" class="nav-link" :class="{ 'active-nav': isModule('goals') }">
            <Target class="nav-icon" />
            <span>{{ t('Goals') }}</span>
          </router-link>
          <router-link to="/home/projects" class="nav-link" :class="{ 'active-nav': isModule('projects') }">
            <Rocket class="nav-icon" />
            <span>{{ t('Projects') }}</span>
          </router-link>
        </nav>
      </aside>

      <!-- Main Content Area -->
      <main class="main-content">
        <slot>
          <router-view></router-view>
        </slot>
      </main>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getStoredUser } from '@/utils/permissions'
import AppTopBar from '@/components/layout/AppTopBar.vue'
import { useI18nStore } from '@/store/useI18nStore'
import { CircleUser, Clock, Star, Bell, Megaphone, Hexagon, Users, Target, Rocket } from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()
const i18nStore = useI18nStore()
const t = i18nStore.t

const showCreateModal = ref(false)

const currentUser = getStoredUser()
const userEmail = currentUser?.email || ''
const derivedName = userEmail ? userEmail.split('@')[0] : 'User'
const userName = currentUser?.username || currentUser?.name || currentUser?.publicName || derivedName
const userInitials = userName ? userName.substring(0, 2).toUpperCase() : 'US'

const isModule = (moduleName) => {
  if (moduleName === 'people') {
    return route.path.includes('/home/people') || route.path.includes('/home/profile')
  }
  if (moduleName === 'teams') {
    return route.path.includes('/home/teams') || route.path.includes('/home/people') || route.path.includes('/home/profile')
  }
  return route.path.includes(`/home/${moduleName}`)
}

const goToSiteSelection = () => {
  router.push('/site-selection')
}
</script>

<style scoped>
.home-site-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background-color: var(--color-bg);
  color: var(--color-text-primary);
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  overflow: hidden;
}

/* Layout Wrapper */
.home-content-wrapper {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* Sidebar */
.sidebar {
  width: 240px;
  background-color: var(--color-bg);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  overflow-y: auto;
}

.sidebar-header {
  padding: 24px 16px 16px;
}

.site-switcher {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: background-color 0.2s;
}

.site-switcher:hover {
  background-color: var(--color-surface-hover);
}

.site-icon {
  width: 32px;
  height: 32px;
  background-color: var(--color-accent);
  color: white;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: bold;
  flex-shrink: 0;
}

.site-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}

.site-name {
  font-weight: 600;
  font-size: 14px;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.site-subtitle {
  font-size: 12px;
  color: var(--color-text-secondary);
}

.dropdown-icon {
  color: var(--color-text-muted);
  font-size: 12px;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  padding: 0 16px;
}

.nav-section-title {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  margin: 16px 0 8px 8px;
  letter-spacing: 0.5px;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  text-decoration: none;
  color: var(--color-text-secondary);
  font-size: 14px;
  font-weight: 500;
  border-radius: var(--radius-sm);
  margin-bottom: 2px;
  transition: all 0.2s;
}

.nav-link:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.nav-link.active-nav, .router-link-active.nav-link {
  background-color: color-mix(in srgb, var(--color-accent) 10%, transparent);
  color: var(--color-accent);
  font-weight: 600;
}

.nav-icon {
  width: 18px;
  height: 18px;
  color: currentColor;
}

.text-accent {
  color: var(--color-accent);
}

.nav-divider {
  height: 1px;
  background-color: var(--color-border);
  margin: 16px 8px;
}

/* Main Content */
.main-content {
  flex: 1;
  overflow-y: auto;
  background-color: var(--color-surface);
  position: relative;
}
</style>
