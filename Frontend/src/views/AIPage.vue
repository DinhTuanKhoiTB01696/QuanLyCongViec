<template>
  <div class="dashboard-layout">
    <!-- Top Nav removed as per request for a cleaner AI experience -->

    <div class="main-body">
      <!-- Sidebar -->
      <aside class="sidebar" :class="{ 'show': sidebarVisible }">
        <ul class="side-menu">
          <li @click="goToDashboard"><i class="fa-solid fa-border-all"></i> Dành cho bạn</li>
          <li v-if="sidebarPreferences.spaces" @click="goToSpace"><i class="fa-regular fa-folder-open"></i> Không gian</li>
          <li v-if="sidebarPreferences.recent"><i class="fa-regular fa-clock"></i> Gần đây</li>
          <li v-if="sidebarPreferences.ai" class="active"><i class="fa-solid fa-robot"></i> Trợ lý AI</li>
          <li v-if="sidebarPreferences.audit && isAdmin" @click="router.push('/audit-log')"><i class="fa-solid fa-list-check"></i> Audit Log</li>
          <li v-if="sidebarPreferences.users" @click="router.push('/user-management')"><i class="fa-solid fa-users-gear"></i> Quản lý người dùng</li>
          <li class="more-dropdown-wrapper" style="padding: 0; background: transparent !important; margin-bottom: 4px;">
            <el-dropdown trigger="click" placement="bottom-start" popper-class="custom-sidebar-dropdown" style="width: 100%;">
              <div class="sidebar-more-trigger">
                <i class="fa-solid fa-ellipsis"></i> Thêm
              </div>
              <template #dropdown>
                <el-dropdown-menu class="jira-more-menu" style="background-color: var(--bg-card); border: 1px solid var(--border-color); border-radius: 4px; padding: 4px 0; width: 200px;">
                  <!-- Unselected items mapped to Dropdown -->
                  <el-dropdown-item v-if="!sidebarPreferences.spaces">
                    <div @click="goToSpace" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-regular fa-folder-open"></i>
                      <span>Không gian</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.recent">
                    <div @click="goToDashboard" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-regular fa-clock"></i>
                      <span>Gần đây</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.ai">
                    <div @click="toggleAIView" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-robot"></i>
                      <span>Trợ lý AI</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.audit && isAdmin">
                    <div @click="router.push('/audit-log')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-list-check"></i>
                      <span>Audit Log</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.users">
                    <div @click="router.push('/user-management')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-users-gear"></i>
                      <span>Quản lý người dùng</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.spaces || !sidebarPreferences.recent || !sidebarPreferences.ai || !sidebarPreferences.audit" divided></el-dropdown-item>

                  <el-dropdown-item>
                    <div @click="showCustomizeModal = true" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-sliders"></i>
                      <span>Customize sidebar</span>
                    </div>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </li>
        </ul>
        <div class="sidebar-footer">
          <div class="user-card-mini">
            <UserDropdown />
            <div class="user-info">
              <div class="name">Người dùng</div>
              <div class="plan">Gói miễn phí</div>
            </div>
          </div>
        </div>
      </aside>

      <!-- AI Content Area -->
      <main class="content-area">
        <div class="ai-container">
          <div class="ai-page-header">
            <div class="header-left">
              <div class="menu-toggle mobile-only" @click="sidebarVisible = !sidebarVisible">
                <i class="fa-solid fa-bars"></i>
              </div>
              <h2 class="page-title">Trợ lý AI</h2>
            </div>
          </div>

          <!-- Chat History -->
          <div class="chat-history">
            <!-- Bot Welcome -->
            <div class="chat-row bot">
              <div class="bot-icon-circle"><i class="fa-solid fa-robot"></i></div>
              <div class="bubble">
                Xin chào! Tôi là trợ lý AI của SprintA. Tôi có thể giúp bạn tổ chức dự án, tóm tắt các luồng thảo luận dài, hoặc tạo lộ trình. Tôi có thể giúp gì cho bạn hôm nay?
              </div>
            </div>
          </div>

          <!-- Chat Input -->
          <div class="ai-chat-input-wrapper">
            <div class="input-box">
              <i class="fa-solid fa-paperclip attach-btn"></i>
              <input type="text" placeholder="Hỏi SprintA AI bất cứ điều gì..." />
              <button class="send-btn"><span class="fa fa-paper-plane"></span></button>
            </div>
            <div class="ai-disclaimer">SprintA AI có thể mắc sai sót. Hãy kiểm tra lại các thông tin quan trọng.</div>
          </div>
        </div>
      </main>

      <!-- Right Sidebar: AI Details -->
      <aside class="ai-details-panel">
        <div class="panel-section">
          <div class="section-label">Trợ lý AI</div>
          <div class="section-title">HÀNH ĐỘNG NHANH</div>
          <div class="quick-links">
            <div class="q-link"><i class="fa-solid fa-map-location-dot"></i> Tạo lộ trình</div>
            <div class="q-link"><i class="fa-regular fa-file-lines"></i> Tóm tắt công việc</div>
            <div class="q-link"><i class="fa-solid fa-pen-nib"></i> Soạn bản cập nhật</div>
          </div>
        </div>

        <div class="panel-section mt-30">
          <div class="section-title">NGỮ CẢNH GẦN ĐÂY</div>
          <div class="context-item">
            <div class="c-icon blue"><i class="fa-solid fa-layer-group"></i></div>
            <div class="c-text">
              <div class="c-name">Mô-đun xác thực</div>
              <div class="c-time">Cập nhật 2 giờ trước</div>
            </div>
          </div>
          <div class="context-item">
            <div class="c-icon purple"><i class="fa-solid fa-database"></i></div>
            <div class="c-text">
              <div class="c-name">Backend API</div>
              <div class="c-time">Chờ đồng bộ</div>
            </div>
          </div>
        </div>

        <!-- Upgrade Card -->
        <div class="upgrade-card-wrapper">
          <div class="upgrade-card">
            <div class="plan-label">GÓI PRO</div>
            <div class="plan-desc">Mở khóa các truy vấn AI không giới hạn và quy trình làm việc tùy chỉnh.</div>
            <button class="btn-upgrade">Nâng cấp ngay</button>
          </div>
        </div>
      </aside>
    </div>

    <!-- AI Sidebar (Reuse Popup component logic) -->
    <transition name="slide-right">
      <aside class="ai-sidebar popup" v-if="aiVisible">
        <div class="ai-header">
          <h4><i class="fa-solid fa-robot"></i> AI Chat</h4>
          <i class="fa-solid fa-xmark" style="color: #64748b; cursor: pointer" @click="toggleAI"></i>
        </div>
        <div class="ai-content">
          <div class="chat-message bot">
            <div class="avatar-bot"><i class="fa-solid fa-robot"></i></div>
            <div class="message-bubble">Bạn đang ở trang trợ lý AI toàn màn hình. Bạn có thể sử dụng cửa sổ nhỏ này để chat nhanh khi đang ở các trang khác!</div>
          </div>
        </div>
      </aside>
    </transition>

    <!-- Customize Sidebar Modal -->
    <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" @saved="handleSidebarSaved" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import logoImg from '../assets/logo_QLCV.png'
