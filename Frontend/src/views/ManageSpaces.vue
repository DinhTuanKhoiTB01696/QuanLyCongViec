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
          <button class="plane-btn-secondary outline-btn">
             <i class="fa-solid fa-arrow-down-short-wide"></i> Created date
          </button>
          <button class="plane-btn-secondary outline-btn">
             <i class="fa-solid fa-filter"></i> Filters
          </button>
          <button class="plane-btn-primary" @click="isCreateModalVisible = true">
            Add Project
          </button>
        </div>
      </header>

      <!-- Spaces Grid -->
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
          <div class="card-cover" :style="{ background: coverGradients[index % coverGradients.length] }">
             <div class="card-actions-top" @click.stop>
               <button class="card-icon-btn"><i class="fa-solid fa-link"></i></button>
               <button class="card-icon-btn" :class="{ 'starred': space.starred }"><i class="fa-regular fa-star"></i></button>
             </div>
          </div>
          
          <div class="card-body">
            <!-- Floating Project Icon -->
            <div class="floating-icon">
              <span class="emoji">{{ emojiList[index % emojiList.length] || '👇' }}</span>
            </div>
            
            <div class="proj-title-row">
               <h3>{{ space.name }}</h3>
               <span class="proj-key">{{ space.key }}</span>
            </div>
            
            <p class="proj-desc">
              {{ space.originalRow?.description || 'Welcome to this Project! This project throws you into the driver\'s seat of work management. Through curated work items, you\'ll uncover key features...' }}
            </p>
            
            <div class="card-footer" @click.stop>
               <div class="avatar-group">
                 <div class="avatar">{{ space.leadName ? space.leadName.charAt(0).toUpperCase() : 'U' }}</div>
               </div>
               <button class="card-icon-btn" @click="goToAdmin(space.id)">
                 <i class="fa-solid fa-gear"></i>
               </button>
            </div>
          </div>
        </div>
      </div>
      
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

const router = useRouter()
const loading = ref(false)
const spaces = ref([])
const searchQuery = ref('')
const isCreateModalVisible = ref(false)

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const systemRoles = currentUser.systemRoles || []
const canManageSpace = computed(() => {
  return systemRoles.includes('System Admin') || systemRoles.includes('Admin') || systemRoles.includes('PM') || systemRoles.includes('PO') || systemRoles.includes('admin')
})

const goToAdmin = (projectId) => {
  router.push('/admin/configuration')
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
      starred: false,
      name: p.name,
      key: p.key || p.name.substring(0, 4).toUpperCase(),
      leadName: p.leadName || p.reporterName || 'Admin',
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
  return spaces.value.filter(s => {
    return !searchQuery.value || s.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || s.key.toLowerCase().includes(searchQuery.value.toLowerCase())
  })
})

const goToSpace = (id) => {
  router.push(`/space/${id}`)
}
</script>

<style scoped>
.manage-spaces-page {
  padding: 40px;
  max-width: 1200px;
  margin: 0 auto;
  color: #E4E4E7;
  font-family: 'Inter', -apple-system, sans-serif;
}

/* Header */
.spaces-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
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

/* Grid */
.spaces-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 24px;
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
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
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
}
.card-footer .card-icon-btn:hover { background: #27272A; }

.loading-state, .empty-state { text-align: center; margin-top: 60px; color: #A1A1AA; }
.empty-icon { font-size: 48px; color: #3F3F46; margin-bottom: 16px; }
.empty-state p { margin-bottom: 24px; }
</style>
