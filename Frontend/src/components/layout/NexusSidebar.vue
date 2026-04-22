<template>
  <aside class="plane-sidebar" :class="{ 'collapsed': !isVisible }">
    <div class="sidebar-scrollable">
      <div class="sidebar-top-action">
        <button class="new-work-btn" @click="triggerCreateTask">
          <i class="fa-solid fa-pen-to-square"></i>
          <span>New work item</span>
        </button>
      </div>

      <ul class="nav-menu">
        <li class="nav-item">
          <router-link to="/dashboard" class="nav-link" active-class="active">
            <i class="fa-solid fa-house"></i>
            <span>Home</span>
          </router-link>
        </li>
        <li class="nav-item">
          <router-link to="/drafts" class="nav-link" exact-active-class="active">
            <i class="fa-solid fa-pen-nib"></i>
            <span>Drafts</span>
          </router-link>
        </li>
        <li class="nav-item">
          <router-link to="/your-work" class="nav-link">
            <i class="fa-regular fa-user"></i>
            <span>Your work</span>
          </router-link>
        </li>
        <li class="nav-item">
          <router-link to="/stickies" class="nav-link">
            <i class="fa-solid fa-note-sticky"></i>
            <span>Stickies</span>
          </router-link>
        </li>
        <li class="nav-item">
          <router-link to="/rewards" class="nav-link">
            <i class="fa-solid fa-trophy"></i>
            <span>Rewards</span>
          </router-link>
        </li>
      </ul>

      <!-- Favorites Division -->
      <div class="nav-section-title" v-if="favoriteSprints.length > 0">Favorites</div>
      <ul class="nav-menu" v-if="favoriteSprints.length > 0">
        <li class="nav-item" v-for="fs in favoriteSprints" :key="fs.id">
          <router-link :to="`/space/${fs.projectId}/cycles`" class="nav-link">
             <i class="fa-solid fa-arrows-spin fav-icon"></i>
             <span class="truncate">{{ fs.name }}</span>
          </router-link>
        </li>
      </ul>

      <!-- Workspace Division -->
      <div class="nav-section-title">Workspace</div>
      <ul class="nav-menu">
        <li class="nav-item">
          <router-link to="/spaces" class="nav-link">
            <i class="fa-solid fa-briefcase"></i>
            <span>Projects</span>
          </router-link>
        </li>
        <li class="nav-item">
          <div class="nav-link" :class="{ 'dropdown-active': showMorePanel }" @click="showMorePanel = !showMorePanel">
            <i class="fa-solid fa-ellipsis"></i>
            <span>{{ showMorePanel ? 'Hide' : 'More' }}</span>
          </div>
        </li>
      </ul>

      <!-- Secondary Panel for More -->
      <transition name="slide-left">
        <div class="more-panel" v-if="showMorePanel">
          <ul class="nav-menu">
            <li class="nav-item sub-item">
              <router-link to="/views" class="nav-link">
                <i class="fa-solid fa-layer-group"></i>
                <span>Views</span>
                <i class="fa-solid fa-thumbtack pin-icon"></i>
              </router-link>
            </li>
            <li class="nav-item sub-item">
              <router-link to="/analytics" class="nav-link">
                <i class="fa-solid fa-chart-simple"></i>
                <span>Analytics</span>
                <i class="fa-solid fa-thumbtack pin-icon"></i>
              </router-link>
            </li>
            <li class="nav-item sub-item">
              <router-link to="/archives" class="nav-link">
                <i class="fa-solid fa-box-archive"></i>
                <span>Archives</span>
                <i class="fa-solid fa-thumbtack pin-icon"></i>
              </router-link>
            </li>
          </ul>
        </div>
      </transition>

      <!-- Projects Division -->
      <div class="nav-section-title flex-between">
        Projects
        <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
      </div>
      <ul class="nav-menu">
        <template v-for="project in projectTree" :key="project.id">
          <li class="nav-item">
            <div
              class="nav-link proj-folder"
              :class="{ active: currentProjectId === project.id }"
              @click="toggleProject(project.id)"
            >
              <span class="proj-icon" :style="{ background: projectColor(project) }">{{ projectIcon(project) }}</span>
              <span class="truncate">{{ project.name }}</span>
              <i class="fa-solid ms-auto" :class="project.expanded ? 'fa-chevron-down' : 'fa-chevron-right'" style="font-size: 10px;"></i>
            </div>
          </li>

          <li v-for="child in project.children" v-show="project.expanded" :key="child.id" class="nav-item sub-item">
            <router-link :to="child.route" class="nav-link" active-class="active">
              <i :class="childIcon(child.key)"></i>
              <span>{{ child.label }}</span>
            </router-link>
          </li>
        </template>
      </ul>
    </div>

    <!-- Bottom Actions -->
    <div class="sidebar-bottom">
      <a href="#" class="community-link">
        <i class="fa-regular fa-comment"></i> Community
      </a>
    </div>
  </aside>
</template>

<script setup>
import { computed, ref, defineProps, defineEmits, watch, onMounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useSprintStore } from '@/store/useSprintStore'
import { useProjectStore } from '@/store/useProjectStore'

const route = useRoute()
const router = useRouter()
const showMorePanel = ref(false)
const projectStore = useProjectStore()
const currentProjectId = computed(() => {
  return route.params.id || localStorage.getItem('currentProjectId') || 'default'
})

const props = defineProps({
  isVisible: { type: Boolean, default: true }
})
const emit = defineEmits(['close-mobile'])

