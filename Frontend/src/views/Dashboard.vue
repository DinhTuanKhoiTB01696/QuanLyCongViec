<template>
  <div class="dashboard-layout">
    <!-- Navbar -->
    <header class="top-nav">
      <div class="nav-left">
        <button class="menu-toggle" @click="toggleSidebar">
          <i class="fa-solid fa-bars"></i>
        </button>
        <router-link to="/dashboard" class="nav-brand">
          <img :src="logoImg" alt="SprintA Logo" class="nav-logo" />
          <span>SprintA</span>
        </router-link>
        <span class="nav-link active desktop-only">Dự án</span>
      </div>

      <div class="nav-center desktop-only">
        <div class="top-search-create">
          <div class="search-input-mock">
            <i class="fa-solid fa-magnifying-glass"></i>
            <input type="text" placeholder="Tìm kiếm" v-model="searchQuery" />
          </div>
          <button class="btn-create-jira"><i class="fa-solid fa-plus"></i> Tạo mới</button>
        </div>
      </div>

      <div class="nav-right">
        <!-- Icon Robot mở popup AI -->
        <div class="nav-icon bot-icon" @click="toggleAI" :class="{ 'active': aiVisible }">
          <i class="fa-solid fa-robot"></i>
        </div>

        <NotificationsDropdown class="desktop-only" />
        <HelpDropdown class="desktop-only" />
        <SettingsDropdown class="desktop-only" />

        <UserDropdown />
      </div>
    </header>

    <div class="main-body">
      <!-- Sidebar -->
      <aside class="sidebar" :class="{ 'mobile-show': sidebarVisible }">

        <ul class="side-menu">
          <!-- Dynamic Main Sidebar Items -->
          <li class="active"><i class="fa-solid fa-border-all"></i> Dành cho bạn</li>
          <li v-if="sidebarPreferences.spaces" @click="goToDefaultSpace"><i class="fa-solid fa-folder-open"></i> Không gian</li>
          <li v-if="sidebarPreferences.recent"><i class="fa-solid fa-clock"></i> Gần đây</li>
          <li v-if="sidebarPreferences.ai" class="ai-item" @click="goToAI"><i class="fa-solid fa-robot"></i> Trợ lý AI</li>
          <li v-if="sidebarPreferences.audit" @click="router.push('/audit-log')"><i class="fa-solid fa-list-check"></i> Audit Log</li>
          <li v-if="sidebarPreferences.users" @click="router.push('/user-management')"><i class="fa-solid fa-users-gear"></i> Quản lý người dùng</li>
          
          <!-- More Dropdown -->
          <li class="more-dropdown-wrapper" style="padding: 0; background: transparent !important; margin-bottom: 4px;">
            <el-dropdown trigger="click" placement="bottom-start" popper-class="custom-sidebar-dropdown" style="width: 100%;">
              <div class="sidebar-more-trigger">
                <i class="fa-solid fa-ellipsis"></i> Thêm
              </div>
              <template #dropdown>
                <el-dropdown-menu class="jira-more-menu" style="background-color: var(--bg-card); border: 1px solid var(--border-color); border-radius: 4px; padding: 4px 0; width: 200px;">
                  <!-- Unselected items mapped to Dropdown -->
                  <el-dropdown-item v-if="!sidebarPreferences.spaces">
                    <div @click="goToDefaultSpace" style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-folder-open"></i>
                      <span>Không gian</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.recent">
                    <div style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-clock"></i>
                      <span>Gần đây</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.ai">
                    <div @click="goToAI" style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-robot"></i>
                      <span>Trợ lý AI</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.audit">
                    <div @click="router.push('/audit-log')" style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-list-check"></i>
                      <span>Audit Log</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.users">
                    <div @click="router.push('/user-management')" style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-users-gear"></i>
                      <span>Quản lý người dùng</span>
                    </div>
                  </el-dropdown-item>

                  <el-dropdown-item v-if="!sidebarPreferences.spaces || !sidebarPreferences.recent || !sidebarPreferences.ai || !sidebarPreferences.audit" divided></el-dropdown-item>

                  <el-dropdown-item>
                    <div @click="showCustomizeModal = true" style="display: flex; align-items: center; gap: 12px; color: var(--text-secondary); font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-sliders"></i>
                      <span>Customize sidebar</span>
                    </div>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </li>
        </ul>
      </aside>

      <!-- NEW Main Content: Jira 'For You' Mockup -->
      <main class="content-area">
        <div class="content-wrapper">
          <h1 class="page-title">Dành cho bạn</h1>

          <div class="section-header">
            <h3 class="section-title">Không gian gần đây</h3>
            <a href="#" class="view-all-link">Xem tất cả không gian</a>
          </div>

          <div class="recent-spaces-grid">
            <div v-if="spaces.length === 0" class="empty-state">
              <i class="fa-regular fa-folder-open"></i>
              <p>Chưa có không gian nào gần đây</p>
            </div>
            <div v-else class="space-card jira-card" v-for="space in spaces" :key="space.id" @click="goToSpace(space.id)">
              <div class="card-left-border" :style="{ backgroundColor: space.color }"></div>
              <div class="card-content">
                <div class="card-header">
                  <div class="space-icon-jira" :style="{ background: space.gradient }">
                    <i :class="space.icon"></i>
                  </div>
                  <div class="space-title-block">
                    <h4>{{ space.name }}</h4>
                    <span>{{ space.type }}</span>
                  </div>
                </div>
                
                <div class="quick-links-title">Liên kết nhanh</div>
                <div class="space-link-item">
                  <a href="#">Công việc đang mở của tôi</a>
                  <span class="badge-count">0</span>
                </div>
                
                <div class="board-dropdown">
                  0 bảng <i class="fa-solid fa-chevron-down"></i>
                </div>
              </div>
            </div>
          </div>

          <!-- Tabs -->
          <div class="jira-tabs scrollable-tabs">
            <div class="jira-tab active">Đã làm</div>
            <div class="jira-tab">Đã xem</div>
            <div class="jira-tab">Được giao cho tôi <span class="badge-count tab-badge">0</span></div>
            <div class="jira-tab">Đã đánh dấu sao</div>
            <div class="jira-tab">Bảng</div>
          </div>

          <div class="task-list-section">
            <div class="list-time-header">TRONG THÁNG QUA</div>
            
            <div v-if="tasks.length === 0" class="empty-state">
              <i class="fa-solid fa-square-check"></i>
              <p>Không tìm thấy công việc nào</p>
            </div>
            <div class="jira-task-row" v-for="task in tasks" :key="task.id">
              <div class="jira-task-left">
                <!-- Icon logic based on type -->
                <i v-if="task.isSubtask" class="fa-solid fa-diagram-project task-icon subtask-color"></i>
                <i v-else class="fa-solid fa-square-check task-icon done-color"></i>
                
                <div class="task-text">
                  <div class="task-name">{{ task.name }}</div>
                  <div class="task-meta">{{ task.id }} · {{ task.space }}</div>
                </div>
              </div>
              
              <div class="jira-task-right">
                <span class="task-action-text desktop-only">Đã tạo</span>
                <div class="user-avatar-small">DN</div>
              </div>
            </div>
          </div>
        </div>
      </main>

      <!-- Right AI Sidebar Popup -->
      <transition name="slide-right">
        <aside class="ai-sidebar" v-if="aiVisible">
          <div class="ai-header">
            <h4><i class="fa-solid fa-robot"></i> Trợ lý AI</h4>
            <button class="close-ai mobile-only" @click="toggleAI">
              <i class="fa-solid fa-xmark"></i>
            </button>
          </div>
          
          <div class="ai-content">
            <div class="quick-actions-title">THAO TÁC NHANH</div>
            <div class="quick-actions">
              <el-button size="small" round plain class="ai-chip">Tóm tắt công việc</el-button>
              <el-button size="small" round plain class="ai-chip">Gợi ý ưu tiên</el-button>
              <el-button size="small" round plain class="ai-chip">Soạn cập nhật</el-button>
            </div>

            <div class="chat-message bot">
              <div class="message-bubble">
                Theo dõi tiến độ là sức mạnh! Mình có thể giúp gì cho việc tổng kết các dealine sắp tới của bạn không?
              </div>
              <div class="message-meta">Trợ lý AI · Vừa xong</div>
            </div>
          </div>

          <div class="ai-input-area">
            <div class="ai-input-wrapper">
              <input type="text" placeholder="Hỏi AI để lấy ý tưởng cho công việc..." />
              <div class="ai-input-actions">
                <i class="fa-solid fa-paperclip"></i>
                <i class="fa-solid fa-at"></i>
                <button class="send-btn"><i class="fa-solid fa-paper-plane"></i></button>
              </div>
            </div>
          </div>
        </aside>
      </transition>
      
      <!-- Customize Sidebar Modal -->
      <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" @saved="handleSidebarSaved" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import logoImg from '../assets/logo_QLCV.png'
