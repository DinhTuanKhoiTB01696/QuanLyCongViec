<template>
  <aside class="nexus-sidebar" :class="{ 'collapsed': !isVisible }">
    <div class="sidebar-scrollable">
      <ul class="nav-menu">
        <!-- Home -->
        <li class="nav-item">
          <router-link to="/dashboard" class="nav-link" active-class="active">
            <i class="fa-solid fa-house"></i>
            <span>Dành cho bạn</span>
          </router-link>
        </li>
        
        <!-- Spaces -->
        <!-- Spaces -->
        <li class="nav-item has-action" v-if="localSidebarConfig.spaces">
          <div class="nav-link" :class="{ 'active': route.path.includes('/space') }" @click="handleSpacesClick">
            <i class="fa-solid fa-folder-open"></i>
            <span>Không gian</span>
            <div class="actions-group">
              <button class="action-btn btn-plus-blue" title="Create space" @click.stop="$emit('open-create-modal')">
                 <i class="fa-solid fa-plus"></i>
              </button>
              <button class="action-btn" :class="{'active': showSpacesMenu}" @click.stop="toggleSpacesMenu" title="More options">
                 <i class="fa-solid fa-ellipsis"></i>
              </button>
            </div>
          </div>
          
          <!-- Spaces Context Menu -->
          <ul v-if="showSpacesMenu" class="context-popup">
             <li @click="goToManageSpaces"><i class="fa-solid fa-gear"></i> Manage spaces</li>
             <div class="menu-divider"></div>
             <li @click.stop="hideSpacesFromSidebar"><i class="fa-solid fa-eye-slash"></i> Hide from sidebar</li>
          </ul>

          <!-- Sub-menu (Recent spaces) if expanded -->
          <ul v-if="spacesExpanded" class="dropdown-content">
             <li class="sub-link-title">Recent</li>
             <router-link v-for="sp in spaces.slice(0, 5)" :key="sp.id" :to="`/space/${sp.id}`" class="sub-link" active-class="active">
               <div class="space-icon-xs" :style="{ background: '#3b82f6' }">
                 <i class="fa-solid fa-folder" style="font-size: 10px; color: white;"></i>
               </div> 
               <span class="sub-link-text">{{ sp.name }}</span>
             </router-link>
             <li class="sub-link more-spaces" @click.stop="goToManageSpaces">
                <i class="fa-solid fa-list-ul"></i> More spaces
             </li>
          </ul>
        </li>

        <!-- Recent -->
        <li class="nav-item" v-if="localSidebarConfig.recent">
          <router-link to="/recent" class="nav-link" active-class="active">
            <i class="fa-solid fa-clock"></i>
            <span>Gần đây</span>
          </router-link>
        </li>

        <!-- AI Pages -->
        <li class="nav-item" v-if="localSidebarConfig.ai">
          <router-link to="/ai-assistant" class="nav-link" active-class="active">
            <i class="fa-solid fa-robot"></i>
            <span>Trợ lý AI</span>
          </router-link>
        </li>

        <!-- Audit Log -->
        <li class="nav-item" v-if="canManageSystem && localSidebarConfig.audit">
          <router-link to="/audit-log" class="nav-link" active-class="active">
            <i class="fa-solid fa-list-check"></i>
            <span>Audit Log</span>
          </router-link>
        </li>

        <!-- User Management -->
        <li class="nav-item" v-if="canManageUsers && localSidebarConfig.users">
          <router-link to="/user-management" class="nav-link" active-class="active">
            <i class="fa-solid fa-users-gear"></i>
            <span>Quản lý người dùng</span>
          </router-link>
        </li>

        <!-- More Nav Item -->
        <li class="nav-item has-action">
          <div class="nav-link" :class="{ 'active': showMoreMenu }" @click="showMoreMenu = !showMoreMenu">
            <i class="fa-solid fa-ellipsis"></i>
            <span>More</span>
          </div>

          <!-- More Dropdown Menu -->
          <ul v-if="showMoreMenu" class="dropdown-content more-dropdown">
             <!-- Hidden Items -->
             <li v-if="!localSidebarConfig.recent" class="sub-link hidden-nav" @click="router.push('/recent')">
                <i class="fa-regular fa-clock"></i> Recent
             </li>
             <li v-if="!localSidebarConfig.spaces" class="sub-link hidden-nav" @click="handleSpacesClick">
                <i class="fa-regular fa-folder-open"></i> Spaces
             </li>
             <li v-if="!localSidebarConfig.ai" class="sub-link hidden-nav" @click="router.push('/ai-assistant')">
                <i class="fa-solid fa-robot"></i> Trợ lý AI
             </li>
             <li v-if="canManageSystem && !localSidebarConfig.audit" class="sub-link hidden-nav" @click="router.push('/audit-log')">
                <i class="fa-solid fa-list-check"></i> Audit Log
             </li>
             <li v-if="canManageUsers && !localSidebarConfig.users" class="sub-link hidden-nav" @click="router.push('/user-management')">
                <i class="fa-solid fa-users-gear"></i> Quản lý người dùng
             </li>
             
             <!-- Divider if there are any hidden links -->
             <div class="menu-divider" v-if="hasHiddenItems" style="margin: 4px 12px; border-top: 1px solid var(--border-color);"></div>
             
             <!-- Customize sidebar option -->
             <li class="sub-link customize-link" @click.stop="openCustomizeModal">
                <i class="fa-solid fa-sliders"></i> Customize sidebar
             </li>
          </ul>
        </li>
      </ul>
    </div>

    <!-- Bottom Actions -->
    <div class="sidebar-bottom">
      <a href="#" class="help-link">
        <i class="fa-regular fa-circle-question"></i> Help and Support
      </a>
    </div>

    <!-- Customize Sidebar Modal -->
    <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" />
  </aside>
