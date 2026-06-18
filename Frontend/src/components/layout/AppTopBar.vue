<template>
  <header class="app-topbar">
    <div class="nav-left">
      <div class="app-launcher-icon">
        <Grip class="w-5 h-5" />
      </div>

      <div class="sprinta-logo" @click="router.push('/site-selection')">
        <img src="@/assets/logo_QLCV.png" alt="SprintA Logo" class="sprinta-logo-img" />
        <span class="logo-text">SprintA</span>
      </div>

      <!-- Home Site Context Navigation -->
      <nav v-if="isHomeContext" class="topbar-nav">
        <!-- Removed Teams, Goals, Projects, People links as requested by user -->
      </nav>

      <!-- Space Project Context Navigation -->
      <div v-else-if="isSpaceContext" class="space-nav">
        <div class="workspace-switcher" @click="router.push('/spaces')">
          <div class="ws-icon">{{ workspaceBadge }}</div>
          <span class="ws-name">{{ workspaceName }}</span>
          <ChevronDown class="w-3.5 h-3.5 ms-1 opacity-70" />
        </div>
        <button class="menu-toggle" @click="emit('toggle-sidebar')">
          <Menu class="w-4 h-4" />
        </button>
      </div>
    </div>

    <div class="nav-center" ref="searchWrapperRef">
      <div class="search-input-wrapper">
        <Search class="w-3.5 h-3.5 search-icon" />
        <input type="text" :placeholder="isSpaceContext ? t('Search work items...') : t('Search')" v-model="searchQuery" @input="handleSearchInput" />
        <div v-if="showSearchDropdown" class="search-dropdown">
          <div v-if="searching" class="search-state">{{ t('Searching...', 'Đang tìm kiếm...') }}</div>
          <template v-else-if="searchResults.length">
            <button v-for="result in searchResults" :key="result.id" type="button" class="search-result" @click="openSearchResult(result)">
              <strong>{{ result.sequenceId || result.title || result.name }}</strong>
              <span>{{ result.title || result.description || result.type }}</span>
              <small v-if="result.projectName">{{ result.projectName }}</small>
            </button>
          </template>
          <div v-else class="search-state">{{ t('No items found.', 'Không tìm thấy kết quả.') }}</div>
        </div>
      </div>
      <SprintaButton v-if="isHomeContext" variant="primary" class="ms-2" @click="handleGlobalCreate">{{ t('Create', 'Tạo') }}</SprintaButton>
    </div>

    <div class="nav-right">
      <NotificationsDropdown v-if="isSpaceContext" />
      <button class="icon-btn" @click="goToNotifications" v-else>
        <Bell class="w-4 h-4" />
      </button>

      <button class="icon-btn" @click="toggleTheme()" :title="currentTheme === 'dark' ? 'Light mode' : 'Dark mode'">
        <Sun v-if="currentTheme === 'dark'" class="w-4 h-4" />
        <Moon v-else class="w-4 h-4" />
      </button>

      <button v-if="isSpaceContext" class="icon-btn" @click="emit('toggle-ai')">
        <Bot class="w-4 h-4" />
      </button>

      <SettingsDropdown />
      <UserDropdown class="nav-item" />
    </div>
  </header>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Grip, ChevronDown, Menu, Search, Bell, Sun, Moon, Bot } from 'lucide-vue-next'
import SprintaButton from '@/components/ui/SprintaButton.vue'
import axiosClient from '@/api/axiosClient'
import UserDropdown from '@/components/UserDropdown.vue'
import NotificationsDropdown from '@/components/NotificationsDropdown.vue'
import SettingsDropdown from '@/components/SettingsDropdown.vue'
import { useProjectStore } from '@/store/useProjectStore'
import { subscribeAdminRealtime } from '@/utils/adminRealtime'
import { getScopedCurrentProjectId, setScopedCurrentProjectId } from '@/utils/projectContext'
import { useI18nStore } from '@/store/useI18nStore'
import { toggleTheme, currentTheme } from '@/utils/theme'

const emit = defineEmits(['toggle-sidebar', 'toggle-ai', 'toggle-create'])

const handleGlobalCreate = () => {
  window.dispatchEvent(new CustomEvent('global-create-click'))
}

const router = useRouter()
const route = useRoute()
const projectStore = useProjectStore()
const i18nStore = useI18nStore()
const t = i18nStore.t

const isHomeContext = computed(() => route.path.startsWith('/home') || route.path.startsWith('/sites'))
const isSpaceContext = computed(() => route.path.startsWith('/space') || route.path.startsWith('/dashboard') || route.path.startsWith('/stickies') || route.path.startsWith('/rewards'))

