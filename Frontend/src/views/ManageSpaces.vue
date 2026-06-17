<template>
  <AdminLayout>
    <div class="manage-spaces-page" style="padding: 0;">
      <!-- Plane Style Header -->
          <header class="spaces-header">
            <div class="sh-left">
              <i class="fa-solid fa-briefcase"></i>
              <h1>Projects</h1>
            </div>

        <div class="sh-right">
          <div class="search-box">
             <i class="fa-solid fa-magnifying-glass"></i>
             <input type="text" placeholder="Search spaces..." v-model="searchQuery" />
          </div>
          <div style="display: flex; gap: 4px; border: 1px solid var(--color-border); padding: 4px; border-radius: 8px;">
            <button class="plane-btn-secondary outline-btn" style="border: none; margin: 0; padding: 6px 10px;" :class="{ active: viewMode === 'table' }" type="button" @click="setViewMode('table')" title="List view">
              <i class="fa-solid fa-list"></i>
            </button>
            <button class="plane-btn-secondary outline-btn" style="border: none; margin: 0; padding: 6px 10px;" :class="{ active: viewMode === 'grid' }" type="button" @click="setViewMode('grid')" title="Grid view">
              <i class="fa-solid fa-grip"></i>
            </button>
          </div>
          <button class="plane-btn-secondary outline-btn" type="button" @click="toggleSort">
             <i class="fa-solid fa-arrow-down-short-wide"></i> Created date {{ sortDirection === 'desc' ? '↓' : '↑' }}
          </button>
          <div class="project-filter-wrapper">
            <button class="plane-btn-secondary outline-btn" type="button" @click="showProjectFilters = !showProjectFilters" :class="{ active: showProjectFilters || visibilityFilter !== 'all' }">
               <i class="fa-solid fa-filter"></i> {{ filterLabel }}
            </button>
            <div class="project-filter-menu" v-if="showProjectFilters" @click.stop>
              <div class="filter-title">Visibility</div>
              <label class="filter-option"><input type="radio" value="all" v-model="visibilityFilter" /> All projects</label>
              <label class="filter-option"><input type="radio" value="Public" v-model="visibilityFilter" /> Public</label>
              <label class="filter-option"><input type="radio" value="Private" v-model="visibilityFilter" /> Private</label>
              <label class="filter-option"><input type="radio" value="starred" v-model="visibilityFilter" /> Starred</label>
              <button class="clear-filter-btn" type="button" @click="visibilityFilter = 'all'">Clear filters</button>
            </div>
          </div>
          <button class="plane-btn-primary" @click="isCreateModalVisible = true">
            Add Project
          </button>
        </div>
      </header>

      <section class="projects-scroll-panel">
      <div v-if="loading" class="loading-state">
         <i class="fa-solid fa-spinner fa-spin"></i> Loading projects...
      </div>
      <div v-else-if="filteredSpaces.length === 0" class="empty-state">
         <div class="empty-icon-wrap" style="width: 80px; height: 80px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 16px; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; box-shadow: 0 10px 30px rgba(0,0,0,0.5);">
           <i class="fa-solid fa-folder-open empty-icon" style="margin-bottom: 0;"></i>
         </div>
         <h3 class="empty-title" style="margin: 0 0 8px 0; font-size: 16px; font-weight: 600; color: var(--color-text-primary);">No projects found</h3>
         <p style="margin: 0 0 24px 0; font-size: 14px; color: var(--color-text-muted);">It looks like there are no projects here. Let's create your first one!</p>
         <button class="plane-btn-primary" @click="isCreateModalVisible = true">Create your first project</button>
      </div>
      <div v-else>
        <div v-if="viewMode === 'grid'" class="spaces-grid">
          <div class="project-card" v-for="(space, index) in filteredSpaces" :key="space.id" @click="goToSpace(space.id)">
            <!-- Cover Image Mock -->
            <div class="card-cover" :style="{ background: space.cover || coverGradients[index % coverGradients.length] }">
               <div class="card-actions-top" @click.stop>
                 <button class="card-icon-btn" type="button" @click="copySpaceLink(space)"><i class="fa-solid fa-link"></i></button>
                 <button class="card-icon-btn" type="button" :class="{ 'starred': space.starred }" @click="toggleStar(space)"><i :class="space.starred ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i></button>
               </div>
            </div>

            <div class="card-body">
              <!-- Floating Project Icon -->
              <div class="floating-icon">
                <span class="emoji">{{ space.icon || emojiList[index % emojiList.length] || '👇' }}</span>
              </div>

              <div class="proj-title-row">
                 <h3>{{ space.name }}</h3>
                 <span class="proj-key">{{ space.key }}</span>
              </div>

              <p class="proj-desc">
                {{ space.originalRow?.description || 'Welcome to this Project! This project throws you into the driver\'s seat of work management. Through curated work items, you\'ll uncover key features...' }}
              </p>

              <div class="card-footer" @click.stop>
                 <span class="visibility-pill" :class="space.networkType?.toLowerCase()">
                   <i :class="space.networkType === 'Private' ? 'fa-solid fa-lock' : 'fa-solid fa-globe'"></i>
                   {{ space.networkType || 'Public' }}
                 </span>
                 <span style="font-size: 11px; color: var(--color-text-muted); margin-left: auto; margin-right: 8px;">
                   Created: {{ new Date(space.originalRow?.createdAt || space.originalRow?.createdDate || Date.now()).toLocaleDateString() }}
                 </span>
                 <el-dropdown trigger="click" v-if="showProjectSettingsButton(space)" @click.stop>
                   <button class="card-icon-btn" type="button"><i class="fa-solid fa-ellipsis"></i></button>
                   <template #dropdown>
                     <el-dropdown-menu class="plane-dropdown">
                       <el-dropdown-item @click="goToAdmin(space)"><i class="fa-solid fa-gear" style="margin-right: 8px;"></i> Settings</el-dropdown-item>
                       <el-dropdown-item @click="archiveProject(space)"><i class="fa-solid fa-box-archive" style="margin-right: 8px;"></i> Archive project</el-dropdown-item>
                     </el-dropdown-menu>
                   </template>
                 </el-dropdown>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="spaces-table-container">
          <table class="jira-table spaces-table" style="width: 100%; border-collapse: collapse; text-align: left;">
            <thead>
              <tr style="border-bottom: 2px solid var(--color-border); color: var(--color-text-muted); font-size: 12px;">
                <th style="padding: 12px 16px; width: 40px;"></th>
                <th style="padding: 12px 16px;">Name</th>
                <th style="padding: 12px 16px;">Key</th>
                <th style="padding: 12px 16px;">Type</th>
                <th style="padding: 12px 16px;">Lead</th>
                <th style="padding: 12px 16px;">Created</th>
                <th style="padding: 12px 16px; width: 50px;"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(space, index) in filteredSpaces" :key="'table-' + space.id" @click="goToSpace(space.id)" style="border-bottom: 1px solid var(--color-border); cursor: pointer; transition: background 0.2s;" class="table-row-hover">
                <td style="padding: 12px 16px;" @click.stop>
                  <button class="card-icon-btn transparent-btn" style="background: transparent; border: none; color: var(--color-text-muted);" :class="{ 'starred': space.starred }" @click="toggleStar(space)">
                    <i :class="space.starred ? 'fa-solid fa-star' : 'fa-regular fa-star'" :style="{ color: space.starred ? '#EAB308' : '' }"></i>
                  </button>
                </td>
                <td style="padding: 12px 16px;">
                  <div style="display: flex; align-items: center; gap: 12px;">
                    <div style="width: 24px; height: 24px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 12px;" :style="{ background: space.cover || coverGradients[index % coverGradients.length] }">
                      {{ space.icon || emojiList[index % emojiList.length] || '📦' }}
                    </div>
                    <span style="font-weight: 500; color: #3b82f6;">{{ space.name }}</span>
                  </div>
                </td>
                <td style="padding: 12px 16px; font-size: 13px;">{{ space.key }}</td>
                <td style="padding: 12px 16px; font-size: 13px; color: var(--color-text-muted);">
                  {{ space.networkType === 'Private' ? 'Team-managed software (Private)' : 'Team-managed software' }}
                </td>
                <td style="padding: 12px 16px;">
                  <div style="display: flex; align-items: center; gap: 8px;">
                    <div style="width: 24px; height: 24px; border-radius: 50%; background: #10B981; color: white; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 600;">
                      {{ space.leadName?.charAt(0).toUpperCase() || 'T' }}
                    </div>
                    <span style="font-size: 13px;">{{ space.leadName }}</span>
                  </div>
                </td>
                <td style="padding: 12px 16px; font-size: 13px; color: var(--color-text-muted);">
                  {{ new Date(space.originalRow?.createdAt || space.originalRow?.createdDate || Date.now()).toLocaleDateString() }}
                </td>
                <td style="padding: 12px 16px;" @click.stop>
                  <el-dropdown trigger="click" v-if="showProjectSettingsButton(space)">
                    <button class="card-icon-btn transparent-btn" style="background: transparent; border: none; font-size: 16px; color: var(--color-text-muted);"><i class="fa-solid fa-ellipsis"></i></button>
                    <template #dropdown>
                      <el-dropdown-menu class="plane-dropdown">
                        <el-dropdown-item @click="goToAdmin(space)"><i class="fa-solid fa-gear" style="margin-right: 8px;"></i> Settings</el-dropdown-item>
                        <el-dropdown-item @click="archiveProject(space)"><i class="fa-solid fa-box-archive" style="margin-right: 8px;"></i> Archive project</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      </section>

      <CreateSpaceModal v-model:visible="isCreateModalVisible" @created="fetchSpaces" />
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import CreateSpaceModal from '@/components/CreateSpaceModal.vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useProjectStore } from '@/store/useProjectStore'
import { canAccessProjectSettings, getProjectSettingsDeniedMessage, getStoredUser } from '@/utils/permissions'
import { subscribeAdminRealtime } from '@/utils/adminRealtime'
import { getProjectSettingsWindowName, openNamedAppWindow, PROJECT_ADMIN_WINDOW_NAME } from '@/utils/windowTabs'

