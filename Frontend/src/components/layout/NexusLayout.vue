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

    <button
      v-if="!aiVisible"
      class="ai-floating-btn"
      type="button"
      :title="aiCopy.floatingTitle"
      @click="toggleAI"
    >
      <i class="fa-solid fa-robot"></i>
      <span>AI</span>
    </button>

    <transition name="ai-backdrop-fade">
      <div v-if="aiVisible && isMobile" class="ai-mobile-backdrop" @click="toggleAI"></div>
    </transition>

    <transition name="slide-right">
      <aside v-if="aiVisible" class="ai-sidebar" aria-label="AI Assistant">
        <div class="ai-hero">
          <div class="ai-hero-top">
            <div class="ai-brand">
              <span class="ai-brand-icon"><i class="fa-solid fa-robot"></i></span>
              <div>
                <p>{{ aiCopy.brand }}</p>
                <h4>{{ aiCopy.title }}</h4>
              </div>
            </div>
            <button class="close-ai" type="button" :title="aiCopy.closeTitle" @click="toggleAI">
              <i class="fa-solid fa-xmark"></i>
            </button>
          </div>
          <p class="ai-hero-copy">{{ aiCopy.hero }}</p>
        </div>

        <div ref="aiContentRef" class="ai-content">
          <div class="quick-actions">
            <button
              v-for="prompt in quickPrompts"
              :key="prompt.text"
              class="quick-action"
              type="button"
              @click="useQuickPrompt(prompt.text)"
            >
              <i :class="prompt.icon"></i>
              <span>{{ prompt.label }}</span>
            </button>
          </div>

          <div class="ai-context-card">
            <div>
              <strong>{{ aiCopy.contextTitle }}</strong>
              <span>{{ currentRouteLabel }}</span>
            </div>
            <button type="button" @click="useQuickPrompt(`${aiCopy.currentPagePrompt}: ${currentRouteLabel}`)">
              <i class="fa-solid fa-wand-magic-sparkles"></i>
            </button>
          </div>

          <div class="chat-thread">
            <div
              v-for="(message, index) in chatHistory"
              :key="`${message.role}-${index}`"
              class="chat-message"
              :class="message.role"
            >
              <div class="message-avatar">
                <i v-if="message.role === 'bot'" class="fa-solid fa-robot"></i>
                <i v-else class="fa-regular fa-user"></i>
              </div>
              <div class="message-stack">
                <span class="message-author">{{ message.role === 'bot' ? aiCopy.botName : aiCopy.you }}</span>
                <div class="message-bubble">
                  <i v-if="message.loading" class="fa-solid fa-spinner fa-spin"></i>
                  <span>{{ message.content }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="ai-input-area">
          <div class="ai-input-wrapper">
            <textarea
              v-model="aiInput"
              rows="1"
              :placeholder="aiCopy.placeholder"
              @keydown.enter.exact.prevent="sendAiMessage"
            ></textarea>
            <button class="send-btn" type="button" :disabled="aiSending || !aiInput.trim()" @click="sendAiMessage">
              <i v-if="!aiSending" class="fa-solid fa-paper-plane"></i>
              <i v-else class="fa-solid fa-spinner fa-spin"></i>
            </button>
          </div>
          <div class="ai-input-foot">
            <span>{{ aiCopy.enterHint }}</span>
            <button type="button" @click="chatHistory = defaultChatHistory()">{{ aiCopy.reset }}</button>
          </div>
        </div>
      </aside>
    </transition>

    <CreateSpaceModal v-model:visible="createSpaceVisible" @created="handleSpaceCreated" />
    <CreateProjectModal v-model:visible="createVisible" @created="handleProjectCreated" />
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref, defineProps } from 'vue'
import { ElMessage } from 'element-plus'
import { useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import CreateProjectModal from '../CreateProjectModal.vue'
import CreateSpaceModal from '../CreateSpaceModal.vue'
import AppTopBar from './AppTopBar.vue'
import NexusSidebar from './NexusSidebar.vue'
import { useI18nStore } from '@/store/useI18nStore'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { useProjectStore } from '@/store/useProjectStore'

const props = defineProps({
  hideSidebar: {
    type: Boolean,
    default: false
  }
})

const route = useRoute()
const i18nStore = useI18nStore()
const workTaskStore = useWorkTaskStore()
const projectStore = useProjectStore()
const sidebarVisible = ref(window.innerWidth > 1024)
const aiVisible = ref(false)
const createVisible = ref(false)
const createSpaceVisible = ref(false)
const isMobile = ref(window.innerWidth <= 1024)
const aiInput = ref('')
const aiSending = ref(false)
const aiContentRef = ref(null)
const aiCopyMap = {
  vi: {
    floatingTitle: 'Mở AI Assistant',
    closeTitle: 'Đóng AI',
    brand: 'SPRINTA AI',
    title: 'Trợ lý công việc',
    hero: 'Hỏi nhanh, tóm tắt tiến độ, tạo checklist hoặc xin gợi ý ưu tiên ở bất kỳ trang nào.',
    contextTitle: 'Ngữ cảnh hiện tại',
    currentPagePrompt: 'Tom tat trang hien tai',
    botName: 'SprintA AI',
    you: 'Bạn',
    placeholder: 'Hỏi AI về task, dashboard, deadline...',
    enterHint: 'Enter để gửi',
    reset: 'Làm mới',
    thinking: 'Đang đọc ngữ cảnh và suy nghĩ...',
    emptyResponse: 'AI không trả về nội dung.',
    sendFailed: 'Không gửi được tin nhắn tới AI.',
    welcome: 'Xin chào Khôi. Mình sẵn sàng tóm tắt, gợi ý ưu tiên, tạo checklist hoặc phân tích nội dung trên trang hiện tại.',
    prompts: [
      { label: 'Tóm tắt trang', icon: 'fa-regular fa-file-lines', text: 'Tom tat trang hien tai va neu 3 diem can chu y.' },
      { label: 'Gợi ý ưu tiên', icon: 'fa-solid fa-arrow-up-wide-short', text: 'Goi y viec nen lam tiep theo dua tren ngu canh hien tai.' },
      { label: 'Tạo checklist', icon: 'fa-solid fa-list-check', text: 'Tao checklist ngan gon de hoan thanh cong viec nay.' },
      { label: 'Viết cập nhật', icon: 'fa-solid fa-pen-nib', text: 'Soan ban cap nhat tien do ngan gon cho team.' }
    ]
  },
  en: {
    floatingTitle: 'Open AI Assistant',
    closeTitle: 'Close AI',
    brand: 'SPRINTA AI',
    title: 'Work assistant',
    hero: 'Ask quickly, summarize progress, create checklists, or get priority suggestions from any page.',
    contextTitle: 'Current context',
    currentPagePrompt: 'Summarize the current page',
    botName: 'SprintA AI',
    you: 'You',
    placeholder: 'Ask AI about tasks, dashboards, deadlines...',
    enterHint: 'Enter to send',
    reset: 'Reset',
    thinking: 'Reading context and thinking...',
    emptyResponse: 'AI did not return any content.',
    sendFailed: 'Could not send the message to AI.',
    welcome: 'Hi Khoi. I can summarize, suggest priorities, create checklists, or analyze the current page.',
    prompts: [
      { label: 'Summarize page', icon: 'fa-regular fa-file-lines', text: 'Summarize the current page and list 3 key points.' },
      { label: 'Suggest priority', icon: 'fa-solid fa-arrow-up-wide-short', text: 'Suggest what I should do next based on the current context.' },
      { label: 'Create checklist', icon: 'fa-solid fa-list-check', text: 'Create a short checklist to finish this work.' },
      { label: 'Write update', icon: 'fa-solid fa-pen-nib', text: 'Draft a concise progress update for the team.' }
    ]
  }
}

const aiCopyOverrideMap = {
  vi: {
    floatingTitle: 'Mở AI Assistant',
    closeTitle: 'Đóng AI',
    brand: 'SPRINTA AI',
    title: 'Trợ lý công việc',
    hero: 'Hỏi nhanh, tạo task thật, chuyển trạng thái, tóm tắt tiến độ hoặc xem thống kê ở bất kỳ trang nào.',
    contextTitle: 'Ngữ cảnh hiện tại',
    currentPagePrompt: 'Tóm tắt trang hiện tại',
    botName: 'SprintA AI',
    you: 'Bạn',
    placeholder: 'Ví dụ: tạo task sửa UI deadline mai, thống kê project, tóm tắt trang...',
    enterHint: 'Enter để gửi',
    reset: 'Làm mới',
    thinking: 'Đang đọc dữ liệu thật và xử lý...',
    emptyResponse: 'AI không trả về nội dung.',
    sendFailed: 'Không gửi được tin nhắn tới AI.',
    needProject: 'Bạn cần mở một project trước khi yêu cầu AI tạo hoặc cập nhật task.',
    welcome: 'Xin chào Khôi. Mình có thể tạo task thật, chuyển trạng thái task, thống kê project, tóm tắt trang và gợi ý ưu tiên từ dữ liệu hiện tại.',
    prompts: [
      { label: 'Tạo task', icon: 'fa-solid fa-square-plus', text: 'Tạo task mới: Hoàn thiện phần demo hôm nay, deadline ngày mai, ưu tiên cao.' },
      { label: 'Thống kê project', icon: 'fa-solid fa-chart-simple', text: 'Thống kê project hiện tại.' },
      { label: 'Tóm tắt trang', icon: 'fa-regular fa-file-lines', text: 'Tóm tắt trang hiện tại và nêu 3 điểm cần chú ý.' },
      { label: 'Gợi ý ưu tiên', icon: 'fa-solid fa-arrow-up-wide-short', text: 'Gợi ý 5 việc nên làm tiếp theo dựa trên task hiện tại.' }
    ]
  },
  en: {
    floatingTitle: 'Open AI Assistant',
    closeTitle: 'Close AI',
    brand: 'SPRINTA AI',
    title: 'Work assistant',
    hero: 'Ask quickly, create real tasks, move status, summarize progress, or get project statistics from any page.',
    contextTitle: 'Current context',
    currentPagePrompt: 'Summarize the current page',
    botName: 'SprintA AI',
    you: 'You',
    placeholder: 'Try: create task fix UI due tomorrow, project stats, summarize page...',
    enterHint: 'Enter to send',
    reset: 'Reset',
    thinking: 'Reading real data and processing...',
    emptyResponse: 'AI did not return any content.',
    sendFailed: 'Could not send the message to AI.',
    needProject: 'Open a project before asking AI to create or update tasks.',
    welcome: 'Hi Khoi. I can create real tasks, move task status, summarize the page, report project stats, and suggest priorities from the current data.',
    prompts: [
      { label: 'Create task', icon: 'fa-solid fa-square-plus', text: 'Create a new task: Finish today demo, due tomorrow, high priority.' },
      { label: 'Project stats', icon: 'fa-solid fa-chart-simple', text: 'Show stats for the current project.' },
      { label: 'Summarize page', icon: 'fa-regular fa-file-lines', text: 'Summarize the current page and list 3 key points.' },
      { label: 'Suggest priority', icon: 'fa-solid fa-arrow-up-wide-short', text: 'Suggest 5 next actions based on current tasks.' }
    ]
  }
}

const aiCopy = computed(() => aiCopyOverrideMap[i18nStore.locale] || aiCopyOverrideMap.vi)
const quickPrompts = computed(() => aiCopy.value.prompts)

const defaultChatHistory = () => [
  {
    role: 'bot',
    content: aiCopy.value.welcome
  }
]

const chatHistory = ref(defaultChatHistory())

const currentRouteLabel = computed(() => {
  const name = route.meta?.title || route.name || route.path
  return typeof name === 'string' ? name : route.path
})

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

const normalizeAiText = (value = '') =>
  `${value}`
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D')
    .toLowerCase()

const currentProjectId = computed(() => {
  const routeId = route.params?.id
  if (typeof routeId === 'string' && routeId.length >= 30) return routeId
  return projectStore.currentProject?.id || projectStore.currentProject?.Id || workTaskStore.currentProjectId || null
})

const currentTasks = computed(() => Array.isArray(workTaskStore.tasks) ? workTaskStore.tasks : [])

const ensureProjectTasks = async () => {
  const projectId = currentProjectId.value
  if (!projectId) return []
  if (workTaskStore.currentProjectId !== projectId || !currentTasks.value.length) {
    await workTaskStore.fetchTasks(projectId)
  }
  return currentTasks.value
}

const todayDateOnly = () => new Date().toISOString().slice(0, 10)

const offsetDateOnly = (days) => {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return date.toISOString().slice(0, 10)
}

const inferDueDate = (normalized) => {
  if (normalized.includes('hom nay') || normalized.includes('today')) return todayDateOnly()
  if (normalized.includes('ngay mai') || normalized.includes('tomorrow')) return offsetDateOnly(1)
  if (normalized.includes('tuan sau') || normalized.includes('next week')) return offsetDateOnly(7)
  const match = normalized.match(/(\d{1,2})[/-](\d{1,2})(?:[/-](\d{2,4}))?/)
  if (!match) return null
  const currentYear = new Date().getFullYear()
  const day = Number(match[1])
  const month = Number(match[2])
  const year = match[3] ? Number(match[3].length === 2 ? `20${match[3]}` : match[3]) : currentYear
  if (!day || !month) return null
  return `${year}-${`${month}`.padStart(2, '0')}-${`${day}`.padStart(2, '0')}`
}

const inferPriority = (normalized) => {
  if (/(khan|urgent|rat cao|critical|nghiem trong|blocker)/.test(normalized)) return 1
  if (/(cao|high|important)/.test(normalized)) return 2
  if (/(thap|low)/.test(normalized)) return 4
  return 3
}

const inferStatusName = (normalized) => {
  if (/(done|hoan thanh|da xong|xong)/.test(normalized)) return 'DONE'
  if (/(review|kiem tra|danh gia)/.test(normalized)) return 'IN REVIEW'
  if (/(progress|dang lam|dang thuc hien|in progress)/.test(normalized)) return 'IN PROGRESS'
  if (/(todo|to do|can lam)/.test(normalized)) return 'TO DO'
  if (/(backlog|cho xu ly)/.test(normalized)) return 'BACKLOG'
  return 'TO DO'
}

const cleanTaskTitle = (message, normalized) => {
  const raw = `${message}`.trim()
  const quoted = raw.match(/["“”']([^"“”']{2,})["“”']/)
  if (quoted?.[1]) return quoted[1].trim()

  const lastBot = [...chatHistory.value].reverse().find(item => item.role === 'bot' && !item.loading)
  const suggested = lastBot?.content?.match(/(?:Tên Task|Task|title)[:：\s*"']+([^*\n"]{2,80})/i)
  if (/(ok tao|tao di|create it|add it|lam di)/.test(normalized) && suggested?.[1]) {
    return suggested[1].replace(/\*\*/g, '').trim()
  }

  const markers = ['tạo task', 'tao task', 'tạo công việc', 'tao cong viec', 'create task', 'add task', 'task mới', 'task moi']
  const lower = raw.toLowerCase()
  let title = raw
  for (const marker of markers) {
    const index = lower.indexOf(marker)
    if (index >= 0) {
      title = raw.slice(index + marker.length)
      break
    }
  }

  title = title
    .replace(/^\s*[:\-–]\s*/, '')
    .replace(/^(mới|moi|new)\s*[:\-–]\s*/i, '')
    .replace(/\b(deadline|due|hạn|han|ưu tiên|uu tien|priority)\b.*$/i, '')
    .replace(/\s+/g, ' ')
    .trim()

  if (!title || /^(moi|mới|new)$/i.test(title)) {
    if (suggested?.[1]) return suggested[1].replace(/\*\*/g, '').trim()
  }

  if (!title && normalized.includes('ok tao')) return 'Task mới từ SprintA AI'
  return title || 'Task mới từ SprintA AI'
}

