<template>
  <div class="project-layout-wrapper">
    <!-- Project Header -->
    <header class="project-global-header">
      <div class="pgh-content">
        <div class="pgh-left">
          <div class="pgh-icon" :style="{ backgroundColor: project?.cover || '#3b82f6' }">
            {{ project?.icon || '📦' }}
          </div>
          <div class="pgh-info">
            <h1 class="pgh-title">{{ project?.name || 'Loading Project...' }}</h1>
            <p class="pgh-desc" v-if="project?.description">{{ project.description }}</p>
          </div>
          <div class="pgh-status" v-if="project?.status">
            {{ project.status }}
          </div>
        </div>
        <div class="pgh-right">
          <button class="nexus-btn-primary create-btn" @click="createTask">
            <i class="fa-solid fa-plus"></i>
            {{ t('Create Work Item', 'Tạo công việc') }}
          </button>
        </div>
      </div>

      <!-- Horizontal Navigation -->
      <nav class="project-horizontal-nav" ref="navScrollRef">
        <div class="nav-links">
          <router-link 
            v-for="nav in projectNavLinks" 
            :key="nav.name"
            :to="`/space/${projectId}/${nav.path}`"
            class="nav-item"
            active-class="nav-active"
          >
            <i :class="nav.icon"></i>
            <span>{{ nav.label }}</span>
          </router-link>
        </div>
      </nav>
    </header>

    <!-- Project Content (Children Routes) -->
    <main class="project-main-content">
      <router-view v-slot="{ Component }">
        <Transition name="fade-fast" mode="out-in">
          <component :is="Component" />
        </Transition>
      </router-view>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useProjectStore } from '@/store/useProjectStore'
import { useI18n } from '@/composables/useI18n'

const route = useRoute()
const router = useRouter()
const projectStore = useProjectStore()
const { t } = useI18n()

const projectId = computed(() => route.params.id)
const project = computed(() => projectStore.currentProject)

const showCreateTaskModal = ref(false)

const projectNavLinks = computed(() => [
  { name: 'Dashboard', path: 'dashboard', label: t('Dashboard', 'Tổng quan'), icon: 'fa-solid fa-chart-pie' },
  { name: 'WorkItems', path: 'work-items', label: t('Work items', 'Công việc'), icon: 'fa-solid fa-layer-group' },
  { name: 'Timeline', path: 'timeline', label: t('Timeline', 'Timeline'), icon: 'fa-solid fa-chart-gantt' },
  { name: 'Cycles', path: 'cycles', label: t('Cycles', 'Chu kỳ'), icon: 'fa-solid fa-rotate' },
  { name: 'Modules', path: 'modules', label: t('Modules', 'Phân hệ'), icon: 'fa-solid fa-cubes' },
  { name: 'Reports', path: 'reports', label: t('Reports', 'Báo cáo'), icon: 'fa-solid fa-chart-line' },
  { name: 'Views', path: 'views', label: t('Views', 'Góc nhìn'), icon: 'fa-regular fa-eye' },
  { name: 'Pages', path: 'pages', label: t('Pages', 'Tài liệu'), icon: 'fa-regular fa-file-lines' },
  { name: 'Members', path: 'members', label: t('Members', 'Thành viên'), icon: 'fa-solid fa-users' }
])

const loadProject = async () => {
  if (projectId.value) {
    if (!projectStore.currentProject || projectStore.currentProject.id !== projectId.value) {
      // It's possible the store handles loading details somewhere else, 
      // but we ensure it's loaded if not.
      projectStore.clearProjectContext(projectId.value)
      await projectStore.fetchProjectDetails(projectId.value)
    }
  }
}

onMounted(() => {
  loadProject()
})

watch(projectId, () => {
  loadProject()
})

const createTask = () => {
  window.dispatchEvent(new CustomEvent('open-create-task', { detail: { statusName: 'TO DO' } }))
  if (route.name !== 'SpaceSummary') {
    router.push({ name: 'SpaceSummary', params: { id: projectId.value } })
  }
}
</script>

<style scoped>
.project-layout-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background-color: var(--color-bg, #ffffff);
}

.project-global-header {
  border-bottom: 1px solid var(--color-border, #dfe1e6);
  background: var(--color-surface, #ffffff);
  flex-shrink: 0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.pgh-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
}

.pgh-left {
  display: flex;
  align-items: center;
  gap: 16px;
  overflow: hidden;
}

.pgh-icon {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
}

.pgh-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.pgh-title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--color-text-primary, #172b4d);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.pgh-desc {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--color-text-muted, #6b778c);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.pgh-status {
  padding: 4px 8px;
  background: rgba(9, 30, 66, 0.04);
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-secondary, #42526e);
}

.pgh-right {
  display: flex;
  align-items: center;
}

.create-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  font-size: 14px;
  font-weight: 500;
}

/* Horizontal Navigation */
.project-horizontal-nav {
  padding: 0 24px;
  overflow-x: auto;
  white-space: nowrap;
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none;  /* IE and Edge */
}
.project-horizontal-nav::-webkit-scrollbar {
  display: none;
}

.nav-links {
  display: flex;
  gap: 24px;
}

.nav-item {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 4px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-secondary, #42526e);
  text-decoration: none;
  position: relative;
  transition: color 0.2s;
  cursor: pointer;
}

.nav-item:hover {
  color: var(--color-text-primary, #172b4d);
}

.nav-active {
  color: var(--color-accent, #0c66e4);
  font-weight: 600;
}

.nav-item::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 3px;
  background-color: transparent;
  border-radius: 3px 3px 0 0;
  transition: background-color 0.2s, transform 0.2s;
  transform: scaleX(0);
  transform-origin: center;
}

.nav-active::after {
  background-color: var(--color-accent, #0c66e4);
  transform: scaleX(1);
}

.project-main-content {
  flex: 1;
  overflow-y: auto;
  background-color: var(--color-surface-hover, #f4f5f7);
}

/* Transition */
.fade-fast-enter-active,
.fade-fast-leave-active {
  transition: opacity 0.15s ease;
}
.fade-fast-enter-from,
.fade-fast-leave-to {
  opacity: 0;
}
</style>