const router = useRouter()
const handleSwitchSettings = (path) => {
  router.push(path)
}

const projectStore = useProjectStore()
const loading = ref(false)
const spaces = ref([])
const searchQuery = ref('')
const sortDirection = ref('desc')
const showProjectFilters = ref(false)
const visibilityFilter = ref('all')
const isCreateModalVisible = ref(false)
const viewMode = ref(localStorage.getItem('spaces_view_mode') || 'table')

const setViewMode = (mode) => {
  viewMode.value = mode
  localStorage.setItem('spaces_view_mode', mode)
}

const currentUser = computed(() => getStoredUser())
const canManageSpace = (space) => canAccessProjectSettings(space, currentUser.value)
const showProjectSettingsButton = (space) => canManageSpace(space)

const goToAdmin = (space) => {
  if (!canManageSpace(space)) {
    ElMessage.warning(getProjectSettingsDeniedMessage())
    return
  }
  const routeData = router.resolve(`/space/${space.id}/settings`)
  openNamedAppWindow(routeData.href, getProjectSettingsWindowName(space.id))
}

const archiveProject = async (space) => {
  try {
    await ElMessageBox.confirm(`Are you sure you want to archive project "${space.name}"?`, 'Archive Project', { type: 'warning' })
    await axiosClient.put(`/projects/${space.id}/archive`)
    ElMessage.success('Project archived')
    fetchSpaces()
  } catch (err) {
    if (err !== 'cancel') ElMessage.error('Failed to archive project')
  }
}