const splitTaskTitles = (message) => {
  const lines = `${message}`
    .split(/\n|;|\d+\.\s+/)
    .map(item => item.replace(/^[-*]\s*/, '').trim())
    .filter(Boolean)
  const taskLines = lines.filter(item => /^(tao|tạo|create|add|task)/i.test(item) || lines.length > 1)
  return taskLines.length > 1 ? taskLines.map(item => cleanTaskTitle(item, normalizeAiText(item))).filter(Boolean) : []
}

const formatTaskLine = (task) => {
  const status = task.statusName || 'BACKLOG'
  const due = task.dueDate || task.plannedEndDate
  return `- ${task.sequenceId || task.id?.slice?.(0, 8) || 'Task'}: ${task.title} (${status}${due ? `, hạn ${due}` : ''})`
}

const buildProjectStats = async () => {
  const tasks = await ensureProjectTasks()
  const isDone = (task) => normalizeAiText(task.statusName).includes('done') || normalizeAiText(task.statusName).includes('hoan thanh')
  const isProgress = (task) => normalizeAiText(task.statusName).includes('progress') || normalizeAiText(task.statusName).includes('dang')
  const isTodo = (task) => normalizeAiText(task.statusName).includes('todo') || normalizeAiText(task.statusName).includes('to do') || normalizeAiText(task.statusName).includes('can lam')
  const today = todayDateOnly()
  const overdue = tasks.filter(task => !isDone(task) && (task.dueDate || task.plannedEndDate) && (task.dueDate || task.plannedEndDate) < today)
  return {
    total: tasks.length,
    done: tasks.filter(isDone).length,
    inProgress: tasks.filter(isProgress).length,
    todo: tasks.filter(isTodo).length,
    backlog: tasks.filter(task => normalizeAiText(task.statusName).includes('backlog') || !task.statusName).length,
    overdue: overdue.length,
    highPriority: tasks.filter(task => Number(task.priority) > 0 && Number(task.priority) <= 2).length
  }
}

