<template>
  <NexusLayout>
    <div class="plane-dashboard">
      <header class="nexus-feature-header">
        <div class="header-info">
          <p class="eyebrow">SprintA Workspace</p>
          <h1><i class="fa-solid fa-house"></i> Home</h1>
          <p class="muted">Welcome back. Track your project health and jump into your active work items.</p>
        </div>
        <div class="nexus-controls-row">
          <button class="nexus-btn nexus-btn-outlined" type="button" @click="router.push('/spaces')">Browse projects</button>
          <button class="nexus-btn nexus-btn-primary" type="button" @click="openTaskModal"><i class="fa-solid fa-plus"></i> New Work Item</button>
        </div>
      </header>

      <section class="hero-panel">
        <div class="hero-copy">
          <h2>{{ currentUser?.fullName || 'Teammate' }}, keep work moving.</h2>
          <p>Track project health, jump into a space, and draft a new work item without leaving the dashboard.</p>
        </div>
        <div class="hero-stats">
          <div class="stat-box">
            <span class="stat-label">Projects</span>
            <strong>{{ visibleProjects.length }}</strong>
          </div>
          <div class="stat-box">
            <span class="stat-label">Favorites</span>
            <strong>{{ favoriteProjects.length }}</strong>
          </div>
        </div>
      </section>

      <section class="search-panel">
        <div class="search-shell">
          <i class="fa-solid fa-magnifying-glass"></i>
          <input v-model="projectSearch" type="text" placeholder="Filter projects by name or key" />
        </div>
      </section>

      <section class="projects-panel">
        <div class="section-head">
          <h2>Projects</h2>
          <span>{{ filteredProjects.length }} shown</span>
        </div>

        <div v-if="projectStore.loading" class="empty-state">
          <i class="fa-solid fa-spinner fa-spin"></i>
          <p>Loading projects...</p>
        </div>

        <div v-else-if="filteredProjects.length === 0" class="empty-state">
          <i class="fa-regular fa-folder-open"></i>
          <p>No projects match your search.</p>
        </div>

        <div v-else class="project-grid">
          <article v-for="project in filteredProjects" :key="project.id" class="project-card">
            <button class="favorite-btn" type="button" @click="toggleFavorite(project)">
              <i :class="project.isFavorite ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
            </button>

            <div class="project-cover" :style="{ background: project.cover || fallbackCover(project) }">
              <span class="project-icon">{{ project.icon || project.name?.charAt(0)?.toUpperCase() || 'P' }}</span>
            </div>

            <div class="project-body">
              <div class="project-heading">
                <h3>{{ project.name }}</h3>
                <span>{{ project.key }}</span>
              </div>
              <p>{{ project.description || 'No description yet.' }}</p>
              <div class="project-meta">
                <span>{{ project.networkType || 'Public' }}</span>
                <span>{{ project.leadName || 'Project owner' }}</span>
              </div>
            </div>

            <div class="project-actions">
              <button class="secondary-btn small" type="button" @click="openProject(project.id)">Open</button>
              <button class="primary-btn small" type="button" @click="openTaskModal(project)">New Work Item</button>
            </div>
          </article>
        </div>
      </section>

      <section v-if="favoriteProjects.length" class="projects-panel">
        <div class="section-head">
          <h2>Favorites</h2>
          <span>{{ favoriteProjects.length }} starred</span>
        </div>

        <div class="project-grid">
          <article v-for="project in favoriteProjects" :key="`favorite-${project.id}`" class="project-card">
            <button class="favorite-btn" type="button" @click="toggleFavorite(project)">
              <i :class="project.isFavorite ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
            </button>

            <div class="project-cover" :style="{ background: project.cover || fallbackCover(project) }">
              <span class="project-icon">{{ project.icon || project.name?.charAt(0)?.toUpperCase() || 'P' }}</span>
            </div>

            <div class="project-body">
              <div class="project-heading">
                <h3>{{ project.name }}</h3>
                <span>{{ project.key }}</span>
              </div>
              <p>{{ project.description || 'No description yet.' }}</p>
              <div class="project-meta">
                <span>{{ project.networkType || 'Public' }}</span>
                <span>{{ project.leadName || 'Project owner' }}</span>
              </div>
            </div>

            <div class="project-actions">
              <button class="secondary-btn small" type="button" @click="openProject(project.id)">Open</button>
            </div>
          </article>
        </div>
      </section>

      <el-dialog v-model="taskModalVisible" title="New Work Item" width="560px" append-to-body>
        <div class="task-form">
          <label>
            Project
            <select v-model="taskForm.projectId">
              <option value="" disabled>Select a project</option>
              <option v-for="project in visibleProjects" :key="project.id" :value="project.id">{{ project.name }}</option>
            </select>
          </label>

          <label>
            Title
            <input v-model="taskForm.title" type="text" placeholder="What needs to be done?" />
          </label>

          <label>
            Status
            <select v-model="taskForm.statusName">
              <option value="BACKLOG">BACKLOG</option>
              <option value="TO DO">TO DO</option>
              <option value="IN PROGRESS">IN PROGRESS</option>
              <option value="DONE">DONE</option>
            </select>
          </label>

          <label>
            Description
            <textarea v-model="taskForm.description" rows="5" placeholder="Add context"></textarea>
          </label>
        </div>

        <template #footer>
          <div class="dialog-actions">
            <button class="secondary-btn" type="button" @click="taskModalVisible = false">Cancel</button>
            <button class="primary-btn" type="button" :disabled="submittingTask" @click="submitTask">
              {{ submittingTask ? 'Creating...' : 'Create work item' }}
            </button>
          </div>
        </template>
      </el-dialog>
    </div>
  </NexusLayout>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useProjectStore } from '@/store/useProjectStore'