import HelpDropdown from '../components/HelpDropdown.vue'
import SettingsDropdown from '../components/SettingsDropdown.vue'
import NotificationsDropdown from '../components/NotificationsDropdown.vue'
import UserDropdown from '../components/UserDropdown.vue'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'
import axiosClient from '../api/axiosClient'
import { ElMessage } from 'element-plus'

const router = useRouter()
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const sidebarVisible = ref(false)
const aiVisible = ref(false)
const searchQuery = ref('')
const isLoading = ref(false)
const showCustomizeModal = ref(false)

const sidebarPreferences = ref({
  audit: true,
  users: true
})

onMounted(() => {
  const saved = localStorage.getItem('sidebarPreferences')
  if (saved) {
    try {
      Object.assign(sidebarPreferences.value, JSON.parse(saved))
    } catch (e) {}
  }
  fetchSpaces()
})

const handleSidebarSaved = (prefs) => {
  const newPrefs = { ...sidebarPreferences.value }
  if (prefs && prefs.navItems) {
    prefs.navItems.forEach(item => {
      if (['recent', 'spaces', 'ai', 'audit', 'users'].includes(item.id)) {
        newPrefs[item.id] = item.checked
      }
    })
  }
  sidebarPreferences.value = newPrefs
  localStorage.setItem('sidebarPreferences', JSON.stringify(newPrefs))
}