</template>

<script setup>
import { ref, reactive, onMounted, computed, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'
import CustomizeSidebarModal from '../CustomizeSidebarModal.vue'

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['close-mobile', 'open-create-modal'])

const router = useRouter()
const route = useRoute()

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const isAdmin = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Admin') || roles.includes('admin')
})

const isPM = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Manager') || roles.includes('manager') || roles.includes('PM')
})

const canManageUsers = computed(() => isAdmin.value || isPM.value)
const canManageSystem = computed(() => isAdmin.value || isPM.value)

const openMenus = reactive({
  taskManager: false,
  projects: true,
  organization: route.path.includes('/user-management') || route.path.includes('/audit-log')
})

const toggleMenu = (menu) => {
  openMenus[menu] = !openMenus[menu]
}

const spaces = ref([])
const showSpacesMenu = ref(false)
const spacesExpanded = ref(route.path.includes('/space'))

const fetchSpaces = async () => {
  try {
    const response = await axiosClient.get('/projects')
    spaces.value = response.data.data || response.data || []
  } catch (error) {
    console.error('Fetch projects error:', error)
  }
}

const handleSpacesClick = () => {
    if (spaces.value.length === 0) {
       ElMessage.error('Bạn chưa tham gia không gian nào.')
    } else {
       spacesExpanded.value = !spacesExpanded.value
    }
}

const toggleSpacesMenu = () => {
   showSpacesMenu.value = !showSpacesMenu.value
}

const goToManageSpaces = () => {
    showSpacesMenu.value = false
    router.push('/spaces')
}

const closeSpacesMenu = (e) => {
   if (!e.target.closest('.has-action')) {
     showSpacesMenu.value = false
     showMoreMenu.value = false
   }
}

// Sidebar Config state logic
const showCustomizeModal = ref(false)
const showMoreMenu = ref(false)

const openCustomizeModal = () => {
    showCustomizeModal.value = true
    showMoreMenu.value = false
}

const localSidebarConfig = ref({
   spaces: true,
   recent: true,
   ai: true,
   audit: true,
   users: true
})

const hasHiddenItems = computed(() => {
   return !localSidebarConfig.value.spaces || !localSidebarConfig.value.recent || 
          !localSidebarConfig.value.ai     || !localSidebarConfig.value.audit || 
          !localSidebarConfig.value.users
})

const loadSidebarConfig = () => {
   const saved = localStorage.getItem('sidebarPreferences')
   if (saved) {
      try {
         const parsed = JSON.parse(saved)
         Object.keys(localSidebarConfig.value).forEach(k => {
            if (parsed[k] !== undefined) {
               localSidebarConfig.value[k] = parsed[k]
            }
         })
      } catch(e) {}
   }
}

const hideSpacesFromSidebar = () => {
   localSidebarConfig.value.spaces = false
   showSpacesMenu.value = false
   
   // Sync to localStorage
   const saved = localStorage.getItem('sidebarPreferences')
   const parsed = saved ? JSON.parse(saved) : {}
   parsed.spaces = false
   localStorage.setItem('sidebarPreferences', JSON.stringify(parsed))
   
   // Notify everyone 
   window.dispatchEvent(new Event('sidebar-updated'))
}

onMounted(() => {
  window.addEventListener('click', closeSpacesMenu)
  window.addEventListener('sidebar-updated', loadSidebarConfig)
  loadSidebarConfig()
  fetchSpaces()
})

onUnmounted(() => {
  window.removeEventListener('click', closeSpacesMenu)
  window.removeEventListener('sidebar-updated', loadSidebarConfig)
})
</script>

<style scoped>
.nexus-sidebar {
  width: 240px;
  background-color: var(--bg-nav);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 999;
  overflow: hidden;
}

.nexus-sidebar.collapsed {
  width: 0;
  opacity: 0;
  border-right: none;
}

