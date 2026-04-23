<template>
  <NexusLayout>
    <div class="manage-spaces-page">
      <!-- Plane Style Header -->
      <header class="spaces-header">
        <div class="sh-left">
          <i class="fa-solid fa-briefcase"></i>
          <h1>Projects</h1>
        </div>
        
        <div class="sh-right">
          <div class="search-box">
             <i class="fa-solid fa-magnifying-glass"></i>
             <input type="text" placeholder="Search commands..." v-model="searchQuery" />
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
         <div class="empty-icon"><i class="fa-regular fa-folder-open"></i></div>
         <p>No projects found.</p>
         <button class="plane-btn-primary" @click="isCreateModalVisible = true">Create your first project</button>
      </div>
      <div v-else class="spaces-grid">
        <div class="project-card" v-for="(space, index) in filteredSpaces" :key="space.id" @click="goToSpace(space.id)">
          <!-- Cover Image Mock -->
          <div class="card-cover" :style="{ background: space.cover || coverGradients[index % coverGradients.length] }">
             <div class="card-actions-top" @click.stop>
               <button class="card-icon-btn" type="button" @click="copySpaceLink(space)"><i class="fa-solid fa-link"></i></button>
               <button class="card-icon-btn" type="button" :class="{ 'starred': space.starred }" @click="toggleStar(space)"><i :class="space.starred ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i></button>
               <el-tooltip content="Archive Project" placement="top">
                 <button class="card-icon-btn" type="button" @click="archiveProject(space)" :disabled="!canUserManageSpace(space)"><i class="fa-solid fa-box-archive"></i></button>
               </el-tooltip>
             </div>
          </div>
          
          <div class="card-body">
            <!-- Floating Project Icon -->
            <div class="floating-icon">
              <span class="emoji">{{ space.icon || emojiList[index % emojiList.length] || '📁' }}</span>
            </div>
            
            <div class="proj-title-row">
               <h3>{{ space.name }}</h3>
               <span class="proj-key">{{ space.key }}</span>
            </div>
            
            <p class="proj-desc">
              {{ space.originalRow?.description || 'Welcome to this Project! This project throws you into the driver\'s seat of work management.' }}
            </p>
            
            <div class="card-footer" @click.stop>
               <span class="visibility-pill" :class="space.networkType?.toLowerCase()">
                 <i :class="space.networkType === 'Private' ? 'fa-solid fa-lock' : 'fa-solid fa-globe'"></i>
                 {{ space.networkType || 'Public' }}
               </span>
               <div class="avatar-group">
                 <div class="avatar">{{ space.leadName ? space.leadName.charAt(0).toUpperCase() : 'U' }}</div>
               </div>
               <button class="card-icon-btn" type="button" @click="goToAdmin(space)" :disabled="!canUserManageSpace(space)" :title="canUserManageSpace(space) ? 'Project settings' : 'You do not have permission'">
                 <i class="fa-solid fa-gear"></i>
               </button>
            </div>
          </div>
        </div>
      </div>
      </section>
      
      <CreateSpaceModal v-model:visible="isCreateModalVisible" @created="fetchSpaces" />
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import CreateSpaceModal from '@/components/CreateSpaceModal.vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useProjectStore } from '@/store/useProjectStore'

const router = useRouter()
const projectStore = useProjectStore()
const loading = ref(false)
const spaces = ref([])
const searchQuery = ref('')
const sortDirection = ref('desc')
const showProjectFilters = ref(false)
const visibilityFilter = ref('all')
const isCreateModalVisible = ref(false)

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const systemRoles = (currentUser.systemRoles || []).map(r => r.toLowerCase())

const isSystemAdmin = computed(() => {
  return systemRoles.includes('system admin') || systemRoles.includes('admin') || systemRoles.includes('pm') || systemRoles.includes('po')
})

const canUserManageSpace = (space) => {
  if (isSystemAdmin.value) return true
  // Allow if user is creator
  const creatorId = space.originalRow?.creatorId || space.creatorId
  return creatorId === currentUser.id
}

const goToAdmin = (space) => {
  if (!canUserManageSpace(space)) {
    ElMessage.warning('You do not have permission to configure this project.')
    return
  }
  router.push(`/space/${space.id}/settings`)
}

const archiveProject = async (space) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to archive project "${space.name}"? It will be moved to the Archived section.`,
      'Archive Project',
      { confirmButtonText: 'Archive', cancelButtonText: 'Cancel', type: 'warning' }
    )
    await axiosClient.put(`/projects/${space.id}/archive`)
    ElMessage.success('Project archived successfully')
    fetchSpaces()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Could not archive project')
    }
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
.manage-spaces-page {
  padding: 40px;
  width: 100%;
  max-width: 1480px;
  margin: 0 auto;
  color: #E4E4E7;
  font-family: 'Inter', -apple-system, sans-serif;
  height: calc(100vh - 66px);
  display: flex;
  flex-direction: column;
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
  color: #A1A1AA;
  font-size: 18px;
}
.sh-left h1 {
  font-size: 16px;
  font-weight: 500;
  margin: 0;
  color: #E4E4E7;
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
  color: #71717A;
  font-size: 13px;
}
.search-box input {
  background: transparent;
  border: none;
  color: #E4E4E7;
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
  color: #A1A1AA;
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
.plane-btn-secondary.outline-btn:hover { background: #18181B; color: #E4E4E7; }
.plane-btn-secondary.outline-btn.active { background: #18181B; color: #fff; border-color: #27272A; }

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
.filter-title { color: #A1A1AA; font-size: 12px; font-weight: 600; margin-bottom: 8px; }
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
  border: 1px solid #27272A;
  border-radius: 6px;
  background: #111315;
  color: #D4D4D8;
  padding: 7px;
  cursor: pointer;
}
.clear-filter-btn:hover { background: #27272A; }

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
  background: #16181D;
  border: 1px solid #1E2025;
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
  color: #A1A1AA;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
  backdrop-filter: blur(4px);
}
.card-icon-btn:hover { background: rgba(0,0,0,0.5); color: #FFF; }
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
  background: #27272A;
  border: 4px solid #16181D;
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
  color: #E4E4E7;
}
.proj-key {
  font-size: 11px;
  color: #71717A;
  font-weight: 600;
  margin-top: 2px;
}

.proj-desc {
  font-size: 13px;
  color: #A1A1AA;
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
  background: #27272a;
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
  border: 2px solid #16181D;
}

.card-footer .card-icon-btn {
  background: transparent;
  border: none;
  font-size: 14px;
  margin-left: auto;
}
.card-footer .card-icon-btn:hover { background: #27272A; }

.loading-state, .empty-state { text-align: center; margin-top: 60px; color: #A1A1AA; }
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
</style>
