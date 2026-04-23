<template>
  <NexusLayout>
    <div class="ai-page-flex-wrapper">
      <div class="ai-container">
        <header class="nexus-feature-header">
          <div class="header-info">
            <p class="eyebrow">Advanced AI</p>
            <h1><i class="fa-solid fa-robot"></i> AI Assistant</h1>
            <p class="muted">Intelligent support for chat, task breakdown, and repository analysis. Powered by Gemini.</p>
          </div>
          <div class="nexus-controls-row">
            <span class="nexus-tag bg-[#e0f2fe] text-[#0c4a6e] font-semibold">BETA</span>
          </div>
        </header>

        <div class="repo-panel">
          <div class="repo-head">
            <div>
              <div class="panel-title">GitHub repo analysis</div>
              <div class="panel-copy">Chon repo, doc nhanh metadata va gui mot prompt phan tich task vao chat box.</div>
            </div>
            <button class="ghost-btn" type="button" :disabled="repoLoading" @click="analyzeRepository">Analyze repo</button>
          </div>
          <div class="repo-grid">
            <input v-model="repoForm.url" type="text" class="repo-input" placeholder="https://github.com/owner/repo" />
            <input v-model="repoForm.token" type="password" class="repo-input" placeholder="GitHub token (optional)" />
          </div>
          <div class="repo-actions">
            <button class="ghost-btn" type="button" @click="useQuickPrompt('Phan ra task sau thanh 3-5 subtask ro rang, co test va ban giao.')">
              Chen lenh breakdown
            </button>
            <button class="ghost-btn" type="button" :disabled="repoLoading" @click="prepareBreakdownPrompt">
              Mau prompt phan ra task
            </button>
          </div>
          <p v-if="repoStatus" class="repo-status">{{ repoStatus }}</p>
        </div>

        <div class="chat-history">
          <div v-for="(msg, idx) in chatHistory" :key="idx" class="chat-row" :class="msg.role">
            <div v-if="msg.role === 'bot'" class="bot-icon-circle"><i class="fa-solid fa-robot"></i></div>
            <div :class="['bubble', msg.role === 'user' ? 'primary' : '']">
              <div>{{ msg.content }}</div>
              <div v-if="msg.progressSteps?.length" class="thinking-steps">
                <div
                  v-for="(step, stepIdx) in msg.progressSteps"
                  :key="`${idx}-${stepIdx}`"
                  class="thinking-step"
                  :class="{ active: stepIdx <= (msg.progressIndex || 0) }"
                >
                  <i class="fa-solid fa-circle-notch" v-if="stepIdx === (msg.progressIndex || 0) && msg.isTyping"></i>
                  <i class="fa-solid fa-check" v-else-if="stepIdx < (msg.progressIndex || 0)"></i>
                  <i class="fa-regular fa-circle" v-else></i>
                  <span>{{ step }}</span>
                </div>
              </div>
              <i v-if="msg.isTyping" class="fa-solid fa-ellipsis fa-fade"></i>
            </div>
            <div v-if="msg.role === 'user'" class="user-avatar-circle">{{ userInitials }}</div>
          </div>
        </div>

        <div class="ai-chat-input-wrapper">
          <div class="input-box">
            <i class="fa-solid fa-paperclip attach-btn"></i>
            <input
              v-model="userMessage"
              type="text"
              placeholder="Hoi SprintA AI bat cu dieu gi..."
              :disabled="isLoading"
              @keyup.enter="sendMessage()"
            />
            <button class="send-btn" type="button" :disabled="isLoading || !userMessage.trim()" @click="sendMessage()">
              <span class="fa fa-paper-plane"></span>
            </button>
          </div>
          <div class="ai-disclaimer">SprintA AI co the mac sai sot. Hay kiem tra lai cac thong tin quan trong.</div>
        </div>
      </div>

      <aside class="ai-details-panel">
        <div class="panel-section">
          <div class="section-label">Tro ly AI</div>
          <div class="section-title">HANH DONG NHANH</div>
          <div class="quick-links">
            <button class="q-link" type="button" @click="useQuickPrompt('Tao lo trinh cho du an hien tai')">
              <i class="fa-solid fa-map-location-dot"></i> Tao lo trinh
            </button>
            <button class="q-link" type="button" @click="useQuickPrompt('Tom tat cac cong viec quan trong')">
              <i class="fa-regular fa-file-lines"></i> Tom tat cong viec
            </button>
            <button class="q-link" type="button" @click="useQuickPrompt('Soan ban cap nhat tien do ngan gon')">
              <i class="fa-solid fa-pen-nib"></i> Soan ban cap nhat
            </button>
            <button class="q-link" type="button" @click="prepareBreakdownPrompt()">
              <i class="fa-solid fa-list-check"></i> Breakdown task
            </button>
          </div>
        </div>

        <div class="panel-section mt-30">
          <div class="section-title">NHAC NHO</div>
          <p class="text-muted sidebar-copy">Retry Gemini da bat 3 lan o backend. Neu AI tre, bong loading se hien cac buoc dang xu ly.</p>
        </div>

        <div class="upgrade-card-wrapper">
          <div class="upgrade-card">
            <div class="plan-label">GOI PRO</div>
            <div class="plan-desc">Mo khoa cac truy van AI khong gioi han va quy trinh lam viec tuy chinh.</div>
            <button class="btn-upgrade" type="button" @click="router.push('/rewards')">Nang cap ngay</button>
          </div>
        </div>
      </aside>
    </div>

    <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" @saved="handleSidebarSaved" />
  </NexusLayout>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'