import HelpDropdown from '../components/HelpDropdown.vue'
import SettingsDropdown from '../components/SettingsDropdown.vue'
import NotificationsDropdown from '../components/NotificationsDropdown.vue'
import UserDropdown from '../components/UserDropdown.vue'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'

const router = useRouter()
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const isAdmin = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Admin') || roles.includes('admin')
})
const searchQuery = ref('')
const aiVisible = ref(false)
const showCustomizeModal = ref(false)
const sidebarVisible = ref(true)

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

const toggleAI = () => {
  aiVisible.value = !aiVisible.value
}

const toggleAIView = () => {
  // Toggle the popup even on this page if requested
  toggleAI()
}


const goToDashboard = () => {
  router.push('/dashboard')
}

const goToSpace = () => {
  router.push('/space/my-team')
}
</script>

<style scoped>
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
}

.nav-left, .nav-right {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 300px; /* Keep sides fixed width to help center */
}

.nav-right {
  justify-content: flex-end;
}

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-primary);
  text-decoration: none;
  font-weight: 800;
  font-size: 20px;
}
.nav-logo { height: 28px; }

.nav-link { font-size: 14px; color: var(--text-secondary); border-left: 1px solid var(--border-color); padding-left: 16px; }

.top-search-create {
  display: flex;
  align-items: center;
  gap: 8px; 
}

.search-input-mock {
  display: flex;
  align-items: center;
  background-color: var(--bg-secondary);
  border: 1px solid var(--border-color); 
  border-radius: 4px;
  padding: 0 12px;
  width: 550px;
  height: 32px;
}
.search-input-mock i { color: var(--text-secondary); font-size: 14px; margin-right: 8px; }
.search-input-mock input { background: transparent; border: none; color: var(--text-primary); font-size: 14px; width: 100%; outline: none; }

.btn-create-jira {
  background-color: #579dff; color: #1d2125; border: none; border-radius: 4px;
  padding: 0 16px; height: 32px; font-size: 14px; font-weight: 500; cursor: pointer;
}