const toggleSort = () => {
  sortDirection.value = sortDirection.value === 'desc' ? 'asc' : 'desc'
}

const copySpaceLink = async (space) => {
  const url = `${window.location.origin}/space/${space.id}`
  try {
    await navigator.clipboard.writeText(url)
    ElMessage.success('Project link copied')
  } catch (error) {
    ElMessage.info(url)
  }
}

const toggleStar = async (space) => {
  const nextFavorite = !space.starred
  space.starred = nextFavorite
  try {
    await projectStore.updateFavorite(space.id, nextFavorite)
    ElMessage.success(nextFavorite ? 'Project starred' : 'Project unstarred')
  } catch (error) {
    space.starred = !nextFavorite
    ElMessage.error('Could not update favorite project')
  }
}

const coverGradients = [
  'linear-gradient(135deg, #1f0b0f 0%, #761d28 40%, #1e1215 100%)',
  'linear-gradient(135deg, #0f172a 0%, #1e40af 50%, #172554 100%)',
  'linear-gradient(135deg, #064e3b 0%, #059669 40%, #022c22 100%)',
  'linear-gradient(135deg, #4c1d95 0%, #7c3aed 50%, #2e1065 100%)'
]

const emojiList = ['👇', '🚀', '⚡', '💡', '🔥', '🎯']

