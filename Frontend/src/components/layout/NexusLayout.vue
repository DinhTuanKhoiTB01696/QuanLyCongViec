<template>
  <div class="dashboard-layout">
    <AppTopBar
      :sidebarVisible="sidebarVisible"
      @toggle-sidebar="toggleSidebar"
      @toggle-ai="toggleAI"
      @toggle-create="toggleCreate"
    />

    <div class="main-body">
      <div
        v-if="sidebarVisible && isMobile"
        class="sidebar-overlay"
        @click="sidebarVisible = false"
      ></div>

      <NexusSidebar v-if="!hideSidebar" :isVisible="sidebarVisible" @close-mobile="sidebarVisible = false" />

      <main class="content-area">
        <div class="content-wrapper">
          <slot></slot>
        </div>
      </main>
    </div>

    <transition name="slide-right">
      <aside v-if="aiVisible" class="ai-sidebar">
        <div class="ai-header border-b border-border p-5 flex justify-between items-center">
          <h4 class="m-0 flex items-center gap-2 text-accent text-lg font-bold">
            <Bot class="w-5 h-5" /> AI Assistant
          </h4>
          <button class="close-ai bg-transparent border-none text-muted cursor-pointer hover:text-primary transition-colors" @click="toggleAI">
            <X class="w-5 h-5" />
          </button>
        </div>

        <div ref="aiContentRef" class="ai-content">
          <div class="quick-actions flex flex-wrap gap-2 mb-6">
            <SprintaButton size="small" variant="subtle" class="rounded-full border border-border" @click="useQuickPrompt('Tóm tắt công việc đang mở')">
              Tóm tắt công việc
            </SprintaButton>
            <SprintaButton size="small" variant="subtle" class="rounded-full border border-border" @click="useQuickPrompt('Gợi ý ưu tiên việc cần làm tiếp theo')">
              Gợi ý ưu tiên
            </SprintaButton>
          </div>

          <div
            v-for="(message, index) in chatHistory"
            :key="`${message.role}-${index}`"
            class="chat-message"
            :class="message.role"
          >
            <div class="message-bubble">
              {{ message.content }}
            </div>
          </div>
        </div>

        <div class="ai-input-area p-5 border-t border-border">
          <div class="ai-input-wrapper flex items-center bg-bg rounded-full py-2 px-4 gap-2.5">
            <input
              v-model="aiInput"
              type="text"
              class="flex-1 bg-transparent border-none outline-none text-primary"
              placeholder="Hỏi AI..."
              @keyup.enter="sendAiMessage"
            />
            <div class="ai-input-actions">
              <button class="send-btn bg-accent text-white border-none w-8 h-8 rounded-full cursor-pointer flex items-center justify-center disabled:opacity-60 disabled:cursor-not-allowed hover:bg-accent-hover transition-colors" :disabled="aiSending || !aiInput.trim()" @click="sendAiMessage">
                <Send v-if="!aiSending" class="w-4 h-4 ml-[-2px]" />
                <Loader2 v-else class="w-4 h-4 animate-spin" />
              </button>
            </div>
          </div>
        </div>
      </aside>
    </transition>

    <CreateSpaceModal v-model:visible="createSpaceVisible" @created="handleSpaceCreated" />
    <CreateProjectModal v-model:visible="createVisible" @created="handleProjectCreated" />
  </div>
</template>

<script setup>
import { nextTick, onMounted, onUnmounted, ref, defineProps } from 'vue'
import { ElMessage } from 'element-plus'
import { Bot, X, Send, Loader2 } from 'lucide-vue-next'
import axiosClient from '@/api/axiosClient'
import CreateProjectModal from '../CreateProjectModal.vue'
import CreateSpaceModal from '../CreateSpaceModal.vue'
import AppTopBar from './AppTopBar.vue'
import NexusSidebar from './NexusSidebar.vue'
import SprintaButton from '@/components/ui/SprintaButton.vue'