const router = useRouter()
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const showCustomizeModal = ref(false)
const sidebarPreferences = ref({ audit: true, users: true })

const userMessage = ref('')
const isLoading = ref(false)
const repoLoading = ref(false)
const repoStatus = ref('')
const repoForm = ref({
  url: '',
  token: localStorage.getItem('githubToken') || ''
})

const chatHistory = ref([
  {
    role: 'bot',
    content: 'Xin chao! Toi la tro ly AI cua SprintA. Toi co the giup ban to chuc du an, phan ra task, va phan tich repo GitHub de goi y backlog.'
  }
])

const breakdownProgressSteps = [
  'Dang phan tich task',
  'Dang goi Gemini',
  'Dang thu lai neu can',
  'Dang tong hop ket qua'
]

const defaultProgressSteps = [
  'Dang doc yeu cau',
  'Dang truy van AI',
  'Dang tong hop phan hoi'
]

let progressTimer = null

const userInitials = computed(() => {
  const name = currentUser?.name || currentUser?.fullName || 'ME'
  return name.substring(0, 2).toUpperCase()
})

const clearProgressTimer = () => {
  if (progressTimer) {
    window.clearInterval(progressTimer)
    progressTimer = null
  }
}

const isBreakdownPrompt = (message) => {
  const text = `${message || ''}`.toLowerCase()
  return text.includes('phan ra') || text.includes('breakdown') || text.includes('subtask') || text.includes('sub-work item')
}

const startThinkingMessage = (message) => {
  const progressSteps = isBreakdownPrompt(message) ? breakdownProgressSteps : defaultProgressSteps
  const thinkingMessage = {
    role: 'bot',
    content: progressSteps[0],
    isTyping: true,
    progressSteps,
    progressIndex: 0
  }

  chatHistory.value.push(thinkingMessage)
  clearProgressTimer()
  progressTimer = window.setInterval(() => {
    const activeMessage = chatHistory.value[chatHistory.value.length - 1]
    if (!activeMessage?.isTyping || !activeMessage.progressSteps?.length) {
      clearProgressTimer()
      return
    }

    const nextIndex = Math.min((activeMessage.progressIndex || 0) + 1, activeMessage.progressSteps.length - 1)
    activeMessage.progressIndex = nextIndex
    activeMessage.content = activeMessage.progressSteps[nextIndex]
  }, 900)
}

const sendMessage = async (overrideMessage = null) => {
  const outgoing = `${overrideMessage ?? userMessage.value}`.trim()
  if (!outgoing || isLoading.value) return

  if (!overrideMessage) {
    userMessage.value = ''
  }

  chatHistory.value.push({ role: 'user', content: outgoing })
  isLoading.value = true
  startThinkingMessage(outgoing)

  try {
    const history = chatHistory.value
      .filter(item => !item.isTyping)
      .slice(-10)
      .map(item => ({ role: item.role === 'bot' ? 'assistant' : 'user', content: item.content }))

    const response = await axiosClient.post('/ai/chat', { message: outgoing, history })
    clearProgressTimer()
    chatHistory.value.pop()
    chatHistory.value.push({ role: 'bot', content: response.data?.data || response.data?.message || 'AI khong tra ve noi dung.' })
  } catch (error) {
    clearProgressTimer()
    chatHistory.value.pop()
    const message = error.response?.data?.message || error.response?.data?.error || error.message || 'Khong ket noi duoc AI.'
    chatHistory.value.push({ role: 'bot', content: `AI chua gui duoc: ${message}` })
  } finally {
    isLoading.value = false
  }
}