const fetchSpaces = async () => {
  loading.value = true
  try {
    const response = await axiosClient.get('/projects/discovery')
    const data = response.data.data || response.data || []

    // Transform data
    spaces.value = data.map(p => ({
      id: p.id,
      starred: Boolean(p.isFavorite),
      name: p.name,
      key: p.key || p.identifier || p.name.substring(0, 4).toUpperCase(),
      myRole: p.myRole || p.MyRole || null,
      projectRole: p.projectRole || p.ProjectRole || null,
      leadName: p.leadName || p.reporterName || 'Admin',
      cover: p.cover,
      icon: p.icon,
      networkType: p.networkType || 'Public',
      originalRow: p
    }))
  } catch (error) {
    console.error('Fetch spaces error:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchSpaces()
})

let unsubscribeAdminRealtime = null

onMounted(() => {
  unsubscribeAdminRealtime = subscribeAdminRealtime(async ({ type }) => {
    if (
      [
        'project-settings-updated',
        'project-settings-favorite-updated',
        'project-settings-integrations-updated',
        'project-administration-updated',
        'project-settings-deleted'
      ].includes(type)
    ) {
      await fetchSpaces()
      await projectStore.fetchAllProjects(true).catch(() => {})
    }
  })
})

onUnmounted(() => {
  unsubscribeAdminRealtime?.()
})

const filteredSpaces = computed(() => {
  return spaces.value
    .filter(s => {
      const matchesSearch = !searchQuery.value || s.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || s.key.toLowerCase().includes(searchQuery.value.toLowerCase())
      const matchesVisibility =
        visibilityFilter.value === 'all' ||
        (visibilityFilter.value === 'starred' && s.starred) ||
        s.networkType === visibilityFilter.value
      return matchesSearch && matchesVisibility
    })
    .sort((a, b) => {
      const left = new Date(a.originalRow?.createdAt || a.originalRow?.createdDate || 0).getTime()
      const right = new Date(b.originalRow?.createdAt || b.originalRow?.createdDate || 0).getTime()
      return sortDirection.value === 'desc' ? right - left : left - right
    })
})

const goToSpace = (id) => {
  router.push(`/space/${id}`)
}

const filterLabel = computed(() => ({
  all: 'Filters',
  Public: 'Public',
  Private: 'Private',
  starred: 'Starred'
}[visibilityFilter.value] || 'Filters'))
</script>

<style scoped>
.manage-spaces-layout {
  display: flex;
  height: 100vh;
  width: 100%;
  background: var(--color-bg);
  overflow: hidden;
}

/* Jira Settings Sidebar */
.jira-admin-sidebar {
  width: 240px;
  border-right: 1px solid var(--color-border);
  background: var(--color-bg);
  padding: 24px 16px;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  font-family: 'Inter', sans-serif;
}

.sidebar-header {
  margin-bottom: 24px;
}

.back-link {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--color-text-muted);
  text-decoration: none;
  font-size: 13px;
  font-weight: 500;
  transition: color 0.2s;
}
.back-link:hover {
  color: var(--color-text-primary);
}