const summarizeCurrentProject = async () => {
  const tasks = await ensureProjectTasks()
  const stats = await buildProjectStats()
  const topTasks = tasks
    .filter(task => !/(done|hoan thanh)/.test(normalizeAiText(task.statusName)))
    .sort((a, b) => Number(a.priority || 9) - Number(b.priority || 9))
    .slice(0, 5)

  return [
    `Tóm tắt project hiện tại: có ${stats.total} task, ${stats.done} đã xong, ${stats.inProgress} đang làm, ${stats.todo} cần làm, ${stats.overdue} quá hạn.`,
    stats.highPriority ? `Có ${stats.highPriority} task ưu tiên cao cần theo dõi.` : 'Hiện chưa có task ưu tiên cao.',
    topTasks.length ? `Việc nên chú ý:\n${topTasks.map(formatTaskLine).join('\n')}` : 'Chưa có task mở nào cần xử lý.'
  ].join('\n\n')
}

const suggestNextActions = async () => {
  const tasks = await ensureProjectTasks()
  const openTasks = tasks
    .filter(task => !/(done|hoan thanh)/.test(normalizeAiText(task.statusName)))
    .sort((a, b) => {
      const priorityDiff = Number(a.priority || 9) - Number(b.priority || 9)
      if (priorityDiff !== 0) return priorityDiff
      return `${a.dueDate || a.plannedEndDate || '9999-12-31'}`.localeCompare(`${b.dueDate || b.plannedEndDate || '9999-12-31'}`)
    })
    .slice(0, 5)

  if (!openTasks.length) return 'Project hiện tại chưa có task mở. Bạn có thể yêu cầu: "tạo task chuẩn bị demo ngày mai".'
  return `Gợi ý ưu tiên tiếp theo:\n${openTasks.map((task, index) => `${index + 1}. ${formatTaskLine(task).slice(2)}`).join('\n')}`
}