const toggleSidebar = () => {
  sidebarVisible.value = !sidebarVisible.value
}

const toggleAI = () => {
  aiVisible.value = !aiVisible.value
}

const goToDefaultSpace = () => {
  if (spaces.value.length > 0) {
    router.push(`/space/${spaces.value[0].id}`)
  } else {
    ElMessage.warning('Bạn chưa tham gia dự án nào')
  }
}

const goToSpace = (id) => {
  router.push(`/space/${id}`)
}

const goToAI = () => {
  router.push('/ai-assistant')
}

const spaces = ref([])
const tasks = ref([])

const fetchSpaces = async () => {
  isLoading.value = true
  try {
    const response = await axiosClient.get('/projects')
    // API C# thường trả về dạng { statusCode: 200, message: "Success", data: [...] }
    const projectList = response.data.data || response.data || []
    spaces.value = projectList.map(p => ({
      ...p,
      gradient: 'linear-gradient(135deg, #3b82f6, #2563eb)'
    }))
  } catch (error) {
    console.error('Fetch projects error:', error)
    if (error.response?.status !== 401) {
        ElMessage.error('Không thể tải danh sách không gian')
    }
  } finally {
    isLoading.value = false
  }
}



onMounted(fetchSpaces)
</script>

<style scoped>
/* Màu sắc chủ đạo '#020617' do User yêu cầu giữ cho Header & Sidebar */

.dashboard-layout {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-layout); 
  color: var(--text-primary); 
  overflow: hidden;
}

.top-nav {
  height: 56px;
  background-color: var(--bg-nav); 
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  flex-shrink: 0;
  z-index: 1000;
}

.nav-left, .nav-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.menu-toggle {
  display: none;
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-size: 20px;
  cursor: pointer;
  padding: 8px;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-primary);
  text-decoration: none;
  font-weight: 800;
  font-size: 20px;
  letter-spacing: -0.5px;
}

.nav-logo {
  height: 28px;
  width: auto;
}

.nav-link {
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 600;
  padding-left: 16px;
  border-left: 1px solid var(--border-color);
  cursor: pointer;
  transition: color 0.2s;
}

.nav-link:hover, .nav-link.active {
  color: var(--text-primary);
}

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}

.top-search-create {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  max-width: 1000px;
  justify-content: center;
}

.search-input-mock {
  display: flex;
  align-items: center;
  background-color: var(--bg-secondary);
  border: 1px solid var(--border-color); 
  border-radius: 4px;
  padding: 0 12px;
  flex: 1;
  max-width: 900px;
  height: 38px;
  transition: background-color 0.2s, border-color 0.2s;
}

.search-input-mock:focus-within {
  background-color: var(--input-bg);
  border-color: var(--border-color);
}

.search-input-mock i {
  color: var(--text-secondary);
  font-size: 14px;
  margin-right: 8px;
}

.search-input-mock input {
  background: transparent;
  border: none;
  color: var(--text-primary);
  font-size: 14px;
  width: 100%;
  outline: none;
}

.search-input-mock input::placeholder {
  color: var(--text-secondary);
}