const isModule = (moduleName) => {
  if (moduleName === 'people') {
    return route.path.includes('/home/people') || route.path.includes('/home/profile')
  }
  return route.path.includes(`/home/${moduleName}`)
}

const searchQuery = ref('')
const searchResults = ref([])
const searching = ref(false)
const searchWrapperRef = ref(null)
let searchTimer = null
let searchAbortController = null
let searchRequestId = 0

const currentProjectId = computed(() => route.params.id || getScopedCurrentProjectId() || '')
const activeProject = computed(() => projectStore.allProjects.find(project => project.id === currentProjectId.value) || projectStore.currentProject)
const workspaceName = computed(() => activeProject.value?.name || 'SprintA')
const workspaceBadge = computed(() => activeProject.value?.icon || workspaceName.value.charAt(0).toUpperCase())
const showSearchDropdown = computed(() => searchQuery.value.trim().length > 0 && (searching.value || searchResults.value.length > 0))

const runSearch = async () => {
  const keyword = searchQuery.value.trim()
  if (!keyword) {
    searchAbortController?.abort()
    searchResults.value = []
    searching.value = false
    return
  }

  searchAbortController?.abort()
  const controller = new AbortController()
  searchAbortController = controller
  const requestId = searchRequestId + 1
  searchRequestId = requestId
  searching.value = true

  if (isHomeContext.value) {
    // Global Home Search API is not available yet, using empty fallback
    setTimeout(() => {
      if (requestId !== searchRequestId) return
      searchResults.value = []
      searching.value = false
      searchAbortController = null
    }, 200)
    return
  }

  // Real Search for Space Project Context
  try {
    const response = await axiosClient.get('/worktasks', {
      params: { search: keyword },
      signal: controller.signal
    })
    if (requestId !== searchRequestId) {
      return
    }
    searchResults.value = response.data?.data || []
  } catch (error) {
    if (error?.name === 'CanceledError' || error?.code === 'ERR_CANCELED') {
      return
    }
    searchResults.value = []
  } finally {
    if (requestId === searchRequestId) {
      searching.value = false
      searchAbortController = null
    }
  }
}

const handleSearchInput = () => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(runSearch, 250)
}

const openSearchResult = (result) => {
  searchAbortController?.abort()
  searchResults.value = []
  searchQuery.value = ''

  if (isHomeContext.value) {
    // Navigate logic pending global search API implementation
    console.log('Global search item clicked:', result)
    return
  }

  router.push(`/space/${result.projectId}?task=${result.id}`)
}

const handleClickOutside = (e) => {
  if (searchWrapperRef.value && !searchWrapperRef.value.contains(e.target)) {
    searchResults.value = []
    searchQuery.value = ''
  }
}

const handleEscKey = (e) => {
  if (e.key === 'Escape') {
    searchResults.value = []
    searchQuery.value = ''
  }
}

const goToNotifications = () => {
  if (isHomeContext.value) {
    router.push('/home/notifications')
  }
}

watch(currentProjectId, (projectId) => {
  if (!projectId) {
    projectStore.clearProjectContext()
    return
  }

  if (isSpaceContext.value) {
    setScopedCurrentProjectId(projectId)
    projectStore.fetchProjectDetails(projectId, { force: true }).catch(() => {})
  }
}, { immediate: true })

onMounted(() => {
  if (isSpaceContext.value) {
    projectStore.fetchAllProjects(true).catch(() => {})
  }
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleEscKey)
})

let unsubscribeAdminRealtime = null

onMounted(() => {
  unsubscribeAdminRealtime = subscribeAdminRealtime(async ({ type, payload }) => {
    const activeProjectId = currentProjectId.value || null

    if (payload?.projectId && activeProjectId && `${payload.projectId}` !== `${activeProjectId}`) {
      await projectStore.fetchAllProjects(true).catch(() => {})
      return
    }

    if (
      [
        'project-settings-updated',
        'project-settings-favorite-updated',
        'project-settings-integrations-updated',
        'project-administration-updated',
        'project-settings-deleted'
      ].includes(type)
    ) {
      await projectStore.fetchAllProjects(true).catch(() => {})
      if (activeProjectId && type !== 'project-settings-deleted') {
        await projectStore.fetchProjectDetails(activeProjectId, { force: true }).catch(() => {})
      }
    }
  })
})

onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  searchAbortController?.abort()
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleEscKey)
  unsubscribeAdminRealtime?.()
})
</script>