const createRealTasks = async (message) => {
  const projectId = currentProjectId.value
  if (!projectId) throw new Error(aiCopy.value.needProject)
  const normalized = normalizeAiText(message)
  const titles = splitTaskTitles(message)
  const finalTitles = titles.length ? titles : [cleanTaskTitle(message, normalized)]
  const dueDate = inferDueDate(normalized)
  const statusName = inferStatusName(normalized)
  const priority = inferPriority(normalized)
  const created = []

  for (const title of finalTitles.slice(0, 8)) {
    const payload = {
      title,
      description: `Được tạo bởi SprintA AI từ yêu cầu:\n${message}`,
      statusName,
      typeName: 'Task',
      priority,
      storyPoints: 0
    }
    if (dueDate) payload.dueDate = dueDate
    created.push(await workTaskStore.createTask(projectId, payload))
  }

  window.dispatchEvent(new CustomEvent('sprinta-ai-task-created', { detail: { projectId, tasks: created } }))
  return created.length === 1
    ? `Đã tạo task thật: "${created[0]?.title || finalTitles[0]}" (${statusName}${dueDate ? `, hạn ${dueDate}` : ''}).`
    : `Đã tạo ${created.length} task thật:\n${created.map(task => `- ${task?.title}`).join('\n')}`
}

