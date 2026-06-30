<template>
  <div class="jd-content">
    <div class="jd-header">
      <h3>Starred</h3>
    </div>

    <div class="jd-body">
      <!-- Empty state: kh\u00f4ng c\u00f3 c\u1ea3 project l\u1eabn task -->
      <div v-if="starredProjects.length === 0 && starredTasks.length === 0" class="jd-empty-starred">
        <img src="https://jira-frontend-bifrost.prod-east.frontend.public.atl-paas.net/assets/starred-empty.svg" alt="Empty Starred" onerror="this.style.display='none'" />
        <h4>You haven't starred anything yet</h4>
        <p>Mark items that are important to you with a star to quickly access them here.</p>
      </div>

      <div v-else class="jd-list">
        <!-- Section: Starred Spaces (Projects) -->
        <template v-if="starredProjects.length > 0">
          <div class="jd-section-label">Spaces</div>
          <div
            v-for="project in starredProjects"
            :key="`proj-${project.id}`"
            class="jd-item"
            @click="goToProject(project)"
          >
            <div class="jd-item-icon">
              <span
                class="proj-icon"
                :style="{ background: projectColor(project) }"
              >{{ project.icon || project.name?.charAt(0)?.toUpperCase() || 'P' }}</span>
            </div>
            <div class="jd-item-content">
              <div class="jd-item-title">{{ project.name || 'Space' }}</div>
              <div class="jd-item-subtitle">Space</div>
            </div>
            <div class="jd-item-action" @click.stop="unstarProject(project)" title="Remove from starred">
              <i class="fa-solid fa-star text-yellow-400"></i>
            </div>
          </div>
        </template>

        <!-- Section: Starred Tasks -->
        <template v-if="starredTasks.length > 0">
          <div class="jd-section-label">Work items</div>
          <div
            v-for="item in starredTasks"
            :key="`task-${item.id}`"
            class="jd-item"
            @click="goToTask(item)"
          >
            <div class="jd-item-icon">
              <i class="fa-solid fa-square-check text-blue-500"></i>
            </div>
            <div class="jd-item-content">
              <div class="jd-item-title">{{ item.title || 'Task' }}</div>
              <div class="jd-item-subtitle">Task • {{ item.sequenceId || item.id?.substring(0, 8).toUpperCase() }} • {{ item.projectName || 'Project' }}</div>
            </div>
            <div class="jd-item-action" @click.stop="unstarTask(item)" title="Remove from starred">
              <i class="fa-solid fa-star text-yellow-400"></i>
            </div>
          </div>
        </template>
      </div>
    </div>

    <div class="jd-footer" v-if="starredProjects.length > 0 || starredTasks.length > 0">
      <button @click="viewAllStarred">View all starred items</button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { useProjectStore } from '@/store/useProjectStore'
import axiosClient from '@/api/axiosClient'

const emit = defineEmits(['close'])
const router = useRouter()
const workTaskStore = useWorkTaskStore()
const projectStore = useProjectStore()

// --- Starred Projects (reactive t\u1ef1 \u0111\u1ed9ng theo favoriteProjects getter) ---
const starredProjects = computed(() => projectStore.favoriteProjects)

// --- Starred Tasks (reactive t\u1ef1 \u0111\u1ed9ng theo Pinia state) ---
const starredTasks = computed(() => [...workTaskStore.starredTasks].reverse())

// Gi\u1eef expose \u0111\u1ec3 t\u01b0\u01a1ng th\u00edch v\u1edbi parent (NexusSidebar @show="onStarredShow")
const loadStarredItems = async () => {
  try {
    await workTaskStore.fetchStarredTasks()
  } catch (err) {
    console.error('Failed to validate starred tasks:', err)
  }
}
defineExpose({ loadStarredItems })

// --- Actions ---
const unstarProject = async (project) => {
  try {
    await projectStore.updateFavorite(project.id, false)
  } catch {
    // silent
  }
}

