<template>
  <div class="dashboard-layout">
    <!-- Navbar -->
    <NexusTopbar 
      :sidebarVisible="sidebarVisible" 
      @toggle-sidebar="toggleSidebar" 
      @toggle-ai="toggleAI"
      @toggle-create="toggleCreate"
    />

    <div class="main-body">
      <!-- Sidebar Overlay for Mobile -->
      <div 
        class="sidebar-overlay" 
        v-if="sidebarVisible && isMobile" 
        @click="sidebarVisible = false"
      ></div>

      <!-- Sidebar -->
      <NexusSidebar :isVisible="sidebarVisible" @close-mobile="sidebarVisible = false" @open-create-modal="toggleCreateSpace" />

      <!-- Main Content -->
      <main class="content-area">
        <div class="content-wrapper">
          <slot></slot>
        </div>
      </main>
    </div>

    <!-- AI Sidebar Popup -->
    <transition name="slide-right">
      <aside class="ai-sidebar" v-if="aiVisible">
        <div class="ai-header">
          <h4><i class="fa-solid fa-robot"></i> Trợ lý AI</h4>
          <button class="close-ai" @click="toggleAI">
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>
        
        <div class="ai-content">
          <div class="quick-actions">
            <el-button size="small" round plain>Tóm tắt công việc</el-button>
            <el-button size="small" round plain>Gợi ý ưu tiên</el-button>
          </div>
          <div class="chat-message bot">
            <div class="message-bubble">
              Xin chào! Mình có thể giúp gì cho bạn hôm nay?
            </div>
          </div>
        </div>

        <div class="ai-input-area">
          <div class="ai-input-wrapper">
            <input type="text" placeholder="Hỏi AI..." />
            <div class="ai-input-actions">
              <button class="send-btn"><i class="fa-solid fa-paper-plane"></i></button>
            </div>
          </div>
        </div>
      </aside>
    </transition>

    <!-- Space Creation Modal -->
    <CreateSpaceModal v-model:visible="createSpaceVisible" @created="handleSpaceCreated" />

    <!-- Project/Task Creation Modal -->
    <CreateProjectModal v-model:visible="createVisible" @created="handleProjectCreated" />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import CreateProjectModal from '../CreateProjectModal.vue'
import CreateSpaceModal from '../CreateSpaceModal.vue'
import NexusTopbar from './NexusTopbar.vue'
import NexusSidebar from './NexusSidebar.vue'

const sidebarVisible = ref(window.innerWidth > 1024)
const aiVisible = ref(false)
const createVisible = ref(false)
const createSpaceVisible = ref(false)
const isMobile = ref(window.innerWidth <= 1024)

const updateSize = () => {
  isMobile.value = window.innerWidth <= 1024
  if (!isMobile.value) {
    sidebarVisible.value = true
  }
}

onMounted(() => {
  window.addEventListener('resize', updateSize)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateSize)
})

const toggleSidebar = () => {
  sidebarVisible.value = !sidebarVisible.value
}

const toggleAI = () => {
  aiVisible.value = !aiVisible.value
}

const toggleCreate = () => {
  createSpaceVisible.value = !createSpaceVisible.value
}

const toggleCreateSpace = () => {
  createSpaceVisible.value = !createSpaceVisible.value
}

const handleSpaceCreated = (newSpace) => {
  if (newSpace && newSpace.id) {
    window.location.href = `/space/${newSpace.id}`;
  } else {
    window.location.reload();
  }
}

const handleProjectCreated = (newProject) => {
  console.log('Task created:', newProject)
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
  font-family: 'Inter', system-ui, sans-serif;
}

.main-body {
  display: flex;
  flex: 1;
  overflow: hidden;
  position: relative;
}

.sidebar-overlay {
  position: fixed;
  top: 64px;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 998;
  backdrop-filter: blur(2px);
}

.content-area {
  flex: 1;
  background: #0D0F11;
  padding: 0;
  overflow-y: hidden;
  transition: all 0.3s;
  display: flex;
  flex-direction: column;
}

.dark .content-area {
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.2);
}

.content-wrapper {
  width: 100%;
  height: 100%;
  margin: 0;
  display: flex;
  flex-direction: column;
}

@media (max-width: 1024px) {
  .content-area {
    padding: 0;
  }
}

/* AI Sidebar Styles */
.ai-sidebar {
  position: fixed;
  right: 0;
  top: 0;
  bottom: 0;
  width: 360px;
  background: var(--bg-card);
  box-shadow: -4px 0 24px rgba(0,0,0,0.1);
  z-index: 2000;
  display: flex;
  flex-direction: column;
}

@media (max-width: 576px) {
  .ai-sidebar {
    width: 100%;
  }
}

.dark .ai-sidebar { box-shadow: -4px 0 24px rgba(0,0,0,0.5); }

.ai-header { padding: 20px; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; }
.ai-header h4 { margin: 0; display: flex; align-items: center; gap: 8px; color: #3b82f6; font-size: 18px; }
.close-ai { background: transparent; border: none; font-size: 18px; color: var(--text-muted); cursor: pointer; }

.ai-content { flex: 1; padding: 20px; overflow-y: auto; }
.quick-actions { display: flex; gap: 8px; margin-bottom: 24px; }
.message-bubble { background: var(--bg-layout); padding: 12px 16px; border-radius: 16px; border-top-left-radius: 4px; font-size: 14px; line-height: 1.5; }

.ai-input-area { padding: 20px; border-top: 1px solid var(--border-color); }
.ai-input-wrapper { display: flex; background: var(--bg-layout); border-radius: 20px; padding: 8px 16px; align-items: center; }
.ai-input-wrapper input { flex: 1; background: transparent; border: none; outline: none; color: var(--text-primary); }
.send-btn { background: #3b82f6; color: white; border: none; width: 32px; height: 32px; border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; }

.slide-right-enter-active, .slide-right-leave-active { transition: transform 0.3s ease; }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }
</style>