const router = useRouter()
const projectStore = useProjectStore()

const currentUser = ref(null)
const currentDateTime = ref('')
const projectSearch = ref('')
const taskModalVisible = ref(false)
const submittingTask = ref(false)
const taskForm = ref({
  projectId: '',
  title: '',
  description: '',
  statusName: 'TO DO'
})

const visibleProjects = computed(() => projectStore.sidebarProjects)
const favoriteProjects = computed(() => projectStore.favoriteProjects)
const filteredProjects = computed(() => {
  const keyword = projectSearch.value.trim().toLowerCase()
  if (!keyword) return visibleProjects.value
  return visibleProjects.value.filter(project =>
    project.name?.toLowerCase().includes(keyword) || project.key?.toLowerCase().includes(keyword)
  )
})

const updateTime = () => {
  const now = new Date()
  currentDateTime.value = now.toLocaleString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fallbackCover = (project) => {
  const gradients = [
    'linear-gradient(135deg, var(--color-bg) 0%, var(--color-accent) 100%)',
    'linear-gradient(135deg, var(--bg-secondary) 0%, var(--color-success) 100%)',
    'linear-gradient(135deg, var(--bg-tertiary) 0%, var(--color-warning) 100%)',
    'linear-gradient(135deg, var(--color-surface) 0%, var(--color-danger) 100%)'
  ]
  const name = project?.name || ''
  const index = name.length % gradients.length
  return gradients[index]
}

const openProject = (projectId) => {
  router.push(`/space/${projectId}`)
}

const openTaskModal = async (project = null) => {
  const availableProjects = visibleProjects.value.length
    ? visibleProjects.value
    : await projectStore.fetchAllProjects(true)

  if (!availableProjects.length) {
    ElMessage.warning('Create a project before creating a work item.')
    await router.push('/spaces')
    return
  }

  taskForm.value = {
    projectId: project?.id || availableProjects[0]?.id || '',
    title: '',
    description: '',
    statusName: 'TO DO'
  }
  taskModalVisible.value = true
}

const toggleFavorite = async (project) => {
  const nextValue = !project.isFavorite
  project.isFavorite = nextValue
  try {
    await projectStore.updateFavorite(project.id, nextValue)
    ElMessage.success(nextValue ? 'Project starred' : 'Project unstarred')
  } catch (error) {
    project.isFavorite = !nextValue
    ElMessage.error('Could not update favorite')
  }
}

const submitTask = async () => {
  if (!taskForm.value.projectId || !taskForm.value.title.trim()) {
    ElMessage.warning('Please choose a project and enter a title')
    return
  }

  submittingTask.value = true
  try {
    await axiosClient.post(`/projects/${taskForm.value.projectId}/WorkTasks`, {
      title: taskForm.value.title.trim(),
      description: taskForm.value.description,
      statusName: taskForm.value.statusName,
      priority: 3
    })
    ElMessage.success('Work item created')
    taskModalVisible.value = false
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create work item')
  } finally {
    submittingTask.value = false
  }
}

onMounted(async () => {
  const userStr = localStorage.getItem('user')
  if (userStr) currentUser.value = JSON.parse(userStr)
  updateTime()
  setInterval(updateTime, 60000)
  await projectStore.fetchAllProjects(true)
})
</script>

<style scoped>
.plane-dashboard {
  min-height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
  padding: 32px;
}

.dashboard-header,
.hero-panel,
.search-panel,
.projects-panel {
  max-width: 1180px;
  margin: 0 auto 24px;
}

.dashboard-header,
.section-head,
.hero-panel,
.project-actions,
.dialog-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.eyebrow {
  margin: 0 0 8px;
  color: var(--color-accent);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
}

.dashboard-title,
.hero-copy h2,
.section-head h2 {
  margin: 0;
}

.dashboard-subtitle,
.hero-copy p,
.section-head span,
.project-body p,
.project-meta {
  color: var(--color-text-secondary);
}

.header-actions,
.hero-stats {
  display: flex;
  gap: 12px;
}

.primary-btn,
.secondary-btn {
  border-radius: 2px;
  padding: 10px 14px;
  border: 1px solid transparent;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.primary-btn {
  background: var(--color-accent);
  color: #ffffff;
}

.secondary-btn {
  background: var(--color-surface);
  border-color: var(--color-border);
  color: var(--color-text-primary);
}

.small {
  padding: 8px 12px;
  font-size: 13px;
}

.hero-panel {
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 2px;
  padding: 24px;
  box-shadow: var(--shadow-sm);
}

.hero-copy {
  max-width: 680px;
}

.hero-stats {
  flex-wrap: wrap;
}

.stat-box {
  min-width: 120px;
  background: var(--bg-primary);
  border: 1px solid var(--border-color);
  border-radius: 2px;
  padding: 14px;
}

.stat-label {
  display: block;
  color: var(--color-text-muted);
  font-size: 12px;
  margin-bottom: 6px;
}

.search-shell {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 2px;
  padding: 0 14px;
}

.search-shell input {
  width: 100%;
  background: transparent;
  border: none;
  color: var(--color-text-primary);
  padding: 14px 0;
  outline: none;
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 18px;
}

.project-card {
  position: relative;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 2px;
  overflow: hidden;
  transition: transform 0.2s, box-shadow 0.2s;
}
.project-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.favorite-btn {
  position: absolute;
  top: 12px;
  right: 12px;
  z-index: 1;
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 2px;
  background: rgba(0, 0, 0, 0.45);
  color: #facc15;
  cursor: pointer;
}

.project-cover {
  height: 110px;
  display: flex;
  align-items: flex-end;
  padding: 16px;
}

.project-icon {
  width: 40px;
  height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 2px;
  background: rgba(255, 255, 255, 0.25);
  color: #ffffff;
  font-weight: 700;
  backdrop-filter: blur(4px);
}

.project-body {
  padding: 16px;
}

.project-heading {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.project-heading span {
  color: var(--color-text-muted);
  font-size: 12px;
}

.project-meta {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
}

.project-actions {
  padding: 0 16px 16px;
}

.empty-state {
  min-height: 180px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  background: var(--bg-secondary);
  border: 1px dashed var(--border-color);
  border-radius: 2px;
}

.task-form {
  display: grid;
  gap: 14px;
}

.task-form label {
  display: grid;
  gap: 8px;
  color: var(--color-text-secondary);
  font-size: 13px;
  font-weight: 600;
}

.task-form input,
.task-form select,
.task-form textarea {
  width: 100%;
  border: 1px solid var(--border-color);
  border-radius: 2px;
  padding: 10px 12px;
  background: var(--bg-primary);
  color: var(--text-primary);
  font: inherit;
}

@media (max-width: 768px) {
  .plane-dashboard {
    padding: 20px;
  }

  .dashboard-header,
  .hero-panel {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>