.sidebar-section {
  margin-bottom: 20px;
}

.section-title {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  margin-bottom: 8px;
  letter-spacing: 0.5px;
}

.section-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 13px;
  color: var(--color-text-primary);
  font-weight: 600;
}
.section-item.active {
  background: #18181b;
  border: 1px solid var(--color-border);
}

.sidebar-menu {
  list-style: none;
  padding: 0 0 0 12px;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
  border-left: 1px solid var(--color-border);
}

.menu-item {
  display: block;
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 13px;
  color: var(--color-text-secondary);
  text-decoration: none;
  transition: all 0.2s;
}
.menu-item:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}
.menu-item.active {
  background: color-mix(in srgb, var(--color-accent) 10%, transparent);
  color: var(--color-accent);
  font-weight: 600;
}

/* Right Content */
.spaces-main-content {
  flex: 1;
  overflow-y: auto;
  min-width: 0;
}

.manage-spaces-page {
  padding: 40px;
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  color: var(--color-text-primary);
  font-family: 'Inter', -apple-system, sans-serif;
  min-height: 0;
}

/* Header */
.spaces-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
  margin-bottom: 24px;
  flex-shrink: 0;
}

.sh-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.sh-left i {
  color: var(--color-text-muted);
  font-size: 18px;
}
.sh-left h1 {
  font-size: 16px;
  font-weight: 500;
  margin: 0;
  color: var(--color-text-primary);
}

.sh-right {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}
.search-box i {
  position: absolute;
  left: 12px;
  color: var(--color-text-muted);
  font-size: 13px;
}
.search-box input {
  background: transparent;
  border: none;
  color: var(--color-text-primary);
  padding: 6px 12px 6px 32px;
  font-size: 13px;
  outline: none;
  width: 180px;
  transition: width 0.2s;
}
.search-box input:focus { width: 240px; }
.search-box input::placeholder { color: #52525B; }

.plane-btn-secondary.outline-btn {
  background: transparent;
  border: 1px solid transparent;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}
.plane-btn-secondary.outline-btn:hover { background: #18181B; color: var(--color-text-primary); }
.plane-btn-secondary.outline-btn.active { background: #18181B; color: var(--color-text-primary); border-color: var(--color-border); }

.project-filter-wrapper { position: relative; }
.project-filter-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  z-index: 20;
  width: 220px;
  background: #1B1C20;
  border: 1px solid #2D2F36;
  border-radius: 8px;
  padding: 12px;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.35);
}
.filter-title { color: var(--color-text-muted); font-size: 12px; font-weight: 600; margin-bottom: 8px; }
.filter-option {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #D4D4D8;
  font-size: 13px;
  padding: 6px 0;
  cursor: pointer;
}
.clear-filter-btn {
  width: 100%;
  margin-top: 8px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
  color: #D4D4D8;
  padding: 7px;
  cursor: pointer;
}
.clear-filter-btn:hover { background: var(--color-border); }

.plane-btn-primary {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 14px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}
.plane-btn-primary:hover { background: #0284C7; }

.projects-scroll-panel {
  min-height: 0;
  overflow-y: auto;
  padding: 2px 8px 24px 2px;
  scrollbar-width: thin;
  scrollbar-color: #3f3f46 transparent;
}

.projects-scroll-panel::-webkit-scrollbar {
  width: 8px;
}

.projects-scroll-panel::-webkit-scrollbar-thumb {
  background: #3f3f46;
  border-radius: 999px;
}

.spaces-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  align-items: stretch;
}

/* Card */
.project-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  flex-direction: column;
  min-height: 300px;
}
.project-card:hover {
  border-color: #3F3F46;
  transform: translateY(-2px);
}