const moveTaskByPrompt = async (message) => {
  const projectId = currentProjectId.value
  if (!projectId) throw new Error(aiCopy.value.needProject)
  const tasks = await ensureProjectTasks()
  const normalized = normalizeAiText(message)
  const statusName = inferStatusName(normalized)
  const sequenceMatch = message.match(/\b[A-Z0-9]+-\d+\b/i)
  const quoted = message.match(/["“”']([^"“”']{2,})["“”']/)
  const keyword = normalizeAiText(quoted?.[1] || sequenceMatch?.[0] || message.replace(/(chuyen|chuyển|move|dua|đưa|sang|vao|vào|to do|todo|done|in progress|dang lam|hoan thanh|xong)/gi, ''))
  const task = tasks.find(item =>
    (sequenceMatch && normalizeAiText(item.sequenceId) === normalizeAiText(sequenceMatch[0])) ||
    (keyword && normalizeAiText(item.title).includes(keyword.trim()))
  )

  if (!task) {
    return 'Mình chưa tìm thấy task cần chuyển. Hãy ghi rõ mã task hoặc đặt tên task trong dấu ngoặc kép, ví dụ: chuyển "Bug Bash" sang Done.'
  }

  await workTaskStore.updateTaskStatus(projectId, task.id, statusName)
  return `Đã chuyển task "${task.title}" sang trạng thái ${statusName}.`
}

