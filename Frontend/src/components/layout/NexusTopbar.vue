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

    <div class="nav-center">
      <div class="search-input-wrapper">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" placeholder="Search work items..." v-model="searchQuery" @input="handleSearchInput" />
        <div v-if="showSearchDropdown" class="search-dropdown">
          <div v-if="searching" class="search-state">Searching...</div>
          <template v-else-if="searchResults.length">
            <button v-for="result in searchResults" :key="result.id" type="button" class="search-result" @click="openSearchResult(result)">
              <strong>{{ result.sequenceId || result.title }}</strong>
              <span>{{ result.title }}</span>
              <small>{{ result.projectName }}</small>
            </button>
          </template>
          <div v-else class="search-state">No work items found.</div>
        </div>
      </div>
    </div>

    <div class="nav-right">
      <NotificationsDropdown />
      <div class="help-btn" @click="$emit('toggle-ai')">
         <i class="fa-solid fa-robot"></i>
      </div>
      <UserDropdown class="nav-item" />
    </div>
  </header>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import UserDropdown from '@/components/UserDropdown.vue'
import NotificationsDropdown from '@/components/NotificationsDropdown.vue'
import { useProjectStore } from '@/store/useProjectStore'

const router = useRouter()
const route = useRoute()
const projectStore = useProjectStore()
const searchQuery = ref('')
const searchResults = ref([])
const searching = ref(false)
let searchTimer = null
let searchAbortController = null
let searchRequestId = 0

const currentProjectId = computed(() => route.params.id || localStorage.getItem('currentProjectId') || '')
const activeProject = computed(() => projectStore.allProjects.find(project => project.id === currentProjectId.value) || projectStore.currentProject)
const workspaceName = computed(() => activeProject.value?.name || 'SprintA')
const workspaceBadge = computed(() => activeProject.value?.icon || workspaceName.value.charAt(0).toUpperCase())
const showSearchDropdown = computed(() => searching.value || searchResults.value.length > 0 || searchQuery.value.trim().length > 0)

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

onMounted(() => {
  projectStore.fetchAllProjects().catch(() => {})
})

watch(currentProjectId, (projectId) => {
  if (!projectId) {
    projectStore.clearProjectContext()
    return
  }

  localStorage.setItem('currentProjectId', projectId)
  projectStore.fetchProjectDetails(projectId).catch(() => {})
}, { immediate: true })

onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  searchAbortController?.abort()
})
</script>

<style scoped>
.plane-topbar {
  height: 52px;
  background-color: #0d0f11;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid #1e2025;
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
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.2s;
}

.workspace-switcher:hover {
  background: #1e2025;
}

.ws-icon {
  width: 20px;
  height: 20px;
  border-radius: 4px;
  background: #0ea5e9;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
}

.ws-name {
  color: #e4e4e7;
  font-size: 14px;
  font-weight: 500;
}

.workspace-switcher i {
  color: #71717a;
  font-size: 10px;
  margin-left: 2px;
}

.menu-toggle {
  display: none;
  background: transparent;
  border: none;
  color: #a1a1aa;
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
  background-color: #1e2025;
  border: 1px solid #27272a;
  border-radius: 6px;
  padding: 0 12px;
  width: 480px;
  height: 32px;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: #3f3f46;
  background-color: #181a1f;
}

.search-icon { 
  color: #71717a; 
  font-size: 13px; 
  margin-right: 8px; 
}

.search-input-wrapper input { 
  background: transparent; 
  border: none; 
  color: #e4e4e7; 
  font-size: 13px; 
  width: 100%; 
  outline: none; 
}

.search-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  background: #111315;
  border: 1px solid #27272a;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 16px 36px rgba(0, 0, 0, 0.35);
}

.search-result,
.search-state {
  width: 100%;
  display: grid;
  gap: 4px;
  text-align: left;
  padding: 12px 14px;
  color: #e4e4e7;
}

.search-result {
  border: none;
  background: transparent;
  cursor: pointer;
}

.search-result:hover {
  background: #18181b;
}

.search-result span,
.search-result small,
.search-state {
  color: #a1a1aa;
}

.nav-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.help-btn {
  color: #a1a1aa;
  font-size: 15px;
  cursor: pointer;
}
.help-btn:hover {
  color: #e4e4e7;
}

@media (max-width: 1024px) {
  .menu-toggle { display: block; }
  .nav-center { display: none; }
}
</style>