const useQuickPrompt = (prompt) => {
  userMessage.value = prompt
}

const prepareBreakdownPrompt = () => {
  userMessage.value = 'Phan ra task sau thanh 3-5 subtask ro rang. Moi subtask can co muc tieu, owner de xuat, test/checklist va ban giao.'
}

const analyzeRepository = async () => {
  const repoUrl = repoForm.value.url.trim()
  if (!repoUrl) {
    ElMessage.warning('Hay nhap repo GitHub truoc.')
    return
  }

  const parsed = parseRepo(repoUrl)
  if (!parsed) {
    ElMessage.error('Repo URL khong dung dinh dang GitHub.')
    return
  }

  repoLoading.value = true
  repoStatus.value = 'Dang doc metadata repo...'

  try {
    if (repoForm.value.token?.trim()) {
      localStorage.setItem('githubToken', repoForm.value.token.trim())
    }

    const headers = {
      Accept: 'application/vnd.github+json'
    }

    if (repoForm.value.token?.trim()) {
      headers.Authorization = `Bearer ${repoForm.value.token.trim()}`
    }

    const [repoRes, issuesRes, readmeRes, languagesRes] = await Promise.all([
      fetch(`https://api.github.com/repos/${parsed.owner}/${parsed.repo}`, { headers }),
      fetch(`https://api.github.com/repos/${parsed.owner}/${parsed.repo}/issues?state=open&per_page=5`, { headers }),
      fetch(`https://api.github.com/repos/${parsed.owner}/${parsed.repo}/readme`, { headers }),
      fetch(`https://api.github.com/repos/${parsed.owner}/${parsed.repo}/languages`, { headers })
    ])

    if (!repoRes.ok) {
      throw new Error('Khong doc duoc repo tu GitHub API.')
    }

    const repo = await repoRes.json()
    const issues = issuesRes.ok ? await issuesRes.json() : []
    const readme = readmeRes.ok ? await readmeRes.json() : null
    const languages = languagesRes.ok ? await languagesRes.json() : {}
    const readmeSnippet = readme?.content ? atob(readme.content).slice(0, 1200).replace(/\s+/g, ' ') : 'Khong doc duoc README.'

    repoStatus.value = 'Dang tao prompt phan tich backlog...'

    const prompt = [
      `Phan tich GitHub repo ${parsed.owner}/${parsed.repo}.`,
      `Mo ta: ${repo.description || 'Khong co mo ta.'}`,
      `Ngon ngu chinh: ${Object.keys(languages).join(', ') || repo.language || 'Khong ro'}.`,
      `Open issues: ${(issues || []).map(item => item.title).join(' | ') || 'Khong co issue mo.'}`,
      `README snippet: ${readmeSnippet}`,
      'Hay de xuat backlog gom: 1) quick wins, 2) medium tasks, 3) risky tasks, 4) test/verification plan.'
    ].join(' ')

    userMessage.value = prompt
    repoStatus.value = 'Da chen prompt vao chat box. Dang gui cho AI...'
    await sendMessage(prompt)
    repoStatus.value = `Da phan tich repo ${parsed.owner}/${parsed.repo}.`
  } catch (error) {
    repoStatus.value = error.message || 'Khong phan tich duoc repo.'
    ElMessage.error(repoStatus.value)
  } finally {
    repoLoading.value = false
  }
}