const unstarTask = (item) => {
  workTaskStore.toggleTaskStar(item)
}

const goToProject = (project) => {
  emit('close')
  router.push(`/space/${project.id}`)
}

const goToTask = (item) => {
  emit('close')
  if (item.projectId) {
    router.push(`/space/${item.projectId}/work-items`)
  }
}

const viewAllStarred = () => {
  emit('close')
  router.push('/dashboard?tab=starred')
}

const projectColor = (project) => {
  if (project.cover && project.cover.startsWith('#')) return project.cover
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47']
  return colors[(project.name?.length || 0) % colors.length]
}
</script>

<style scoped>
.jd-content {
  display: flex;
  flex-direction: column;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 96%, var(--color-accent) 4%), var(--color-surface));
  color: var(--color-text-primary, #172b4d);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  border-radius: 16px;
  overflow: hidden;
  min-width: 340px;
}

.jd-header {
  padding: 16px 16px 8px;
}
.jd-header h3 {
  margin: 0;
  font-size: 12px;
  text-transform: uppercase;
  color: var(--color-text-muted, #6b778c);
  font-weight: 850;
  letter-spacing: 0.08em;
}

.jd-body {
  flex: 1;
  overflow-y: auto;
  max-height: 360px;
  padding: 0 8px 8px;
}

.jd-empty-starred {
  text-align: center;
  padding: 24px 16px;
}
.jd-empty-starred img {
  width: 100px;
  margin: 0 auto 16px;
  opacity: 0.7;
}
.jd-empty-starred h4 {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 6px 0;
  color: var(--color-text-primary, #172b4d);
}
.jd-empty-starred p {
  font-size: 12px;
  color: var(--color-text-muted, #6b778c);
  margin: 0;
  line-height: 1.4;
}

.jd-section-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted, #6b778c);
  letter-spacing: 0.6px;
  padding: 10px 16px 4px;
}

.jd-list {
  padding-bottom: 8px;
}

.jd-item {
  display: flex;
  align-items: center;
  padding: 10px 10px;
  cursor: pointer;
  border-radius: 12px;
  transition: background 0.16s ease, transform 0.16s ease;
}
.jd-item:hover {
  background: color-mix(in srgb, var(--color-accent) 10%, var(--color-surface-hover));
  transform: translateX(2px);
}

.jd-item-icon {
  margin-right: 12px;
  font-size: 16px;
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.proj-icon {
  width: 24px;
  height: 24px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: #fff;
}

.jd-item-content {
  flex: 1;
  min-width: 0;
}

.jd-item-title {
  font-size: 13px;
  font-weight: 800;
  color: var(--color-text-primary, #172b4d);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.jd-item-subtitle {
  font-size: 11px;
  color: var(--color-text-muted, #6b778c);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.jd-item-action {
  margin-left: 12px;
  font-size: 14px;
  padding: 4px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  flex-shrink: 0;
  opacity: 0;
  transition: opacity 0.15s;
}
.jd-item:hover .jd-item-action {
  opacity: 1;
}
.jd-item-action:hover {
  background: rgba(9, 30, 66, 0.08);
}

.jd-footer {
  padding: 10px 14px;
  border-top: 1px solid var(--color-border, #ebecf0);
}

.jd-footer button {
  width: 100%;
  text-align: left;
  background: transparent;
  border: none;
  color: var(--color-accent, #0c66e4);
  font-size: 13px;
  font-weight: 800;
  padding: 8px;
  border-radius: 10px;
  cursor: pointer;
}
.jd-footer button:hover {
  background: var(--color-surface-hover, #f4f5f7);
  text-decoration: none;
}

[data-theme='dark'] .jd-content {
  background:
    linear-gradient(180deg, rgba(30, 41, 59, 0.96), rgba(15, 23, 42, 0.98)),
    #0f172a;
}
</style>

