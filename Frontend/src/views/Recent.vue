<template>
  <NexusLayout>
    <div class="recent-container">
      <div class="recent-header">
        <div class="header-left">
          <h1 class="page-title">Gần đây</h1>
          <p class="subtitle">Những công việc bạn đã xem hoặc thực hiện gần đây.</p>
        </div>
      </div>

      <div class="recent-content">
        <div v-if="loading" class="loading-state">
          <i class="fa-solid fa-circle-notch fa-spin"></i>
          <span>Đang tải...</span>
        </div>

        <div v-else-if="recentTasks.length === 0" class="empty-state">
          <div class="empty-illustration">
            <i class="fa-solid fa-clock-rotate-left"></i>
          </div>
          <h3>Chưa có hoạt động nào</h3>
          <p>Bắt đầu làm việc trong các Không gian để thấy công việc gần đây của bạn ở đây.</p>
          <el-button type="primary" @click="$router.push('/dashboard')">Quay về Bảng điều khiển</el-button>
        </div>

        <div v-else class="tasks-grid">
          <div v-for="task in recentTasks" :key="task.id" class="recent-task-card" @click="goToTask(task)">
            <div class="task-top">
              <span class="task-key" :style="{ color: task.statusColor }">{{ task.statusText }}</span>
              <span class="task-time">{{ formatTime(task.updatedAt) }}</span>
            </div>
            <h4 class="task-title">{{ task.title }}</h4>
            <div class="task-footer">
              <div class="project-tag">
                <i class="fa-solid fa-square" :style="{ color: task.projectColor || '#3b82f6' }"></i>
                <span>{{ task.projectName }}</span>
              </div>
              <div class="assignee-avatar" v-if="task.assignee">
                {{ task.assignee[0] }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const router = useRouter()
const loading = ref(true)
const recentTasks = ref([])

const formatTime = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return new Intl.DateTimeFormat('vi-VN', { 
    day: '2-digit', 
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const goToTask = (task) => {
  if (task.projectId) {
    router.push(`/projects/${task.projectId}/summary`)
  }
}

onMounted(() => {
  // Giả lập tải dữ liệu gần đây
  setTimeout(() => {
    recentTasks.value = [
      {
        id: '1',
        title: 'Thiết kế giao diện Dashboard SprintA',
        statusText: 'IN PROGRESS',
        statusColor: '#a855f7',
        updatedAt: new Date().toISOString(),
        projectName: 'Phát triển SprintA',
        projectId: '4238e87d-8153-4886-9057-0744de88924b',
        assignee: 'C'
      },
      {
        id: '2',
        title: 'Cài đặt SignalR cho Real-time updates',
        statusText: 'DONE',
        statusColor: '#22c55e',
        updatedAt: new Date(Date.now() - 3600000).toISOString(),
        projectName: 'Phát triển SprintA',
        projectId: '4238e87d-8153-4886-9057-0744de88924b',
        assignee: 'C'
      },
      {
        id: '3',
        title: 'Fix bug navigation sidebar',
        statusText: 'TODO',
        statusColor: '#64748b',
        updatedAt: new Date(Date.now() - 86400000).toISOString(),
        projectName: 'Quản lý dự án',
        projectId: '4238e87d-8153-4886-9057-0744de88924b',
        assignee: 'A'
      }
    ]
    loading.value = false
  }, 1000)
})
</script>

<style scoped>
.recent-container {
  padding: 32px 40px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  font-size: 28px;
  color: var(--text-primary);
  font-weight: 700;
  margin-bottom: 8px;
}

.subtitle {
  color: var(--text-secondary);
  font-size: 14px;
  margin-bottom: 40px;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 0;
  color: var(--text-secondary);
  gap: 16px;
}

.loading-state i {
  font-size: 32px;
  color: #3b82f6;
}

.empty-state {
  text-align: center;
  padding: 100px 0;
}

.empty-illustration {
  font-size: 64px;
  color: var(--border-color);
  margin-bottom: 24px;
}

.empty-state h3 {
  font-size: 20px;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.empty-state p {
  color: var(--text-secondary);
  margin-bottom: 24px;
}

.tasks-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.recent-task-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.recent-task-card:hover {
  transform: translateY(-4px);
  border-color: #3b82f6;
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
}

.task-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.task-key {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.task-time {
  font-size: 11px;
  color: var(--text-muted);
}

.task-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 20px;
  line-height: 1.4;
}

.task-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.project-tag {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: var(--text-secondary);
}

.assignee-avatar {
  width: 24px;
  height: 24px;
  background-color: #3b82f6;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
}
</style>