const tryHandleLocalAiCommand = async (message) => {
  const normalized = normalizeAiText(message)
  const wantsCreate = /(tao|create|add).*(task|cong viec)|ok tao|tao di|create it/.test(normalized)
  const wantsMove = /(chuyen|move|dua).*(task|cong viec|sang|vao|done|todo|progress|review)|sang (to do|todo|done|in progress)/.test(normalized)
  const wantsStats = /(thong ke|bao cao|report|stats|dashboard|tong quan)/.test(normalized)
  const wantsSummary = /(tom tat|summary|summarize|tong ket)/.test(normalized)
  const wantsPriority = /(uu tien|priority|nen lam|next action|goi y)/.test(normalized)
  const wantsChecklist = /(checklist|danh sach viec|cac buoc)/.test(normalized)

  if (wantsCreate) return await createRealTasks(message)
  if (wantsMove) return await moveTaskByPrompt(message)
  if (wantsStats) {
    const stats = await buildProjectStats()
    return `Thống kê project:\n- Tổng task: ${stats.total}\n- Đã xong: ${stats.done}\n- Đang làm: ${stats.inProgress}\n- Cần làm: ${stats.todo}\n- Backlog: ${stats.backlog}\n- Quá hạn: ${stats.overdue}\n- Ưu tiên cao: ${stats.highPriority}`
  }
  if (wantsSummary) return await summarizeCurrentProject()
  if (wantsPriority) return await suggestNextActions()
  if (wantsChecklist) {
    const suggestion = await suggestNextActions()
    return `Checklist đề xuất:\n1. Kiểm tra các task đang quá hạn hoặc ưu tiên cao.\n2. Chốt task cần làm tiếp theo trong cột To Do.\n3. Chuyển task đang xử lý sang In Progress.\n4. Cập nhật deadline/mô tả nếu còn thiếu.\n5. Báo cáo tiến độ ngắn cho team.\n\n${suggestion}`
  }

  return null
}

const sendAiMessage = async () => {
  const outgoing = aiInput.value.trim()
  if (!outgoing || aiSending.value) return

  aiSending.value = true
  aiInput.value = ''
  chatHistory.value.push({ role: 'user', content: outgoing })
  chatHistory.value.push({ role: 'bot', content: aiCopy.value.thinking, loading: true })
  await scrollAiToBottom()

  try {
    const localResult = await tryHandleLocalAiCommand(outgoing)
    if (localResult) {
      chatHistory.value.pop()
      chatHistory.value.push({ role: 'bot', content: localResult })
      ElMessage.success('SprintA AI đã thao tác dữ liệu thật.')
      return
    }

    const history = chatHistory.value
      .filter(item => !item.loading)
      .map(item => ({
        role: item.role === 'bot' ? 'assistant' : 'user',
        content: item.content
      }))

    const response = await axiosClient.post('/ai/command', {
      prompt: outgoing,
      projectId: currentProjectId.value,
      locale: i18nStore.locale || 'vi',
      history
    })

    chatHistory.value.pop()
    const responseData = response.data?.data
    const message = responseData?.message || responseData || response.data?.message || aiCopy.value.emptyResponse
    if (responseData?.action === 'create-task' && currentProjectId.value) {
      await workTaskStore.fetchTasks(currentProjectId.value)
      window.dispatchEvent(new CustomEvent('sprinta-ai-task-created', {
        detail: { projectId: currentProjectId.value, task: responseData.createdTask }
      }))
    }
    chatHistory.value.push({
      role: 'bot',
      content: message
    })
  } catch (error) {
    chatHistory.value.pop()
    const message = error.response?.data?.message || aiCopy.value.sendFailed
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
  background:
    radial-gradient(circle at top left, color-mix(in srgb, var(--sa-primary) 8%, transparent), transparent 34%),
    var(--sa-bg);
  color: var(--color-text-primary);
  overflow: hidden;
  font-family: 'Be Vietnam Pro', 'Inter', system-ui, sans-serif;
}

.main-body {
  display: flex;
  flex: 1;
  overflow: hidden;
  position: relative;
  background: var(--sa-bg);
}

.sidebar-overlay {
  position: fixed;
  top: var(--sa-topbar-height, 52px);
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 998;
  backdrop-filter: blur(2px);
}

.content-area {
  flex: 1;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--sa-bg) 82%, var(--sa-surface) 18%), var(--sa-bg));
  padding: 0;
  overflow-y: auto;
  transition: all 0.3s;
  display: flex;
  flex-direction: column;
  min-height: 0;
  border-left: 1px solid color-mix(in srgb, var(--sa-border) 62%, transparent);
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
  background: transparent;
}

