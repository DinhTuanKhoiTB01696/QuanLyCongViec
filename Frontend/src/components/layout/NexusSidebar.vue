<template>
  <aside class="plane-sidebar" :class="{ 'collapsed': !isVisible }">
    <div class="sidebar-scrollable">
      <div class="sidebar-top-action">
        <button class="new-issue-btn" @click="triggerCreateTask">
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
             <i class="fa-solid fa-arrows-spin text-orange-400"></i>
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
        <i class="fa-solid fa-chevron-down" style="font-size: 10px; cursor: pointer;"></i>
      </div>
      <ul class="nav-menu">
        <template v-for="project in projectTree" :key="project.id">
          <li class="nav-item">
            <div
              class="nav-link proj-folder"
              :class="{ active: currentProjectId === project.id }"
              @click="toggleProject(project.id)"
              @mouseenter="prefetchProject(project.id)"
              @focusin="prefetchProject(project.id)"
            >
              <span class="proj-icon">{{ projectIcon(project) }}</span>
              <span class="truncate">{{ project.name }}</span>
              <i class="fa-solid ms-auto" :class="project.expanded ? 'fa-chevron-down' : 'fa-chevron-right'" style="font-size: 10px;"></i>
            </div>
          </li>

          <li v-for="child in project.children" v-show="project.expanded" :key="child.id" class="nav-item sub-item">
            <router-link :to="child.route" class="nav-link" active-class="active" @mouseenter="prefetchProject(project.id)" @focusin="prefetchProject(project.id)">
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
  isVisible: {
    type: Boolean,
    default: true
  }
})
const emit = defineEmits(['close-mobile'])

const sprintStore = useSprintStore()
const projectTree = computed(() => projectStore.projectTree)
const favoriteSprints = computed(() => {
   if (!sprintStore.sprints) return [];
   return sprintStore.sprints.filter(s => s.isFavorite);
})

const isSpaceRoute = computed(() => route.path.startsWith('/space/'))

watch(currentProjectId, async (newVal, oldVal) => {
   if (newVal && newVal !== 'default') {
      if (newVal !== oldVal) {
        projectStore.expandProject(newVal)
      }
      localStorage.setItem('currentProjectId', newVal)
      localStorage.setItem('lastProjectId', newVal)
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

const prefetchProject = (projectId) => {
  if (!projectId || projectId === 'default') return
  projectStore.prefetchProjectBundle(projectId).catch(() => {})
  sprintStore.fetchSprints(projectId).catch(() => {})
}

const childIcon = (key) => ({
  'work-items': 'fa-solid fa-layer-group',
  'cycles': 'fa-solid fa-arrows-spin',
  'modules': 'fa-solid fa-table-cells-large',
  'views': 'fa-solid fa-list-ul',
  'pages': 'fa-regular fa-file-lines'
}[key] || 'fa-solid fa-chevron-right')

const projectIcon = (project) => project.icon || project.name?.charAt(0)?.toUpperCase() || 'P'

const triggerCreateTask = async () => {
  const projects = projectStore.allProjects.length
    ? projectStore.allProjects
    : await projectStore.fetchAllProjects()

  if (!projects.length) {
    ElMessage.warning('Bạn cần tạo project trước khi tạo work item.')
    await router.push('/spaces')
    return
  }

  const preferredProjectId = projects.some(project => project.id === currentProjectId.value)
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
  background-color: #0d0f11;
  border-right: 1px solid #1e2025;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 999;
  height: 100vh;
  position: relative;
}

.plane-sidebar.collapsed {
  width: 0;
  border-right: none;
  overflow: hidden;
}

.sidebar-scrollable {
  flex: 1;
  overflow-y: auto;
  padding: 16px 12px;
}

.sidebar-top-action {
  margin-bottom: 20px;
}

.new-issue-btn {
  width: 100%;
  background: #1e2025;
  color: #e4e4e7;
  border: 1px solid #27272a;
  border-radius: 6px;
  padding: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.new-issue-btn:hover {
  background: #27272a;
}

.new-issue-btn i {
  font-size: 14px;
}

.nav-section-title {
  font-size: 11px;
  color: #71717a;
  text-transform: uppercase;
  font-weight: 600;
  letter-spacing: 0.5px;
  margin: 20px 8px 8px;
}

.flex-between {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-right: 4px;
}

.nav-menu {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 6px 10px;
  color: #a1a1aa;
  font-size: 13.5px;
  font-weight: 500;
  border-radius: 6px;
  text-decoration: none;
  cursor: pointer;
  transition: background 0.2s;
}

.nav-link i:first-child {
  width: 16px;
  font-size: 14px;
  margin-right: 12px;
  text-align: center;
}

.nav-link:hover {
  background-color: #1e2025;
  color: #e4e4e7;
}

.nav-link.active {
  background-color: #1e2025;
  color: #e4e4e7;
}

.nav-link.dropdown-active {
  border: 1px solid #71717A;
  background: #16181D;
  color: #E4E4E7;
}

/* Secondary Panel */
.more-panel {
  position: absolute;
  top: 0;
  left: 250px;
  width: 250px;
  height: 100vh;
  background-color: #0d0f11;
  border-right: 1px solid #1e2025;
  padding: 16px 12px;
  z-index: 998;
}

.pin-icon {
  margin-left: auto;
  font-size: 11px;
  color: #71717A;
  opacity: 0;
  transition: opacity 0.2s;
}
.nav-link:hover .pin-icon {
  opacity: 1;
}

.slide-left-enter-active, .slide-left-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.slide-left-enter-from, .slide-left-leave-to {
  transform: translateX(-100%);
  opacity: 0;
}

.proj-folder {
  color: #e4e4e7;
  margin-bottom: 2px;
}

.proj-icon {
  background: #0ea5e9;
  color: white;
  width: 18px;
  height: 18px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
  margin-right: 10px;
}

.sub-item .nav-link {
  padding-left: 28px;
}

.sidebar-bottom {
  padding: 16px;
  border-top: 1px solid #1e2025;
}

.community-link {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #a1a1aa;
  font-size: 13px;
  text-decoration: none;
  font-weight: 500;
  padding: 6px;
  border-radius: 6px;
  transition: background 0.2s;
}

.community-link:hover {
  background: #1e2025;
  color: #e4e4e7;
}

/* Scrollbar customization */
.sidebar-scrollable::-webkit-scrollbar {
  width: 4px;
}
.sidebar-scrollable::-webkit-scrollbar-thumb {
  background: transparent;
  border-radius: 10px;
}
.sidebar-scrollable:hover::-webkit-scrollbar-thumb {
  background: #27272a;
}
</style>
