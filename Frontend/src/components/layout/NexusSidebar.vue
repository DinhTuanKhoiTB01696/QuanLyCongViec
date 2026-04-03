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
        <li class="nav-item">
          <router-link to="/space/my-team" class="nav-link" active-class="active">
            <i class="fa-solid fa-folder-open"></i>
            <span>Không gian</span>
          </router-link>
        </li>

        <!-- Recent -->
        <li class="nav-item">
          <router-link to="/recent" class="nav-link" active-class="active">
            <i class="fa-solid fa-clock"></i>
            <span>Gần đây</span>
          </router-link>
        </li>

        <!-- AI Pages -->
        <li class="nav-item">
          <router-link to="/ai-assistant" class="nav-link" active-class="active">
            <i class="fa-solid fa-robot"></i>
            <span>Trợ lý AI</span>
          </router-link>
        </li>

        <!-- Audit Log -->
        <li class="nav-item">
          <router-link to="/audit-log" class="nav-link" active-class="active">
            <i class="fa-solid fa-list-check"></i>
            <span>Audit Log</span>
          </router-link>
        </li>

        <!-- User Management -->
        <li class="nav-item">
          <router-link to="/user-management" class="nav-link" active-class="active">
            <i class="fa-solid fa-users-gear"></i>
            <span>Quản lý người dùng</span>
          </router-link>
        </li>
      </ul>
    </div>

    <!-- Bottom Actions -->
    <div class="sidebar-bottom">
      <a href="#" class="help-link">
        <i class="fa-regular fa-circle-question"></i> Help and Support
      </a>
    </div>
  </aside>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['close-mobile'])

const router = useRouter()
const route = useRoute()

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const isAdmin = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Admin') || roles.includes('admin')
})

const openMenus = reactive({
  taskManager: false,
  projects: true,
  organization: route.path.includes('/user-management') || route.path.includes('/audit-log')
})

const toggleMenu = (menu) => {
  openMenus[menu] = !openMenus[menu]
}

const spaces = ref([])

const fetchSpaces = async () => {
  try {
    const response = await axiosClient.get('/projects')
    spaces.value = response.data.data || response.data || []
  } catch (error) {
    console.error('Fetch projects error:', error)
  }
}

onMounted(() => {
  fetchSpaces()
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
  display: flex;
  align-items: center;
  position: relative;
}
.has-action .nav-link {
  flex: 1;
}
.action-btn {
  position: absolute;
  right: 12px;
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: none;
}
.nav-item:hover .action-btn {
  display: block;
}
.action-btn:hover {
  background-color: var(--border-color);
  color: var(--text-primary);
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