const sprintStore = useSprintStore()
const projectTree = computed(() => projectStore.projectTree)
const favoriteSprints = computed(() => {
   if (!sprintStore.sprints) return [];
   return sprintStore.sprints.filter(s => s.isFavorite);
})

watch(currentProjectId, async (newVal, oldVal) => {
   if (newVal && newVal !== 'default') {
      if (newVal !== oldVal) {
        projectStore.expandProject(newVal)
      }
      localStorage.setItem('currentProjectId', newVal)
      sprintStore.fetchSprints(newVal)
      await projectStore.fetchProjectDetails(newVal)
   }
}, { immediate: true })

onMounted(() => {
  projectStore.fetchAllProjects().catch(() => {})
})

const toggleProject = (projectId) => {
  if (currentProjectId.value !== projectId) {
    router.push(`/space/${projectId}`)
  }
  projectStore.toggleProject(projectId)
}

const childIcon = (key) => ({
  'work-items': 'fa-solid fa-layer-group',
  'cycles': 'fa-solid fa-arrows-spin',
  'modules': 'fa-solid fa-table-cells-large',
  'views': 'fa-solid fa-list-ul',
  'pages': 'fa-regular fa-file-lines'
}[key] || 'fa-solid fa-chevron-right')

const projectIcon = (project) => project.icon || project.name?.charAt(0)?.toUpperCase() || 'P'
const projectColor = (project) => {
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47']
  return colors[project.name?.length % colors.length] || '#579dff'
}

const triggerCreateTask = async () => {
  const projects = projectStore.allProjects.length
    ? projectStore.allProjects
    : await projectStore.fetchAllProjects()

  if (!projects.length) {
    ElMessage.warning('Bạn cần tạo project trước khi tạo work item.')
    await router.push('/spaces')
    return
  }

  const preferredProjectId = projects.some(p => p.id === currentProjectId.value)
    ? currentProjectId.value
    : projects[0].id

  if (route.path !== `/space/${preferredProjectId}`) {
    await router.push(`/space/${preferredProjectId}`)
    await nextTick()
    window.setTimeout(() => {
      window.dispatchEvent(new CustomEvent('global-create-task'))
    }, 120)
    return
  }
  window.dispatchEvent(new CustomEvent('global-create-task'))
}
</script>

<style scoped>
.plane-sidebar {
  width: 250px;
  background-color: var(--color-bg);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 999;
  height: 100vh;
  position: relative;
}

.plane-sidebar.collapsed { width: 0; border-right: none; overflow: hidden; }

.sidebar-scrollable { flex: 1; overflow-y: auto; padding: 16px 12px; }

.sidebar-top-action { margin-bottom: 20px; }

.new-work-btn {
  width: 100%;
  background: var(--color-surface);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.new-work-btn:hover { background: var(--color-surface-hover); border-color: var(--color-accent); }

.nav-section-title {
  font-size: 11px;
  color: var(--color-text-muted);
  text-transform: uppercase;
  font-weight: 700;
  letter-spacing: 0.05em;
  margin: 20px 8px 8px;
}

.flex-between { display: flex; justify-content: space-between; align-items: center; padding-right: 4px; }

.nav-menu { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 2px; }

.nav-link {
  display: flex;
  align-items: center;
  padding: 8px 10px;
  color: var(--color-text-secondary);
  font-size: 13.5px;
  font-weight: 500;
  border-radius: 2px;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s;
}

.nav-link i:first-child { width: 16px; font-size: 14px; margin-right: 12px; text-align: center; }

.nav-link:hover { background-color: var(--color-surface-hover); color: var(--color-text-primary); }

.nav-link.active {
  background-color: color-mix(in srgb, var(--color-accent) 10%, transparent);
  color: var(--color-accent);
  font-weight: 700;
}

.fav-icon { color: #f59e0b; }

.more-panel {
  position: absolute;
  top: 0;
  left: 250px;
  width: 250px;
  height: 100vh;
  background-color: var(--color-bg);
  border-right: 1px solid var(--color-border);
  padding: 16px 12px;
  z-index: 998;
  box-shadow: var(--shadow-xl);
}

.pin-icon { margin-left: auto; font-size: 11px; color: var(--color-text-muted); opacity: 0; }
.nav-link:hover .pin-icon { opacity: 1; }

.proj-folder { color: var(--color-text-primary); margin-bottom: 2px; }

.proj-icon {
  width: 20px; height: 20px; border-radius: 2px;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700; color: #fff; margin-right: 10px;
}

.sub-item .nav-link { padding-left: 28px; }

.sidebar-bottom { padding: 16px; border-top: 1px solid var(--color-border); }

.community-link {
  display: flex; align-items: center; gap: 8px;
  color: var(--color-text-secondary); font-size: 13px; text-decoration: none;
  padding: 6px; border-radius: 2px; transition: all 0.2s;
}

.community-link:hover { background: var(--color-surface-hover); color: var(--color-text-primary); }

.ms-auto { margin-left: auto; }

.slide-left-enter-active, .slide-left-leave-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.slide-left-enter-from, .slide-left-leave-to { transform: translateX(-100%); opacity: 0; }

.sidebar-scrollable::-webkit-scrollbar { width: 4px; }
.sidebar-scrollable::-webkit-scrollbar-thumb { background: transparent; border-radius: 10px; }
.sidebar-scrollable:hover::-webkit-scrollbar-thumb { background: var(--color-border); }
</style>