.sidebar-scrollable {
  flex: 1;
  overflow-y: auto;
  padding: 20px 12px;
}

.nav-menu {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  position: relative;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  border-radius: 8px;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s ease;
}

.nav-link i:first-child {
  width: 20px;
  font-size: 16px;
  margin-right: 12px;
  text-align: center;
}

.nav-link:hover {
  background-color: var(--hover-bg);
  color: var(--text-primary);
}

/* Active State requested by User: xanh nhạt background + xanh icon */
.nav-link.active, .nav-link.router-link-exact-active {
  background-color: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
  font-weight: 600;
}

.nav-link.active i {
  color: #3b82f6;
}

.arrow {
  margin-left: auto;
  font-size: 12px !important;
  transition: transform 0.2s;
}
.arrow.rotated {
  transform: rotate(180deg);
}

.has-action {
  position: relative;
}
.has-action .nav-link {
  width: 100%;
  position: relative; /* Fixes the button jumping bug by anchoring the button to the link itself */
}
.actions-group {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 4px;
  z-index: 10;
}
.action-btn {
  background: transparent;
  border: 1px solid transparent;
  color: var(--text-muted);
  cursor: pointer;
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: flex; /* Always show the button */
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.nav-item.has-action:hover .action-btn {
  color: var(--text-primary);
}
.action-btn.btn-plus-blue {
  color: #3b82f6;
}
.action-btn.btn-plus-blue:hover {
  background-color: rgba(59, 130, 246, 0.1);
}
.action-btn:hover {
  background-color: var(--border-color);
  color: var(--text-primary);
}
.action-btn.active {
  background-color: rgba(0, 82, 204, 0.08); /* Jira active blue tint background */
  border: 1px solid #0052cc; /* Blue border */
  color: #0052cc !important;
}

.context-popup {
  position: absolute;
  top: 42px; /* Fixed under the nav item */
  right: 8px; /* Align right under the three-dots button */
  width: 220px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 4px; /* Standard Jira radius */
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.15); /* Jira elevation shadow */
  padding: 4px 0;
  z-index: 1000;
  list-style: none;
  margin: 0;
}
.menu-divider {
  height: 1px;
  background-color: var(--border-color);
  margin: 4px 0;
}
.context-popup li {
  padding: 8px 16px;
  font-size: 13px;
  color: var(--text-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
}
.context-popup li i {
  color: var(--text-muted);
  font-size: 14px;
}
.context-popup li:hover {
  background-color: var(--hover-bg);
}
.context-popup li .arrow-right {
  margin-left: auto;
  font-size: 10px;
}

.dropdown-content {
  display: flex;
  flex-direction: column;
  padding-left: 20px;
  margin-top: 4px;
  gap: 2px;
  list-style: none;
}

.sub-link-title {
  font-size: 11px;
  text-transform: uppercase;
  color: var(--text-muted);
  font-weight: 600;
  padding: 8px 12px;
  letter-spacing: 0.5px;
}

.sub-link {
  color: var(--text-secondary);
  font-size: 13px;
  padding: 8px 12px;
  border-radius: 6px;
  text-decoration: none;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 10px;
}
.space-icon-xs {
  width: 20px;
  height: 20px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.more-spaces {
  cursor: pointer;
}
.more-spaces i {
  color: var(--text-muted);
  margin-left: 2px;
}

.dropdown-content {
  display: flex;
  flex-direction: column;
  padding-left: 44px;
  margin-top: 2px;
  gap: 2px;
}

.sub-link {
  color: var(--text-muted);
  font-size: 13px;
  padding: 8px 12px;
  border-radius: 6px;
  text-decoration: none;
  transition: all 0.2s;
}

.sub-link:hover, .sub-link.active, .sub-link.router-link-exact-active {
  color: var(--text-primary);
  background-color: var(--hover-bg);
}

.text-muted {
  color: var(--text-muted);
  font-size: 13px;
  padding: 8px 12px;
}

.sidebar-bottom {
  padding: 20px 16px;
  border-top: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.btn-create-project {
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 999px; /* Tròn */
  padding: 10px 0;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s, transform 0.1s;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.btn-create-project:hover {
  background-color: #2563eb;
  transform: translateY(-1px);
}

.btn-create-project:active {
  transform: translateY(1px);
}

.help-link {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-muted);
  font-size: 13px;
  text-decoration: none;
  font-weight: 500;
  justify-content: center;
}

.help-link:hover {
  color: var(--text-primary);
}

@media (max-width: 1024px) {
  .nexus-sidebar {
    position: fixed;
    left: -240px;
    width: 240px;
    opacity: 1;
    top: 64px;
    bottom: 0;
    transition: left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }
  .nexus-sidebar.collapsed {
    width: 240px;
    border-right: 1px solid var(--border-color);
  }
  .nexus-sidebar:not(.collapsed) {
    left: 0;
  }
}
</style>
