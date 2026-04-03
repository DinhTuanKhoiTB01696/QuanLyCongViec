<template>
  <NexusLayout>
    <div class="ai-page-flex-wrapper" style="display: flex; height: calc(100vh - 56px); width: calc(100% + 80px); margin: -40px;">
      <div class="ai-container" style="flex: 1; overflow-y: auto;">
        <div class="ai-page-header">
            <div class="header-left">
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
      </div> <!-- Closes ai-container -->

      <!-- Right Sidebar: AI Details -->
      <aside class="ai-details-panel" style="overflow-y: auto;">
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
    </div> <!-- Closes ai-page-flex-wrapper -->
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
  </NexusLayout>
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
import NexusLayout from '@/components/layout/NexusLayout.vue'

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
.ai-container { width: 100%; display: flex; flex-direction: column; height: 100%; }

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