<style scoped>
.app-topbar {
  height: 56px;
  background-color: var(--brand-navy);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid var(--brand-navy-deep);
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.app-launcher-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--brand-text-light);
  border-radius: var(--radius-sm);
}

.app-launcher-icon:hover {
  background-color: color-mix(in srgb, var(--brand-text-light) 20%, transparent);
}

.grid-icon {
  font-size: 18px;
  line-height: 1;
  letter-spacing: -2px;
}

.sprinta-logo {
  display: flex;
  align-items: center;
  gap: 0;
  margin-right: 16px;
  cursor: pointer;
}

.sprinta-logo-img {
  height: 32px;
  width: auto;
  object-fit: contain;
}

.logo-text {
  font-size: 20px;
  font-weight: bold;
  color: var(--brand-text-light);
  letter-spacing: -0.5px;
}

/* Home Site Nav */
.topbar-nav {
  display: flex;
  align-items: center;
  gap: 4px;
}

.topbar-link {
  padding: 6px 12px;
  color: var(--brand-text-light);
  text-decoration: none;
  font-weight: 500;
  font-size: 14px;
  border-radius: var(--radius-sm);
  transition: background-color 0.2s, color 0.2s;
}

.topbar-link:hover {
  background-color: color-mix(in srgb, var(--brand-text-light) 10%, transparent);
  color: var(--brand-text-light);
}

.topbar-link.active {
  color: var(--color-text-inverse, #FFFFFF);
  background-color: color-mix(in srgb, var(--brand-text-light) 10%, transparent);
}

/* Space Nav */
.space-nav {
  display: flex;
  align-items: center;
  gap: 8px;
}

.workspace-switcher {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: background 0.2s;
  color: var(--brand-text-light);
}

.workspace-switcher:hover {
  background: color-mix(in srgb, var(--brand-text-light) 20%, transparent);
  color: var(--color-text-inverse, #FFFFFF);
}

.ws-icon {
  width: 22px;
  height: 22px;
  border-radius: 2px;
  background: var(--color-accent, #0052cc);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
}

.ws-name {
  color: inherit;
  font-size: 14px;
  font-weight: 600;
  letter-spacing: -0.02em;
}

.menu-toggle {
  display: none;
  background: transparent;
  border: none;
  color: var(--brand-text-light);
  cursor: pointer;
  padding: 4px;
}

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  background-color: color-mix(in srgb, var(--brand-navy-deep) 50%, transparent);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  padding: 0 12px;
  width: 400px;
  height: 32px;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: var(--color-accent);
  background-color: var(--color-surface);
}

.search-input-wrapper:focus-within .search-icon {
  color: var(--color-text-muted);
}

.search-input-wrapper:focus-within input {
  color: var(--color-text-primary);
}


.search-icon { 
  color: var(--brand-text-light); 
  margin-right: 8px; 
}

.search-input-wrapper input { 
  background: transparent; 
  border: none; 
  color: var(--brand-text-light); 
  font-size: 14px; 
  width: 100%; 
  outline: none; 
.search-icon {
  color: #DEEBFF;
  font-size: 13px;
  margin-right: 8px;
}

.search-input-wrapper input {
  background: transparent;
  border: none;
  color: #DEEBFF;
  font-size: 14px;
  width: 100%;
  outline: none;

}

.search-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  background: var(--color-surface, #ffffff);
  border: 1px solid var(--color-border, #dfe1e6);
  border-radius: var(--radius-sm);
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.search-result,
.search-state {
  width: 100%;
  display: grid;
  gap: 4px;
  text-align: left;
  padding: 12px 14px;
  color: var(--color-text-primary, #172b4d);
}

.search-result {
  border: none;
  background: transparent;
  cursor: pointer;
  border-bottom: 1px solid var(--color-border, #f4f5f7);
}

.search-result:last-child {
  border-bottom: none;
}

.search-result:hover {
  background: var(--color-surface-hover, #f4f5f7);
}

.search-result span {
  color: var(--color-text-secondary, #42526e);
  font-size: 13px;
}

.search-result small,
.search-state {
  color: var(--color-text-muted, #6b778c);
  font-size: 11px;
}

.nav-right {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
}

.icon-btn {
  background: none;
  border: none;
  cursor: pointer;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--brand-text-light);
}

.icon-btn:hover {
  background-color: color-mix(in srgb, var(--brand-text-light) 10%, transparent);
}

@media (max-width: 1024px) {
  .menu-toggle { display: block; }
  .nav-center { display: none; }
}
</style>