.card-cover {
  height: 120px;
  position: relative;
  display: flex;
  justify-content: flex-end;
  padding: 12px;
}

.card-actions-top {
  display: flex;
  gap: 8px;
}

.card-icon-btn {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  background: rgba(0,0,0,0.3);
  border: 1px solid rgba(255,255,255,0.1);
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
  backdrop-filter: blur(4px);
}
.card-icon-btn:hover { background: rgba(0,0,0,0.5); color: var(--color-text-primary); }
.card-icon-btn.starred { color: #EAB308; }
.card-icon-btn:disabled { opacity: 0.45; cursor: not-allowed; }

.card-body {
  padding: 0 20px 20px 20px;
  position: relative;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.floating-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  background: var(--color-border);
  border: 4px solid var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: -18px;
  margin-bottom: 12px;
  font-size: 18px;
}

.proj-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.proj-title-row h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--color-text-primary);
}
.proj-key {
  font-size: 11px;
  color: var(--color-text-muted);
  font-weight: 600;
  margin-top: 2px;
}

.proj-desc {
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.5;
  margin: 0 0 20px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  flex: 1;
}

.card-footer {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  gap: 10px;
  margin-top: auto;
}

.visibility-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 24px;
  padding: 0 8px;
  border-radius: 6px;
  background: var(--color-border);
  color: #d4d4d8;
  font-size: 11px;
  font-weight: 600;
}

.visibility-pill.private {
  color: #fca5a5;
}

.avatar-group {
  display: flex;
}
.avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #10B981;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
  border: 2px solid var(--color-surface);
}

.card-footer .card-icon-btn {
  background: transparent;
  border: none;
  font-size: 14px;
  margin-left: auto;
}
.card-footer .card-icon-btn:hover { background: var(--color-border); }

.loading-state, .empty-state { text-align: center; margin-top: 60px; color: var(--color-text-muted); }
.empty-icon { font-size: 48px; color: #3F3F46; margin-bottom: 16px; }
.empty-state p { margin-bottom: 24px; }

@media (max-width: 900px) {
  .manage-spaces-page {
    padding: 24px;
  }

  .spaces-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .sh-right {
    width: 100%;
    justify-content: flex-start;
  }

  .search-box input,
  .search-box input:focus {
    width: 180px;
  }
}

.table-row-hover:hover {
  background: var(--color-surface);
}

.spaces-table-container {
  overflow-x: auto;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
}

.switch-trigger-btn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  cursor: pointer;
  background: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  color: var(--color-text-primary) !important;
  padding: 8px 12px;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 600;
  transition: all 0.2s;
}

.switch-trigger-btn:hover {
  background: var(--color-surface-hover) !important;
}

.switch-trigger-btn i {
  color: var(--color-text-secondary) !important;
}

.switch-trigger-btn span {
  color: var(--color-text-primary) !important;
}
</style>

<style>
.el-popper.jira-switch-dropdown-popper {
  background: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  padding: 4px 0 !important;
  z-index: 100002 !important;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2) !important;
}

.jira-switch-dropdown-menu .el-dropdown-menu__item {
  color: var(--color-text-primary) !important;
  display: flex !important;
  align-items: center !important;
  gap: 8px !important;
  font-size: 13px !important;
  padding: 8px 16px !important;
}

.jira-switch-dropdown-menu .el-dropdown-menu__item:hover {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary) !important;
}

.jira-switch-dropdown-menu .el-dropdown-menu__item.is-disabled {
  color: var(--color-accent) !important;
  font-weight: 600 !important;
  background: transparent !important;
  cursor: default !important;
}
</style>