.nav-icon {
  color: var(--text-secondary); font-size: 18px; cursor: pointer; width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center; border-radius: 50%;
}
.nav-icon:hover { background-color: var(--hover-bg); color: var(--text-primary); }
.nav-icon.active { color: #3b82f6; background-color: var(--hover-bg); }

.user-avatar {
  background: #fdbba7; color: #1d2125; width: 32px; height: 32px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 12px;
}

.main-body { display: flex; flex: 1; overflow: hidden; }

.sidebar { width: 260px; background-color: var(--bg-sidebar); border-right: 1px solid var(--border-color); padding: 16px; display: flex; flex-direction: column;}
.side-menu { list-style: none; padding: 0; margin: 0 0 20px 0; }
.side-menu li { padding: 10px 12px; border-radius: 6px; color: var(--text-secondary); font-size: 14px; font-weight: 500; margin-bottom: 4px; cursor: pointer; display: flex; align-items: center; gap: 12px; }
.side-menu li:hover { background-color: var(--hover-bg); color: var(--text-primary); }
.side-menu li.active { background-color: var(--active-bg); color: #579dff; }

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

.sidebar-section { margin-top: 20px; }
.section-label { font-size: 11px; color: var(--text-secondary); font-weight: 700; letter-spacing: 0.5px; padding: 8px 12px; text-transform: uppercase; }
.dot { width: 8px; height: 8px; border-radius: 50%; }
.dot.blue { background: #3b82f6; }
.dot.orange { background: #f97316; }

.sidebar-footer { margin-top: auto; border-top: 1px solid var(--border-color); padding-top: 16px; }
.user-card-mini { display: flex; align-items: center; gap: 12px; padding: 8px; border-radius: 8px; cursor: pointer; }
.user-card-mini:hover { background: var(--hover-bg); }
.user-avatar-small { width: 28px; height: 28px; background: #579dff; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #1d2125; }
.user-info { flex: 1; }
.user-info .name { font-size: 13px; font-weight: 600; color: var(--text-primary); }
.user-info .plan { font-size: 11px; color: var(--text-secondary); }

.content-area { flex: 1; background-color: var(--bg-content); display: flex; justify-content: center; padding: 0; overflow-y: auto;}
.ai-container { width: 100%; max-width: 900px; display: flex; flex-direction: column; height: 100%; }

.ai-page-header { 
  padding: 24px 40px; 
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}
.page-title { font-size: 24px; font-weight: 700; color: var(--text-primary); margin: 0; }

.chat-history { flex: 1; padding: 40px; display: flex; flex-direction: column; gap: 32px; overflow-y: auto; }
.chat-row { display: flex; gap: 16px; max-width: 85%; }
.chat-row.user { align-self: flex-end; flex-direction: row-reverse; }

.bot-icon-circle { width: 32px; height: 32px; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 4px; display: flex; align-items: center; justify-content: center; color: #579dff; flex-shrink: 0; }
.user-avatar-circle { width: 32px; height: 32px; background: #579dff; border-radius: 4px; display: flex; align-items: center; justify-content: center; color: var(--bg-nav); font-weight: 700; font-size: 11px; flex-shrink: 0; }

.bubble { 
  background: var(--bg-secondary); 
  padding: 16px 20px; 
  border-radius: 12px; 
  color: var(--text-primary); 
  font-size: 14px; 
  line-height: 1.6; 
  border: 1px solid var(--border-color);
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  transition: all 0.2s ease;
}
.bubble.primary { 
  background: #3b82f6; 
  color: white; 
  border: none;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
}
.chat-row { display: flex; gap: 16px; max-width: 85%; align-items: flex-start; }

.draft-box { background: var(--bg-layout); border: 1px solid var(--border-color); border-radius: 8px; padding: 16px; margin: 12px 0; font-family: 'Inter', sans-serif; font-size: 14px; color: var(--text-primary); }

.ai-chat-input-wrapper { padding: 24px 40px 32px; }
.input-box { background: var(--bg-secondary); border: 1px solid var(--border-color); display: flex; align-items: center; padding: 12px 20px; gap: 12px; border-radius: 8px; }
.input-box input { flex: 1; background: transparent; border: none; color: var(--text-primary); outline: none; font-size: 14px; }
.attach-btn { color: #94a3b8; cursor: pointer; }
.send-btn { background: #579dff; border: none; width: 32px; height: 32px; border-radius: 4px; color: #1d2125; cursor: pointer; }

.ai-disclaimer { text-align: center; font-size: 11px; color: #64748b; margin-top: 12px; }

/* AI Details Panel */
.ai-details-panel { width: 320px; background: var(--bg-sidebar); border-left: 1px solid var(--border-color); padding: 32px 24px; display: flex; flex-direction: column; }
.panel-section { margin-bottom: 40px; }
.panel-section .section-title { font-size: 11px; color: var(--text-muted); font-weight: 700; margin-bottom: 16px; text-transform: uppercase; }

.quick-links { display: flex; flex-direction: column; gap: 8px; }
.q-link { background: transparent; border: 1px solid var(--border-color); border-radius: 8px; padding: 12px 16px; font-size: 13px; color: var(--text-secondary); display: flex; align-items: center; gap: 12px; cursor: pointer; }
.q-link:hover { border-color: #3b82f6; color: var(--text-primary); }
.q-link i { color: #3b82f6; font-size: 14px; }

.context-item { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; cursor: pointer; }
.c-icon { width: 36px; height: 36px; border-radius: 4px; display: flex; align-items: center; justify-content: center; background: var(--bg-secondary); font-size: 14px; }
.c-icon.blue { color: #3b82f6; }
.c-icon.purple { color: #a855f7; }
.c-text .c-name { font-size: 14px; font-weight: 500; color: var(--text-primary); }
.c-text .c-time { font-size: 12px; color: var(--text-secondary); }

.upgrade-card-wrapper { margin-top: auto; }
.upgrade-card { background: var(--active-bg); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; text-align: left; }
.plan-label { font-size: 11px; font-weight: 800; color: #579dff; margin-bottom: 8px; }
.plan-desc { font-size: 12px; color: var(--text-secondary); line-height: 1.4; margin-bottom: 16px; }
.btn-upgrade { width: 100%; background: #3b82f6; border: none; color: white; padding: 10px; border-radius: 6px; font-weight: 600; font-size: 13px; cursor: pointer; }
.btn-upgrade:hover { background: #2563eb; }

/* AI Popup Sidebar */
.ai-sidebar.popup { width: 420px; background-color: var(--bg-nav); border-left: 1px solid var(--border-color); position: fixed; top: 56px; right: 0; bottom: 0; z-index: 100; }
.ai-header { padding: 16px 20px; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; }
.ai-header h4 { font-size: 14px; margin: 0; color: var(--text-primary); display: flex; align-items: center; gap: 8px; }
.ai-header h4 i { color: #579dff; }
.ai-content { padding: 20px; }
.chat-message { display: flex; gap: 12px; }
.avatar-bot { width: 28px; height: 28px; border-radius: 50%; background-color: #3b82f6; color: white; display: flex; align-items: center; justify-content: center; }
.message-bubble { background-color: var(--bg-secondary); padding: 14px; border-radius: 4px 16px 16px 16px; color: var(--text-primary); font-size: 13px; line-height: 1.5; border: 1px solid var(--border-color); }

.slide-right-enter-active, .slide-right-leave-active { transition: transform 0.3s ease; }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }

.mobile-only {
  display: none;
}

@media (max-width: 1024px) {
  .ai-details-panel {
    display: none;
  }
}

@media (max-width: 768px) {
  .mobile-only {
    display: flex;
  }
  .desktop-only {
    display: none;
  }
  .sidebar {
    position: fixed;
    left: -260px;
    top: 56px;
    bottom: 0;
    z-index: 1001;
    transition: left 0.3s ease;
  }
  .sidebar.show {
    left: 0;
  }
  .nav-left, .nav-right {
    width: auto;
  }
  .ai-page-header {
    padding: 20px 16px 0;
  }
  .jira-tabs {
    overflow-x: auto;
    padding-bottom: 8px;
  }
  .jira-tab {
    white-space: nowrap;
  }
  .chat-history {
    padding: 20px;
  }
  .ai-chat-input-wrapper {
    padding: 16px;
  }
}

.menu-toggle {
  font-size: 20px;
  color: #94a3b8;
  cursor: pointer;
  margin-right: 12px;
}
</style>

<style>
.custom-sidebar-dropdown.el-popper {
  background: var(--bg-secondary) !important;
  border: 1px solid var(--border-color) !important;
  border-radius: 4px !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item {
  background-color: transparent !important;
  color: var(--text-primary) !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item:hover,
.custom-sidebar-dropdown .el-dropdown-menu__item:focus {
  background-color: var(--hover-bg) !important;
}
.custom-sidebar-dropdown .el-popper__arrow::before {
  background: var(--bg-secondary) !important;
  border: 1px solid var(--border-color) !important;
}
</style>
