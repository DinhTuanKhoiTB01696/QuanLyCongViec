<template>
  <NexusLayout>
    <div class="archives-container">
      <!-- Header -->
      <header class="archives-header">
        <div class="ah-left">
          <span class="breadcrumb"><i class="fa-solid fa-briefcase"></i> Projects <i class="fa-solid fa-chevron-right separator"></i> Archived</span>
        </div>
        <div class="ah-right">
          <div class="search-box">
             <i class="fa-solid fa-magnifying-glass"></i>
          </div>
          <button class="plane-toolbar-btn"><i class="fa-solid fa-arrow-down-short-wide"></i> Created date</button>
          <button class="plane-toolbar-btn"><i class="fa-solid fa-filter"></i> Filters</button>
        </div>
      </header>

      <!-- Content -->
      <div v-if="loading" class="loading-wrap">
        <el-skeleton :rows="5" animated />
      </div>

      <div v-else-if="archivedProjects.length > 0" class="archives-grid">
        <div v-for="project in archivedProjects" :key="project.id" class="project-card">
          <div class="pc-icon">
            <i class="fa-solid fa-briefcase"></i>
          </div>
          <div class="pc-content">
            <h4 class="pc-name">{{ project.name }}</h4>
            <p class="pc-desc">{{ project.description || 'No description' }}</p>
            <div class="pc-meta">
               <span><i class="fa-solid fa-users"></i> {{ project.activeMemberCount }} members</span>
               <span><i class="fa-solid fa-calendar"></i> {{ formatDate(project.createdAt) }}</span>
            </div>
          </div>
          <div class="pc-actions">
            <el-button link type="primary" @click="handleRestore(project)">
              <i class="fa-solid fa-rotate-left"></i> Restore
            </el-button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="empty-state">
        <div class="empty-icon-wrap">
          <i class="fa-solid fa-box-archive empty-icon"></i>
        </div>
        <h3 class="empty-title">No projects archived</h3>
        <p class="empty-desc">Looks like all your projects are still active—great job!</p>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage, ElMessageBox } from 'element-plus'

const archivedProjects = ref([])
const loading = ref(true)

const fetchArchivedProjects = async () => {
  loading.value = true
  try {
    const res = await axiosClient.get('/projects/archived')
    archivedProjects.value = res.data.data || []
  } catch (error) {
    console.error('Failed to fetch archived projects:', error)
    ElMessage.error('Không thể tải danh sách dự án lưu trữ')
  } finally {
    loading.value = false
  }
}

const handleRestore = async (project) => {
  try {
    await ElMessageBox.confirm(`Bạn có chắc muốn khôi phục dự án "${project.name}"?`, 'Xác nhận', {
      confirmButtonText: 'Khôi phục',
      cancelButtonText: 'Hủy',
      type: 'info'
    })
    
    await axiosClient.put(`/projects/${project.id}/restore`)
    ElMessage.success(`Đã khôi phục dự án ${project.name}`)
    fetchArchivedProjects()
  } catch (err) {
    if (err !== 'cancel') {
      console.error('Restore failed:', err)
      ElMessage.error('Lỗi khi khôi phục dự án')
    }
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return 'N/A'
  return new Date(dateStr).toLocaleDateString()
}

onMounted(() => {
  fetchArchivedProjects()
})
</script>

<style scoped>
.archives-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.archives-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-muted);
}
.separator { font-size: 10px; color: var(--color-text-muted); }

.ah-right {
  display: flex;
  gap: 12px;
}
.search-box {
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  color: var(--color-text-muted);
  border-radius: 4px;
  cursor: pointer;
}
.search-box:hover { color: var(--color-text-primary); background: var(--color-border); }

.plane-toolbar-btn {
  background: transparent;
  border: 1px solid var(--color-border);
  color: #D4D4D8;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-toolbar-btn:hover { background: var(--color-border); }

.loading-wrap {
  padding: 40px;
}

.archives-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 16px;
  padding: 24px;
  overflow-y: auto;
}

.project-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 16px;
  display: flex;
  gap: 16px;
  align-items: flex-start;
  transition: border-color 0.2s, background 0.2s;
  position: relative;
}
.project-card:hover {
  border-color: #3F3F46;
  background: #141619;
}

.pc-icon {
  width: 40px; height: 40px;
  background: var(--color-border);
  border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.pc-content {
  flex: 1;
}
.pc-name {
  margin: 0 0 4px 0;
  font-size: 15px;
  font-weight: 600;
}
.pc-desc {
  margin: 0 0 12px 0;
  font-size: 13px;
  color: var(--color-text-muted);
}
.pc-meta {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: var(--color-text-muted);
}

.pc-actions {
  display: flex;
  align-items: center;
}

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
.empty-icon-wrap {
  width: 80px; height: 80px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 24px;
  position: relative;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
}
.empty-icon { font-size: 32px; color: var(--color-text-muted); }
.empty-title { margin: 0 0 8px 0; font-size: 16px; font-weight: 600; color: var(--color-text-primary); }
.empty-desc { margin: 0; font-size: 14px; color: var(--color-text-muted); }
</style>