@media (max-width: 1024px) {
  .content-area {
    padding: 0;
  }
}

.ai-floating-btn {
  position: fixed;
  right: 22px;
  bottom: 22px;
  z-index: 1800;
  height: 44px;
  min-width: 82px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid color-mix(in srgb, var(--sa-primary) 42%, #ffffff);
  border-radius: 999px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--sa-primary) 94%, #2563eb 6%), #2563eb);
  color: #ffffff;
  font-weight: 800;
  box-shadow: 0 18px 44px rgba(14, 165, 233, 0.32);
  cursor: pointer;
}

.ai-floating-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 22px 54px rgba(14, 165, 233, 0.40);
}

.ai-mobile-backdrop {
  position: fixed;
  inset: var(--sa-topbar-height, 52px) 0 0;
  z-index: 1900;
  background: rgba(2, 6, 23, 0.48);
  backdrop-filter: blur(3px);
}

.ai-sidebar {
  position: fixed;
  right: 16px;
  top: 68px;
  bottom: 16px;
  width: min(520px, calc(100vw - 32px));
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 94%, var(--sa-primary) 6%), var(--color-surface));
  border: 1px solid color-mix(in srgb, var(--color-border) 82%, var(--sa-primary));
  border-radius: 16px;
  box-shadow: var(--shadow-popover), 0 24px 80px rgba(2, 8, 23, 0.22);
  z-index: 2000;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ai-hero {
  padding: 16px;
  border-bottom: 1px solid var(--color-border);
  background:
    radial-gradient(circle at top right, color-mix(in srgb, var(--sa-primary) 24%, transparent), transparent 44%),
    color-mix(in srgb, var(--color-surface) 88%, var(--sa-primary-soft) 12%);
}

.ai-hero-top,
.ai-brand,
.quick-actions,
.ai-context-card,
.chat-message,
.message-bubble,
.ai-input-wrapper,
.ai-input-foot {
  display: flex;
}

.ai-hero-top {
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.ai-brand {
  align-items: center;
  gap: 11px;
  min-width: 0;
}

.ai-brand-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: var(--sa-primary);
  color: #ffffff;
  box-shadow: 0 12px 28px rgba(14, 165, 233, 0.24);
}

.ai-brand p,
.ai-brand h4,
.ai-hero-copy {
  margin: 0;
}

.ai-brand p {
  color: var(--color-accent);
  font-size: 11px;
  font-weight: 900;
  letter-spacing: 0.08em;
}

.ai-brand h4 {
  font-size: 17px;
  line-height: 1.25;
}

.ai-hero-copy {
  margin-top: 12px;
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.55;
}

.close-ai {
  width: 34px;
  height: 34px;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: var(--color-surface);
  color: var(--color-text-muted);
  cursor: pointer;
}

.close-ai:hover {
  color: var(--color-text-primary);
  border-color: color-mix(in srgb, var(--sa-primary) 36%, var(--color-border));
}

.ai-content {
  flex: 1;
  padding: 14px;
  overflow-y: auto;
}

.quick-actions {
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.quick-action {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  min-height: 32px;
  padding: 6px 10px;
  border: 1px solid var(--color-border);
  border-radius: 999px;
  background: var(--color-surface);
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
}

.quick-action:hover {
  border-color: color-mix(in srgb, var(--sa-primary) 36%, var(--color-border));
  background: var(--sa-primary-soft);
  color: var(--color-accent);
}

.ai-context-card {
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
  padding: 12px;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-surface-hover);
}