const props = defineProps({
  hideSidebar: {
    type: Boolean,
    default: false
  }
})

const sidebarVisible = ref(window.innerWidth > 1024)
const aiVisible = ref(false)
const createVisible = ref(false)
const createSpaceVisible = ref(false)
const isMobile = ref(window.innerWidth <= 1024)
const aiInput = ref('')
const aiSending = ref(false)
const aiContentRef = ref(null)
const chatHistory = ref([
  {
    role: 'bot',
    content: 'Xin chao! Minh co the giup ban tom tat, tra loi cau hoi, va goi y viec tiep theo.'
  }
])

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

const scrollAiToBottom = async () => {
  await nextTick()
  if (aiContentRef.value) {
    aiContentRef.value.scrollTop = aiContentRef.value.scrollHeight
  }
}

const toggleAI = async () => {
  aiVisible.value = !aiVisible.value
  if (aiVisible.value) {
    await scrollAiToBottom()
  }
}

const toggleCreate = () => {
  createVisible.value = !createVisible.value
}

const useQuickPrompt = (prompt) => {
  aiInput.value = prompt
}

const sendAiMessage = async () => {
  const outgoing = aiInput.value.trim()
  if (!outgoing || aiSending.value) return

  aiSending.value = true
  aiInput.value = ''
  chatHistory.value.push({ role: 'user', content: outgoing })
  chatHistory.value.push({ role: 'bot', content: 'Dang suy nghi...' })
  await scrollAiToBottom()

  try {
    const history = chatHistory.value
      .filter(item => item.content !== 'Dang suy nghi...')
      .map(item => ({
        role: item.role === 'bot' ? 'assistant' : 'user',
        content: item.content
      }))

    const response = await axiosClient.post('/ai/chat', {
      message: outgoing,
      history
    })

    chatHistory.value.pop()
    chatHistory.value.push({
      role: 'bot',
      content: response.data?.data || response.data?.message || 'AI khong tra ve noi dung.'
    })
  } catch (error) {
    chatHistory.value.pop()
    const message = error.response?.data?.message || 'Khong gui duoc tin nhan toi AI.'
    chatHistory.value.push({ role: 'bot', content: message })
    ElMessage.error(message)
  } finally {
    aiSending.value = false
    await scrollAiToBottom()
  }
}

const handleSpaceCreated = (newSpace) => {
  if (newSpace && newSpace.id) {
    window.location.href = `/space/${newSpace.id}`
  } else {
    window.location.reload()
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
  background-color: var(--color-bg);
  color: var(--color-text-primary);
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
  background: var(--color-bg);
  padding: 0;
  overflow-y: auto;
  transition: all 0.3s;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.dark .content-area {
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.2);
}

.content-wrapper {
  width: 100%;
  min-height: 100%;
  margin: 0;
  display: flex;
  flex-direction: column;
}

@media (max-width: 1024px) {
  .content-area {
    padding: 0;
  }
}

.ai-sidebar {
  position: fixed;
  right: 0;
  top: 0;
  bottom: 0;
  width: 360px;
  background: var(--color-surface);
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.1);
  z-index: 2000;
  display: flex;
  flex-direction: column;
}

@media (max-width: 576px) {
  .ai-sidebar {
    width: 100%;
  }
}

.dark .ai-sidebar {
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.5);
}

.ai-header {
  padding: 20px;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chat-message {
  display: flex;
  margin-bottom: 12px;
}

.chat-message.user {
  justify-content: flex-end;
}

.message-bubble {
  max-width: 88%;
  background: var(--color-bg);
  padding: 12px 16px;
  border-radius: 16px;
  border-top-left-radius: 4px;
  font-size: 14px;
  line-height: 1.5;
  white-space: pre-wrap;
}

.chat-message.user .message-bubble {
  background: color-mix(in srgb, var(--color-accent) 16%, transparent);
  border-top-left-radius: 16px;
  border-top-right-radius: 4px;
}

.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 0.3s ease;
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
}
</style>
