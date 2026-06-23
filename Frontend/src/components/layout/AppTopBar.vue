<template>
  <header class="app-topbar">
    <div class="nav-left">
      <div class="app-launcher-icon">
        <span class="grid-icon">⋮⋮⋮</span>
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
          <i class="fa-solid fa-chevron-down ms-1"></i>
        </div>
        <button class="menu-toggle" @click="emit('toggle-sidebar')">
          <i class="fa-solid fa-bars-staggered"></i>
        </button>
      </div>
    </div>

    <div class="nav-center" ref="searchWrapperRef">
      <div class="search-input-wrapper">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" :placeholder="isSpaceContext ? t('Search work items...') : t('Search')" v-model="searchQuery" @input="handleSearchInput" />
        <div v-if="showSearchDropdown" class="search-dropdown">
          <div v-if="searching" class="search-state">{{ t('Searching...') }}</div>
          <template v-else-if="searchResults.length">
            <button v-for="result in searchResults" :key="result.id" type="button" class="search-result" @click="openSearchResult(result)">
              <strong>{{ result.sequenceId || result.title || result.name }}</strong>
              <span>{{ result.title || result.description || result.type }}</span>
              <small v-if="result.projectName">{{ result.projectName }}</small>
            </button>
          </template>
          <div v-else class="search-state">{{ t('No items found.') }}</div>
        </div>
      </div>
      <button v-if="isHomeContext" class="topbar-btn create-btn" @click="handleGlobalCreate">{{ t('Create', 'Tạo') }}</button>
    </div>

    <div class="nav-right">
      <!-- Language Flags -->
      <div class="lang-selector">
        <button class="flag-btn" :class="{ active: i18nStore.locale === 'vi' }" @click="i18nStore.setLocale('vi')" title="Tiếng Việt">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 3 2" class="flag-svg">
            <rect width="3" height="2" fill="#da251d"/>
            <polygon points="1.5,0.4 1.62,0.85 2.1,0.85 1.71,1.13 1.86,1.6 1.5,1.31 1.14,1.6 1.29,1.13 0.9,0.85 1.38,0.85" fill="#ffff00"/>
          </svg>
        </button>
        <button class="flag-btn" :class="{ active: i18nStore.locale === 'en' }" @click="i18nStore.setLocale('en')" title="English">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 300" class="flag-svg">
            <clipPath id="australia-jack">
              <path d="M0 0h300v150H0z"/>
            </clipPath>
            <rect width="600" height="300" fill="#00008b"/>
            <g clip-path="url(#australia-jack)">
              <path d="M0 0l300 150M300 0L0 150" stroke="#fff" stroke-width="30"/>
              <path d="M0 0l300 150M300 0L0 150" stroke="#ff0000" stroke-width="20"/>
              <path d="M150 0v150M0 75h300" stroke="#fff" stroke-width="50"/>
              <path d="M150 0v150M0 75h300" stroke="#ff0000" stroke-width="30"/>
            </g>
            <g fill="#fff">
              <path d="M150 205l5 15 15-5-10 13 13 10-16 1-3 16-3-16-16-1 13-10-10-13 15 5z"/>
              <path d="M450 225l2.5 7.5 7.5-2.5-5 6.5 6.5 5-8 0.5-1.5 8-1.5-8-8-0.5 6.5-5-5-6.5 7.5 2.5z"/>
              <path d="M380 125l2.5 7.5 7.5-2.5-5 6.5 6.5 5-8 0.5-1.5 8-1.5-8-8-0.5 6.5-5-5-6.5 7.5 2.5z"/>
              <path d="M450 55l2.5 7.5 7.5-2.5-5 6.5 6.5 5-8 0.5-1.5 8-1.5-8-8-0.5 6.5-5-5-6.5 7.5 2.5z"/>
              <path d="M510 115l2.5 7.5 7.5-2.5-5 6.5 6.5 5-8 0.5-1.5 8-1.5-8-8-0.5 6.5-5-5-6.5 7.5 2.5z"/>
              <polygon points="475,145 477,151 483,151 478,155 480,161 475,157 470,161 472,155 467,151 473,151"/>
            </g>
          </svg>
        </button>
      </div>

      <NotificationsDropdown v-if="isSpaceContext" />
      <button class="icon-btn" @click="goToNotifications" v-else>
        <i class="fa-regular fa-bell"></i>
      </button>

      <button class="icon-btn" @click="toggleTheme()" :title="currentTheme === 'dark' ? t('Light mode') : t('Dark mode')">
        <i :class="currentTheme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon'"></i>
      </button>

      <button v-if="isSpaceContext" class="icon-btn" @click="emit('toggle-ai')">
        <i class="fa-solid fa-robot"></i>
      </button>

      <SettingsDropdown />
      <UserDropdown class="nav-item" />
    </div>
  </header>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
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
const t = (key) => i18nStore.t(key)

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
  background-color: #172B4D;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid #091E42;
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
  color: #DEEBFF;
  border-radius: 3px;
}

.app-launcher-icon:hover {
  background-color: rgba(255, 255, 255, 0.2);
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
  height: 24px;
  width: auto;
  object-fit: contain;
  transform: scale(4);
  margin-right: 12px;
  margin-left: 8px;
}

.logo-text {
  font-size: 20px;
  font-weight: bold;
  color: #FFFFFF;
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
  color: #DEEBFF;
  text-decoration: none;
  font-weight: 500;
  font-size: 14px;
  border-radius: 3px;
  transition: background-color 0.2s, color 0.2s;
}

.topbar-link:hover {
  background-color: rgba(255, 255, 255, 0.2);
  color: #FFFFFF;
}

.topbar-link.active {
  color: #FFFFFF;
  background-color: rgba(255, 255, 255, 0.1);
}

.create-btn {
  background-color: #0052cc;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  margin-left: 8px;
  cursor: pointer;
}

.create-btn:hover {
  background-color: #0047b3;
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
  border-radius: 3px;
  cursor: pointer;
  transition: background 0.2s;
  color: #DEEBFF;
}

.workspace-switcher:hover {
  background: rgba(255, 255, 255, 0.2);
  color: #FFFFFF;
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
  color: #DEEBFF;
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
  background-color: rgba(9, 30, 66, 0.48);
  border: 1px solid transparent;
  border-radius: 3px;
  padding: 0 12px;
  width: 400px;
  height: 32px;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: #4c9aff;
  background-color: #ffffff;
}

.search-input-wrapper:focus-within .search-icon {
  color: #6b778c;
}

.search-input-wrapper:focus-within input {
  color: #172b4d;
}

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
  border-radius: 3px;
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
  font-size: 16px;
  cursor: pointer;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #DEEBFF;
}

.icon-btn:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

@media (max-width: 1024px) {
  .menu-toggle { display: block; }
  .nav-center { display: none; }
}

.lang-selector {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-right: 4px;
}

.flag-btn {
  background: none;
  border: 1px solid transparent;
  padding: 2px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: all 0.2s ease;
  opacity: 0.6;
}

.flag-btn:hover {
  opacity: 1;
  background-color: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.2);
}

.flag-btn.active {
  opacity: 1;
  border-color: #38bdf8;
  background-color: rgba(56, 189, 248, 0.1);
}

.flag-svg {
  width: 24px;
  height: 16px;
  border-radius: 2px;
  display: block;
  object-fit: cover;
}
</style>