.ai-context-card strong,
.ai-context-card span {
  display: block;
}

.ai-context-card strong {
  font-size: 12px;
}

.ai-context-card span {
  margin-top: 2px;
  color: var(--color-text-secondary);
  font-size: 12px;
}

.ai-context-card button {
  width: 32px;
  height: 32px;
  border: 1px solid var(--color-border);
  border-radius: 9px;
  background: var(--color-surface);
  color: var(--color-accent);
  cursor: pointer;
}

.chat-thread {
  display: grid;
  gap: 14px;
}

.chat-message {
  align-items: flex-start;
  gap: 10px;
}

.chat-message.user {
  flex-direction: row-reverse;
}

.message-avatar {
  width: 30px;
  height: 30px;
  flex: 0 0 30px;
  display: grid;
  place-items: center;
  border-radius: 9px;
  background: var(--color-surface-hover);
  color: var(--color-text-secondary);
}

.chat-message.bot .message-avatar {
  background: var(--sa-primary-soft);
  color: var(--color-accent);
}

.chat-message.user .message-avatar {
  background: color-mix(in srgb, var(--color-success) 14%, var(--color-surface));
  color: var(--color-success);
}

.message-stack {
  max-width: calc(100% - 42px);
}

.chat-message.user .message-stack {
  display: grid;
  justify-items: end;
}

.message-author {
  display: block;
  margin-bottom: 4px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
}

.message-bubble {
  align-items: flex-start;
  gap: 8px;
  max-width: 100%;
  padding: 10px 12px;
  border: 1px solid var(--color-border);
  border-radius: 14px;
  border-top-left-radius: 5px;
  background: var(--color-surface);
  color: var(--color-text-primary);
  font-size: 13px;
  line-height: 1.55;
  white-space: pre-wrap;
  box-shadow: var(--shadow-sm);
}

.chat-message.user .message-bubble {
  border-top-left-radius: 14px;
  border-top-right-radius: 5px;
  border-color: color-mix(in srgb, var(--sa-primary) 30%, var(--color-border));
  background: color-mix(in srgb, var(--sa-primary-soft) 68%, var(--color-surface));
}

.ai-input-area {
  padding: 16px;
  border-top: 1px solid var(--color-border);
  background: color-mix(in srgb, var(--color-surface) 92%, var(--color-surface-hover));
}

.ai-input-wrapper {
  align-items: flex-end;
  gap: 10px;
  border: 1px solid color-mix(in srgb, var(--color-border) 84%, var(--sa-primary));
  border-radius: 14px;
  background: var(--color-surface);
  padding: 10px;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04);
}

.ai-input-wrapper:focus-within {
  border-color: var(--color-accent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--sa-primary) 18%, transparent);
}

.ai-input-wrapper textarea {
  flex: 1;
  min-height: 58px !important;
  max-height: 170px;
  resize: none;
  background: transparent !important;
  border: 0 !important;
  color: var(--color-text-primary) !important;
  padding: 10px !important;
  line-height: 1.5;
  outline: none;
  box-shadow: none !important;
}

.send-btn {
  width: 42px;
  height: 42px;
  flex: 0 0 42px;
  border: 0;
  border-radius: 11px;
  background: var(--color-accent);
  color: #ffffff;
  cursor: pointer;
  display: grid;
  place-items: center;
}

.send-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.ai-input-foot {
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  margin-top: 8px;
  color: var(--color-text-muted);
  font-size: 11px;
}

.ai-input-foot button {
  border: 0;
  background: transparent;
  color: var(--color-accent);
  font-weight: 800;
  cursor: pointer;
}

.ai-backdrop-fade-enter-active,
.ai-backdrop-fade-leave-active {
  transition: opacity 0.2s ease;
}

.ai-backdrop-fade-enter-from,
.ai-backdrop-fade-leave-to {
  opacity: 0;
}

.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 0.3s ease;
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
}

@media (max-width: 760px) {
  .content-area {
    border-left: 0;
  }

  .main-body {
    min-width: 0;
  }

  .ai-sidebar {
    top: 56px;
    right: 8px;
    left: 8px;
    bottom: 8px;
    width: auto;
    border-radius: 14px;
  }

  .ai-floating-btn {
    right: 14px;
    bottom: 14px;
    height: 42px;
    min-width: 74px;
  }

  .quick-action {
    flex: 1 1 calc(50% - 6px);
    justify-content: center;
  }
}
</style>