.btn-create-jira {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background-color: #579dff; 
  color: #1d2125;
  border: none;
  border-radius: 4px;
  padding: 0 20px;
  height: 38px;
  font-size: 14px;
  font-weight: 600;
  white-space: nowrap;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-create-jira:hover {
  background-color: #85b8ff;
}

.nav-icon {
  color: var(--text-secondary);
  font-size: 18px;
  cursor: pointer;
  transition: color 0.2s;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
}

.nav-icon:hover {
  color: var(--text-primary);
  background-color: var(--hover-bg);
}

.nav-icon.active {
  color: #3b82f6; 
  background-color: var(--hover-bg);
}

.user-avatar {
  background: #f59e0b; /* Màu Cam/Vàng do user yêu cầu (chữ DN) */
  color: #1d2125;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
}

.main-body {
  display: flex;
  flex: 1;
  overflow: hidden;
  position: relative;
}

.sidebar {
  width: 240px;
  background-color: var(--bg-sidebar); 
  border-right: 1px solid var(--border-color);
  padding: 24px 16px;
  flex-shrink: 0;
  transition: transform 0.3s ease;
}

.side-menu {
  list-style: none;
  padding: 0;
  margin: 0;
}

.side-menu li {
  padding: 10px 12px;
  border-radius: 6px;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.2s;
}

.side-menu li i {
  font-size: 16px;
  width: 20px;
  text-align: center;
}

.side-menu li:hover {
  background-color: var(--hover-bg);
  color: var(--text-primary);
}

.side-menu li.active {
  background-color: var(--active-bg);
  color: var(--text-primary);
}

.sidebar-more-trigger {
  padding: 10px 12px;
  border-radius: 6px;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.sidebar-more-trigger i {
  font-size: 16px;
  width: 20px;
  text-align: center;
}

.sidebar-more-trigger:hover {
  background-color: var(--hover-bg);
  color: var(--text-primary);
}

.ai-item i {
  color: #3b82f6;
}

/* =========================================
   NEW "FOR YOU" CONTENT (Jira Dark Mode)
========================================== */

.content-area {
  flex: 1;
  background-color: var(--bg-content); /* Jira theme background */
  padding: 40px;
  overflow-y: auto;
}

.content-wrapper {
  max-width: 1200px;
  width: 100%;
  margin: 0 auto;
}

.page-title {
  font-size: 28px;
  color: var(--text-primary);
  font-weight: 600;
  margin-bottom: 32px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 12px;
}

.section-title {
  font-size: 18px;
  color: var(--text-primary);
  font-weight: 500;
  margin: 0;
}

.view-all-link {
  color: #579dff;
  text-decoration: none;
  font-size: 15px;
}
.view-all-link:hover { text-decoration: underline; }

.recent-spaces-grid {
  margin-bottom: 40px;
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
}

.space-card.jira-card {
  display: flex;
  width: 380px;
  background-color: var(--bg-card);
  border-radius: 4px;
  overflow: hidden;
  box-shadow: 0 1px 2px rgba(0,0,0,0.5);
  border: 1px solid var(--border-color);
  transition: background-color 0.2s;
  cursor: pointer;
}
.space-card.jira-card:hover {
  background-color: var(--hover-bg);
}

.card-left-border {
  width: 4px;
  background-color: #ff5c35; /* Orange-red border */
}

.card-content {
  padding: 16px;
  flex: 1;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.space-icon-jira {
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #ff7a59, #ff5c35); /* Grandient đỏ cam */
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 14px;
}

.space-title-block h4 {
  margin: 0;
  font-size: 16px;
  color: var(--text-primary);
  font-weight: 600;
}

.space-title-block span {
  font-size: 13px;
  color: var(--text-muted);
}

.quick-links-title {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 12px;
}

.space-link-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.space-link-item a {
  color: #b3bac5;
  text-decoration: underline;
  font-size: 14px;
}
.space-link-item a:hover {
  color: var(--text-primary);
}

.badge-count {
  background-color: var(--border-color);
  color: var(--text-secondary);
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 10px;
}

.board-dropdown {
  margin-top: 20px;
  font-size: 13px;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.jira-tabs {
  display: flex;
  gap: 28px;
  border-bottom: 1px solid var(--border-color);
  margin-bottom: 24px;
  overflow-x: auto;
  white-space: nowrap;
}

.jira-tabs::-webkit-scrollbar {
  display: none;
}

.jira-tab {
  padding: 12px 0;
  color: var(--text-muted);
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.jira-tab:hover {
  color: #b3bac5;
}

.jira-tab.active {
  color: #579dff;
}

.jira-tab.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background-color: #579dff;
}

.tab-badge {
  background-color: #f4f5f7;
  color: #1d2125;
}

.task-list-section {
  padding: 8px 0;
}

.list-time-header {
  font-size: 12px;
  font-weight: 600;
  color: #8c9bab;
  text-transform: uppercase;
  margin-bottom: 12px;
  letter-spacing: 0.5px;
}

.jira-task-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  border-radius: 4px;
  transition: background-color 0.1s;
  cursor: pointer;
  margin-bottom: 4px;
}

.jira-task-row:hover {
  background-color: #2c333a;
}

.jira-task-left {
  display: flex;
  align-items: flex-start;
  gap: 16px;
}

.task-icon {
  font-size: 18px;
  margin-top: 2px;
}
.subtask-color { color: #579dff; }
.done-color { color: #579dff; }

.task-text {
  display: flex;
  flex-direction: column;
}

.task-name {
  color: var(--text-primary);
  font-size: 15px;
  font-weight: 500;
  margin-bottom: 4px;
}

.task-meta {
  color: #8c9bab;
  font-size: 13px;
}

.jira-task-right {
  display: flex;
  align-items: center;
  gap: 16px;
  padding-right: 16px;
}

.task-action-text {
  color: #8c9bab;
  font-size: 12px;
}

.user-avatar-small {
  background-color: #f59e0b;
  color: #1d2125;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  width: 100%;
  color: #64748b;
  gap: 16px;
}

.empty-state i {
  font-size: 48px;
  opacity: 0.5;
}

.empty-state p {
  font-size: 15px;
}


/* =========================================
   AI SIDEBAR & HELP DROPDOWN (Giữ Nguyên)
========================================== */

.ai-sidebar {
  width: 420px;
  background-color: #0f172a;
  border-left: 1px solid #1e293b;
  display: flex;
  flex-direction: column;
}

.ai-header {
  padding: 24px;
  border-bottom: 1px solid #1e293b;
}

.ai-header h4 {
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.ai-header h4 i {
  color: #60a5fa; 
}

.ai-content {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
}

.quick-actions-title {
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
  margin-bottom: 12px;
  text-align: center;
}

.quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: center;
  margin-bottom: 32px;
}

.ai-chip {
  background-color: transparent !important;
  color: #94a3b8 !important;
  border-color: #334155 !important;
  font-size: 12px !important;
}

.ai-chip:hover {
  background-color: #1e3a8a !important;
  color: #60a5fa !important;
  border-color: #60a5fa !important;
}

.chat-message {
  margin-bottom: 20px;
}

.message-bubble {
  background-color: #1e293b;
  padding: 16px;
  border-radius: 12px 12px 12px 0;
  color: #e2e8f0;
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 8px;
}

.message-meta {
  font-size: 11px;
  color: #64748b;
  padding-left: 4px;
}

.ai-input-area {
  padding: 20px;
  background-color: #0f172a;
  border-top: 1px solid #1e293b;
}

.ai-input-wrapper {
  background-color: #1e293b;
  border: 1px solid #334155;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ai-input-wrapper input {
  background: transparent;
  border: none;
  padding: 16px;
  color: white;
  font-size: 14px;
  outline: none;
}

.ai-input-wrapper input::placeholder {
  color: #64748b;
}

.ai-input-actions {
  display: flex;
  align-items: center;
  padding: 8px 16px 12px;
  gap: 12px;
}

.ai-input-actions i {
  color: #64748b;
  cursor: pointer;
  transition: color 0.1s;
}

.ai-input-actions i:hover {
  color: white;
}

.send-btn {
  background: #3b82f6;
  border: none;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: auto;
  cursor: pointer;
  transition: opacity 0.2s;
}

.send-btn:hover {
  opacity: 0.9;
}

.send-btn i {
  color: white !important;
  font-size: 12px;
}

/* Animations */
.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 0.3s ease;
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
}

.help-footer a:hover {
  color: white;
  text-decoration: underline;
}
</style>

<style>
.custom-sidebar-dropdown.el-popper {
  background: #282e33 !important;
  border: 1px solid #333c43 !important;
  border-radius: 4px !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item {
  background-color: transparent !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item:hover,
.custom-sidebar-dropdown .el-dropdown-menu__item:focus {
  background-color: #3b444b !important;
}
.custom-sidebar-dropdown .el-popper__arrow::before {
  background: #282e33 !important;
  border: 1px solid #333c43 !important;
}
</style>