const parseRepo = (url) => {
  const match = url.match(/github\.com\/([^/]+)\/([^/#?]+)/i)
  if (!match) return null
  return {
    owner: match[1],
    repo: match[2].replace(/\.git$/i, '')
  }
}

onMounted(() => {
  const saved = localStorage.getItem('sidebarPreferences')
  if (saved) {
    try {
      Object.assign(sidebarPreferences.value, JSON.parse(saved))
    } catch {
      // ignore malformed preferences
    }
  }
})

onBeforeUnmount(() => {
  clearProgressTimer()
})

const handleSidebarSaved = (prefs) => {
  const next = { ...sidebarPreferences.value }
  if (prefs?.navItems) {
    prefs.navItems.forEach(item => {
      if (['recent', 'spaces', 'ai', 'audit', 'users'].includes(item.id)) {
        next[item.id] = item.checked
      }
    })
  }
  sidebarPreferences.value = next
  localStorage.setItem('sidebarPreferences', JSON.stringify(next))
}
</script>

<style scoped>
.ai-page-flex-wrapper {
  display: flex;
  height: calc(100vh - 56px);
  width: calc(100% + 80px);
  margin: -40px;
}

.ai-container {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.ai-page-header {
  padding: 24px 40px 12px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
}

.header-pill {
  padding: 4px 10px;
  border-radius: 999px;
  background: #e0f2fe;
  color: #0c4a6e;
  font-size: 12px;
  font-weight: 600;
}

.repo-panel {
  margin: 0 40px 12px;
  padding: 16px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: var(--bg-card);
}

.repo-head,
.repo-grid,
.repo-actions,
.quick-links,
.chat-row,
.input-box,
.thinking-step {
  display: flex;
}

.repo-head,
.thinking-step {
  align-items: center;
}

.repo-head {
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 12px;
}

.panel-title {
  font-weight: 700;
  color: var(--text-primary);
}

.panel-copy,
.repo-status {
  font-size: 13px;
  color: var(--text-secondary);
}

.repo-grid {
  gap: 10px;
  margin-bottom: 10px;
}

.repo-input {
  flex: 1;
  min-width: 0;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: var(--bg-primary);
  color: var(--text-primary);
  padding: 10px 12px;
}

.repo-actions {
  gap: 10px;
}

.ghost-btn {
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: transparent;
  color: var(--text-primary);
  padding: 8px 12px;
  cursor: pointer;
}

.chat-history {
  flex: 1;
  padding: 28px 40px;
  display: flex;
  flex-direction: column;
  gap: 28px;
  overflow-y: auto;
}

.chat-row {
  gap: 16px;
  max-width: 85%;
  align-items: flex-start;
}

.chat-row.user {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.bot-icon-circle,
.user-avatar-circle {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  display: grid;
  place-items: center;
}

.bot-icon-circle {
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  color: #579dff;
}

.user-avatar-circle {
  background: #0ea5e9;
  color: #ffffff;
  font-weight: 700;
}

.bubble {
  min-width: 160px;
  padding: 14px 16px;
  border-radius: 8px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  line-height: 1.6;
}

.bubble.primary {
  background: #0ea5e9;
  color: #ffffff;
  border-color: #0ea5e9;
}

.thinking-steps {
  margin-top: 12px;
  display: grid;
  gap: 6px;
}

.thinking-step {
  gap: 8px;
  color: var(--text-secondary);
  font-size: 12px;
}

.thinking-step.active {
  color: var(--text-primary);
}

.ai-chat-input-wrapper {
  padding: 0 40px 32px;
}

.input-box {
  align-items: center;
  gap: 10px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: var(--bg-card);
  padding: 0 14px;
}

.input-box input {
  flex: 1;
  border: 0;
  background: transparent;
  color: var(--text-primary);
  padding: 16px 0;
  outline: none;
}

.attach-btn {
  color: var(--text-secondary);
}

.send-btn {
  border: 0;
  background: transparent;
  color: #0ea5e9;
  cursor: pointer;
}

.send-btn:disabled,
.ghost-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.ai-disclaimer {
  margin-top: 10px;
  color: var(--text-secondary);
  font-size: 12px;
}

.ai-details-panel {
  width: 340px;
  padding: 32px 24px;
  border-left: 1px solid var(--border-color);
  background: var(--bg-secondary);
}

.section-label {
  color: var(--text-secondary);
  font-size: 12px;
  margin-bottom: 10px;
}

.section-title {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-secondary);
  margin-bottom: 12px;
}

.quick-links {
  flex-direction: column;
  gap: 10px;
}

.q-link {
  width: 100%;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: var(--bg-card);
  color: var(--text-primary);
  padding: 10px 12px;
  text-align: left;
  cursor: pointer;
}

.mt-30 {
  margin-top: 30px;
}

.sidebar-copy {
  line-height: 1.6;
}

.upgrade-card-wrapper {
  margin-top: 30px;
}

.upgrade-card {
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: var(--bg-card);
  padding: 18px;
}

.plan-label {
  color: #0ea5e9;
  font-size: 12px;
  font-weight: 700;
  margin-bottom: 8px;
}

.plan-desc {
  color: var(--text-secondary);
  font-size: 13px;
  line-height: 1.6;
  margin-bottom: 12px;
}

.btn-upgrade {
  border: 0;
  border-radius: 6px;
  background: #0ea5e9;
  color: #ffffff;
  padding: 10px 14px;
  cursor: pointer;
}

@media (max-width: 1100px) {
  .ai-page-flex-wrapper {
    width: 100%;
    margin: 0;
    height: auto;
    flex-direction: column;
  }

  .ai-details-panel {
    width: 100%;
    border-left: 0;
    border-top: 1px solid var(--border-color);
  }

  .repo-grid {
    flex-direction: column;
  }
}
</style>
