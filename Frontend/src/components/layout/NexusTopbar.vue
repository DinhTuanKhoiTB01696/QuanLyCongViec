<template>
  <header class="plane-topbar">
    <div class="nav-left">
      <div class="workspace-switcher" @click="$router.push('/spaces')">
        <div class="ws-icon">{{ workspaceBadge }}</div>
        <span class="ws-name">{{ workspaceName }}</span>
        <i class="fa-solid fa-chevron-down"></i>
      </div>

      <button class="menu-toggle" @click="$emit('toggle-sidebar')">
        <i class="fa-solid fa-bars-staggered"></i>
      </button>
    </div>

    <div class="nav-center" ref="searchWrapperRef">
      <div class="search-input-wrapper">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" :placeholder="t('shell.searchPlaceholder')" v-model="searchQuery" @input="handleSearchInput" />
        <div v-if="showSearchDropdown" class="search-dropdown">
          <div v-if="searching" class="search-state">{{ t('shell.searching') }}</div>
          <template v-else-if="searchResults.length">
            <button v-for="result in searchResults" :key="result.id" type="button" class="search-result" @click="openSearchResult(result)">
              <strong>{{ result.sequenceId || result.title }}</strong>
              <span>{{ result.title }}</span>
              <small>{{ result.projectName }}</small>
            </button>
          </template>
          <div v-else class="search-state">{{ t('shell.searchNoResults') }}</div>
        </div>
      </div>
    </div>

    <div class="nav-right">
      <NotificationsDropdown />
      <button class="theme-toggle" @click="toggleTheme()" :title="currentTheme === 'dark' ? 'Chuyển sang sáng' : 'Chuyển sang tối'">
        <i :class="currentTheme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon'"></i>
      </button>
      <div class="help-btn" @click="$emit('toggle-ai')">
         <i class="fa-solid fa-robot"></i>
      </div>
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
import { toggleTheme, currentTheme } from '@/utils/theme'
import { subscribeAdminRealtime } from '@/utils/adminRealtime'
import { getScopedCurrentProjectId, setScopedCurrentProjectId } from '@/utils/projectContext'
import { useI18n } from '@/composables/useI18n'

const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const projectStore = useProjectStore()
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

watch(currentProjectId, (projectId) => {
  if (!projectId) {
    projectStore.clearProjectContext()
    return
  }

  setScopedCurrentProjectId(projectId)
  projectStore.fetchProjectDetails(projectId, { force: true }).catch(() => {})
}, { immediate: true })

onMounted(() => {
  projectStore.fetchAllProjects(true).catch(() => {})
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
.plane-topbar {
  height: 52px;
  background-color: var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid var(--color-border);
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 250px;
}

.workspace-switcher {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  border-radius: 2px;
  cursor: pointer;
  transition: background 0.2s;
}

.workspace-switcher:hover {
  background: var(--color-surface-hover);
}

.ws-icon {
  width: 22px;
  height: 22px;
  border-radius: 2px;
  background: var(--color-accent);
  color: var(--color-text-inverse);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
}

.ws-name {
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 850; /* Extra bold for prominence */
  letter-spacing: -0.02em;
}

.workspace-switcher i {
  color: var(--color-text-muted);
  font-size: 10px;
  margin-left: 2px;
}

.menu-toggle {
  display: none;
  background: transparent;
  border: none;
  color: var(--color-text-muted);
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
  background-color: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 0 12px;
  width: 480px;
  height: 32px;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: var(--color-accent);
  background-color: var(--color-surface);
}

.search-icon { 
  color: var(--color-text-muted); 
  font-size: 13px; 
  margin-right: 8px; 
}

.search-input-wrapper input { 
  background: transparent; 
  border: none; 
  color: var(--color-text-primary); 
  font-size: 13px; 
  width: 100%; 
  outline: none; 
}

.search-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  overflow: hidden;
  box-shadow: var(--shadow-md);
}

.search-result,
.search-state {
  width: 100%;
  display: grid;
  gap: 4px;
  text-align: left;
  padding: 12px 14px;
  color: var(--color-text-primary);
}

.search-result {
  border: none;
  background: transparent;
  cursor: pointer;
}

.search-result:hover {
  background: var(--color-surface-hover);
}

.search-result span {
  color: var(--color-text-secondary);
  font-size: 13px;
}

.search-result small,
.search-state {
  color: var(--color-text-muted);
  font-size: 11px;
}

.nav-right {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 16px;
  width: 250px;
}

.help-btn {
  color: var(--color-text-muted);
  font-size: 15px;
  cursor: pointer;
}
.help-btn:hover {
  color: var(--color-text-primary);
}

.theme-toggle {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  font-size: 15px;
  cursor: pointer;
  padding: 4px 6px;
  border-radius: 2px;
  transition: color 0.2s, background 0.2s;
  display: flex;
  align-items: center;
}
.theme-toggle:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

@media (max-width: 1024px) {
  .menu-toggle { display: block; }
  .nav-center { display: none; }
}
</style>
