<template>
  <div class="chat-container">
    <!-- Chat Sidebar (Channels & Direct Messages) -->
    <div class="chat-sidebar">
      <div class="sidebar-header">
        <h3 class="font-bold" style="display: flex; align-items: center; gap: 8px;">
          <i class="fa-solid fa-comments text-primary text-lg" style="margin-right: 8px;"></i>
          <span>Kênh Thảo Luận</span>
        </h3>
      </div>

      <!-- Channels List -->
      <div class="sidebar-section">
        <span class="section-title">CHANNELS</span>
        <div class="section-list">
          <button 
            v-for="ch in channels" 
            :key="ch.id" 
            class="list-item" 
            :class="{ active: activeChat.id === ch.id && activeChat.type === 'channel' }"
            @click="selectChat(ch, 'channel')"
          >
            <span class="item-icon">#</span>
            <span class="item-name truncate">{{ ch.name }}</span>
            <el-badge v-if="ch.unread" :value="ch.unread" class="ml-auto" />
          </button>
        </div>
      </div>

      <!-- Direct Messages List -->
      <div class="sidebar-section mt-4">
        <span class="section-title">TIN NHẮN TRỰC TIẾP</span>
        <div class="section-list">
          <button 
            v-for="m in members" 
            :key="m.id" 
            class="list-item" 
            :class="{ active: activeChat.id === m.id && activeChat.type === 'dm' }"
            @click="selectChat(m, 'dm')"
          >
            <div class="avatar-status-wrapper">
              <el-avatar :size="24" :src="m.avatar">{{ m.name.charAt(0) }}</el-avatar>
              <span class="status-dot" :class="m.status"></span>
            </div>
            <div class="flex flex-col text-left overflow-hidden ml-2">
              <span class="item-name truncate">{{ m.name }}</span>
              <span class="text-xs text-muted truncate">{{ m.statusText || (m.status === 'online' ? 'Online' : 'Offline') }}</span>
            </div>
            <el-badge v-if="m.unread" :value="m.unread" class="ml-auto" />
          </button>
        </div>
      </div>
    </div>

    <!-- Active Chat Area -->
    <div class="chat-main">
      <div class="chat-header">
        <div class="active-info">
          <span class="active-icon">{{ activeChat.type === 'channel' ? '#' : '@' }}</span>
          <div>
            <h4 class="font-semibold text-primary leading-tight">{{ activeChat.name }}</h4>
            <p class="text-xs text-muted leading-none">
              {{ activeChat.type === 'channel' ? activeChat.desc : (activeChat.status === 'online' ? 'Đang hoạt động' : 'Ngoại tuyến') }}
            </p>
          </div>
        </div>

        <div class="header-actions">
          <button class="action-btn" title="Kết bạn & Mời thành viên" @click="openAddFriendModal">
            <i class="fa-solid fa-user-plus text-lg"></i>
          </button>
          <button class="action-btn" title="Gọi video" @click="startVideoCall">
            <i class="fa-solid fa-video text-lg"></i>
          </button>
          <button class="action-btn" title="Tìm kiếm tin nhắn">
            <i class="fa-solid fa-magnifying-glass text-lg"></i>
          </button>
          <button class="action-btn" title="Thành viên">
            <i class="fa-solid fa-users text-lg"></i>
          </button>
        </div>
      </div>

      <!-- Messages View -->
      <div ref="messageThread" class="messages-thread">
        <div 
          v-for="(msg, idx) in activeMessages" 
          :key="idx" 
          class="message-card"
          :class="{ 'mine': msg.senderId === currentUser.id }"
        >
          <el-avatar :size="32" :src="msg.senderAvatar" class="flex-shrink-0">
            {{ msg.senderName.charAt(0) }}
          </el-avatar>
          <div class="message-body">
            <div class="message-meta">
              <span class="sender-name">{{ msg.senderName }}</span>
              <span class="send-time">{{ formatTime(msg.sentAt) }}</span>
            </div>
            <div class="message-content">
              <p>{{ msg.content }}</p>
              
              <!-- File Attachment Mockup -->
              <div v-if="msg.attachment" class="attachment-preview">
                <i class="fa-solid fa-file-pdf text-danger text-2xl"></i>
                <div class="flex flex-col ml-2 overflow-hidden">
                  <span class="text-xs font-semibold truncate">{{ msg.attachment.name }}</span>
                  <span class="text-xxs text-muted">{{ msg.attachment.size }}</span>
                </div>
                <button class="ml-auto text-primary hover:underline text-xs">Tải xuống</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Input Bar -->
      <div class="chat-input-area">
        <div class="input-actions-bar">
          <el-button size="small" class="btn-secondary" title="Đính kèm file" @click="triggerAttachment">
            <i class="fa-solid fa-paperclip"></i>
          </el-button>
          <el-button size="small" class="btn-secondary" title="Emojis" @click="addEmoji">
            <i class="fa-regular fa-smile"></i>
          </el-button>
          <span class="text-xs text-muted ml-2" v-if="isTyping">Ai đó đang nhập...</span>
        </div>
        <div class="input-form">
          <input 
            v-model="newMessage" 
            type="text" 
            placeholder="Gửi tin nhắn hoặc gõ / để mở trợ giúp..." 
            class="chat-input w-full"
            @keyup.enter="sendMessage"
          />
          <button class="btn-send" @click="sendMessage">
            <i class="fa-solid fa-paper-plane"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Video Call Overlay (WebRTC / Jitsi Simulation) -->
    <el-dialog
      v-model="videoCallActive"
      title="🎥 Virtual Meeting Room"
      width="800px"
      class="video-call-dialog"
      destroy-on-close
      append-to-body
    >
      <div class="video-grid">
        <div class="video-feed local">
          <div class="feed-placeholder">
            <el-avatar :size="80" :src="currentUser.avatar">{{ currentUser.name.charAt(0) }}</el-avatar>
            <span class="feed-name">Bạn (Quân)</span>
          </div>
        </div>
        <div class="video-feed remote">
          <div class="feed-placeholder">
            <el-avatar :size="80" src="https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=256">L</el-avatar>
            <span class="feed-name">{{ activeChat.name }}</span>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="flex justify-center gap-4">
          <el-button type="danger" class="btn-danger" @click="videoCallActive = false">
            <i class="fa-solid fa-phone-slash mr-2"></i> Kết thúc cuộc gọi
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- Add Friend Dialog -->
    <el-dialog
      v-model="addFriendActive"
      width="480px"
      class="add-friend-dialog"
      append-to-body
    >
      <template #header>
        <div class="dialog-header flex items-center" style="display: flex; align-items: center; gap: 8px;">
          <i class="fa-solid fa-user-plus text-primary text-base" style="margin-right: 8px;"></i>
          <span class="text-sm font-semibold text-primary">Kết bạn & Mời thành viên</span>
        </div>
      </template>
      <div class="add-friend-content">
        <!-- My Invite Info -->
        <div class="my-invite-card mb-5">
          <h5 class="field-label mb-3">Tài khoản của bạn</h5>
          <div class="flex flex-col gap-3">
            <div class="info-row">
              <span class="info-label">Mã kết bạn:</span>
              <div class="info-value-wrapper">
                <code class="info-code">{{ myFriendCode }}</code>
                <button class="copy-btn-link" @click="copyToClipboard(myFriendCode)">
                  <i class="fa-regular fa-copy"></i> <span>Sao chép</span>
                </button>
              </div>
            </div>
            <div class="info-row">
              <span class="info-label">Link kết bạn:</span>
              <div class="info-value-wrapper">
                <span class="info-link truncate">{{ myInviteLink }}</span>
                <button class="copy-btn-link" @click="copyToClipboard(myInviteLink)">
                  <i class="fa-regular fa-copy"></i> <span>Sao chép</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Send Invite Form -->
        <div class="send-invite-section mb-6">
          <h5 class="field-label" style="margin-bottom: 8px !important; margin-top: 20px !important;">Gửi lời mời kết bạn</h5>
          <div style="display: flex; gap: 10px; align-items: center; width: 100%;">
            <input
              v-model="searchFriendQuery"
              placeholder="Nhập mã kết bạn, email hoặc tên..."
              class="custom-friend-input"
              style="flex: 1; height: 38px;"
              @keyup.enter="sendFriendRequest"
            />
            <button class="btn-save" style="height: 38px; padding: 0 16px;" @click="sendFriendRequest">Gửi yêu cầu</button>
          </div>
        </div>

        <!-- Friend Requests List -->
        <div class="friend-requests-section">
          <h5 class="field-label" style="margin-bottom: 10px !important; margin-top: 20px !important;">
            Lời mời kết bạn đang chờ ({{ friendRequests.length }})
          </h5>
          <div v-if="friendRequests.length === 0" class="text-center py-6 text-sm text-muted">
            Không có lời mời nào đang chờ
          </div>
          <div v-else class="requests-list">
            <div v-for="req in friendRequests" :key="req.id" class="request-item" style="display: flex; align-items: center; padding: 12px 16px; justify-content: space-between;">
              <div style="display: flex; align-items: center; flex: 1; min-width: 0;">
                <el-avatar :size="36" :src="req.avatar" style="flex-shrink: 0;">{{ req.name.charAt(0) }}</el-avatar>
                <div class="flex flex-col ml-3 overflow-hidden" style="margin-left: 12px;">
                  <span class="text-sm font-semibold truncate" style="color: var(--color-text-primary); display: block;">{{ req.name }}</span>
                  <span class="text-xs text-muted truncate" style="display: block; margin-top: 2px;">{{ req.email || 'Mã: ' + req.code }}</span>
                </div>
              </div>
              <div style="display: flex; gap: 10px; margin-left: 16px; flex-shrink: 0;">
                <button class="btn-action-accept" @click="acceptFriend(req)">Đồng ý</button>
                <button class="btn-action-decline" @click="declineFriend(req)">Từ chối</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'

const currentUser = {
  id: 'user-quan',
  name: 'Đoàn Minh Quân',
  avatar: ''
}

const channels = ref([
  { id: 'ch-general', name: 'general', desc: 'Thảo luận chung của cả đội', unread: 0 },
  { id: 'ch-frontend', name: 'frontend-dev', desc: 'Nơi thảo luận về giao diện Vue 3 + Element Plus', unread: 2 },
  { id: 'ch-design', name: 'ui-ux-design', desc: 'Phản hồi thiết kế Figma', unread: 0 }
])

const members = ref([
  { id: 'user-kiet', name: 'Nguyễn Tuấn Kiệt', status: 'online', statusText: '💻 Đang code ưu tiên AI', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&q=80&w=128', unread: 0 },
  { id: 'user-phat', name: 'Trần Gia Phát', status: 'online', statusText: '☕ Uống nước nghỉ giải lao', avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128', unread: 1 },
  { id: 'user-tu', name: 'Phạm Minh Tú', status: 'away', statusText: '💬 Đang họp khách hàng', avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&q=80&w=128', unread: 0 },
  { id: 'user-dat', name: 'Lê Tiến Đạt', status: 'offline', statusText: '', avatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&q=80&w=128', unread: 0 }
])

const mockMessages = {
  'ch-general': [
    { senderId: 'user-kiet', senderName: 'Nguyễn Tuấn Kiệt', senderAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&q=80&w=128', content: 'Chào mọi người, chúc một tuần mới làm việc hiệu quả!', sentAt: new Date(Date.now() - 3600000 * 4) },
    { senderId: 'user-quan', senderName: 'Đoàn Minh Quân', senderAvatar: '', content: 'Chào cả nhà, hôm nay mình bắt đầu thiết kế Module Team Collaboration nhé.', sentAt: new Date(Date.now() - 3600000 * 3) }
  ],
  'ch-frontend': [
    { senderId: 'user-phat', senderName: 'Trần Gia Phát', senderAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128', content: 'Cậu ơi, đã hoàn thành import CSS Variables chưa?', sentAt: new Date(Date.now() - 3600000 * 2) },
    { senderId: 'user-phat', senderName: 'Trần Gia Phát', senderAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128', content: 'Tớ có gửi kèm file thiết kế UI ở đây.', sentAt: new Date(Date.now() - 3600000 * 1.9), attachment: { name: 'UI_Specification_v2.pdf', size: '2.4 MB' } }
  ],
  'user-phat': [
    { senderId: 'user-phat', senderName: 'Trần Gia Phát', senderAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128', content: 'Quân ơi, rảnh thì call video thảo luận vụ setup SignalR một chút nhé.', sentAt: new Date(Date.now() - 3600000) }
  ]
}

const activeChat = ref({
  id: 'ch-general',
  name: 'general',
  type: 'channel',
  desc: 'Thảo luận chung của cả đội'
})

const activeMessages = ref([])
const newMessage = ref('')
const messageThread = ref(null)
const videoCallActive = ref(false)
const isTyping = ref(false)

// Add Friend system refs & state
const addFriendActive = ref(false)
const searchFriendQuery = ref('')
const myFriendCode = 'QUAN-9982'
const myInviteLink = `http://localhost:5173/collaboration?invite=${myFriendCode}`
const friendRequests = ref([
  { id: 'req-son', name: 'Đặng Ngọc Sơn', email: 'son.dang@example.com', code: 'SON-4421', avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&q=80&w=128' }
])

const openAddFriendModal = () => {
  addFriendActive.value = true
}

const copyToClipboard = (text) => {
  navigator.clipboard.writeText(text).then(() => {
    ElMessage.success('Đã sao chép vào bộ nhớ tạm!')
  }).catch(() => {
    ElMessage.error('Sao chép thất bại')
  })
}

const sendFriendRequest = () => {
  if (!searchFriendQuery.value.trim()) {
    ElMessage.warning('Vui lòng nhập mã kết bạn, email hoặc tên!')
    return
  }
  ElMessage.success(`Đã gửi lời mời kết bạn đến "${searchFriendQuery.value}" thành công!`)
  searchFriendQuery.value = ''
}

const acceptFriend = (req) => {
  const newUserId = req.id.replace('req-', 'user-')
  members.value.push({
    id: newUserId,
    name: req.name,
    status: 'online',
    statusText: '🎉 Vừa kết bạn',
    avatar: req.avatar || '',
    unread: 0
  })
  mockMessages[newUserId] = [
    { senderId: newUserId, senderName: req.name, senderAvatar: req.avatar, content: 'Chào Quân, mình đã đồng ý kết bạn. Rất vui được hợp tác!', sentAt: new Date() }
  ]
  friendRequests.value = friendRequests.value.filter(r => r.id !== req.id)
  ElMessage.success(`Đã đồng ý kết bạn với ${req.name}`)
}

const declineFriend = (req) => {
  friendRequests.value = friendRequests.value.filter(r => r.id !== req.id)
  ElMessage.info(`Đã từ chối lời mời của ${req.name}`)
}

const selectChat = (item, type) => {
  activeChat.value = {
    id: item.id,
    name: item.name,
    type: type,
    desc: item.desc || '',
    status: item.status || 'offline'
  }
  
  // Ensure the mock container is initialized
  if (!mockMessages[item.id]) {
    mockMessages[item.id] = []
  }
  
  activeMessages.value = mockMessages[item.id]
  item.unread = 0
  
  nextTick(() => {
    scrollToBottom()
  })
}

const sendMessage = () => {
  if (!newMessage.value.trim()) return
  
  const msgObj = {
    senderId: currentUser.id,
    senderName: currentUser.name,
    senderAvatar: currentUser.avatar,
    content: newMessage.value,
    sentAt: new Date()
  }
  
  // Only push once to activeMessages which references the correct mockMessages array
  activeMessages.value.push(msgObj)
  newMessage.value = ''
  
  nextTick(() => {
    scrollToBottom()
  })

  // Simulated auto response
  setTimeout(() => {
    isTyping.value = true
    setTimeout(() => {
      isTyping.value = false
      const replies = [
        'Tuyệt quá Quân ơi! 👍',
        'Ok, mình nhận được thông tin rồi.',
        'Đã hiểu, để tớ kiểm tra lại xem sao.',
        'Có gì mới tớ sẽ báo liền.'
      ]
      const randomReply = replies[Math.floor(Math.random() * replies.length)]
      const botMsg = {
        senderId: 'user-phat',
        senderName: 'Trần Gia Phát',
        senderAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128',
        content: randomReply,
        sentAt: new Date()
      }
      activeMessages.value.push(botMsg)
      nextTick(() => {
        scrollToBottom()
      })
    }, 1500)
  }, 1000)
}

const scrollToBottom = () => {
  if (messageThread.value) {
    messageThread.value.scrollTop = messageThread.value.scrollHeight
  }
}

const startVideoCall = () => {
  videoCallActive.value = true
}

const triggerAttachment = () => {
  ElMessage.info('Chức năng đính kèm tệp sẽ khả dụng khi kết nối Backend.')
}

const addEmoji = () => {
  newMessage.value += '😊'
}

const formatTime = (dateObj) => {
  const d = new Date(dateObj)
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

onMounted(() => {
  selectChat(channels.value[0], 'channel')
})
</script>

<style scoped>
.chat-container {
  display: flex;
  width: min(100% - 44px, 1280px);
  height: calc(100vh - 132px);
  margin: 22px auto;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 18px 46px color-mix(in srgb, #020617 12%, transparent);
}

.chat-sidebar {
  width: 248px;
  border-right: 1px solid var(--color-border);
  background-color: var(--sa-sidebar);
  display: flex;
  flex-direction: column;
  padding: 14px;
}
.sidebar-header {
  margin-bottom: 14px;
}
.sidebar-section {
  display: flex;
  flex-direction: column;
  margin-bottom: 14px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-card);
  padding: 12px;
  background-color: rgba(255, 255, 255, 0.01);
}

.section-title {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-muted);
  letter-spacing: 0.08em;
  margin-bottom: 12px;
  text-transform: uppercase;
}

.section-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  border: none !important;
  padding: 0 !important;
  background-color: transparent !important;
}

.list-item {
  display: flex;
  align-items: center;
  padding: 7px 10px;
  border: none;
  border-radius: var(--radius-button);
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
  gap: 10px;
}

.list-item:hover, .list-item.active {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.list-item.active {
  font-weight: 600;
  background-color: var(--sa-primary-soft);
  color: var(--color-accent);
}
.item-icon {
  margin-right: 8px;
  font-weight: bold;
}
.avatar-status-wrapper {
  position: relative;
  display: inline-block;
}

.status-dot {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  border: 1.5px solid var(--color-surface);
}

.status-dot.online { background-color: var(--color-success); }
.status-dot.away { background-color: var(--color-warning); }
.status-dot.offline { background-color: var(--color-text-muted); }

.chat-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  background-color: var(--color-surface);
}

.chat-header {
  min-height: 58px;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
}

.active-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.active-icon {
  font-size: 20px;
  font-weight: bold;
  color: var(--color-text-muted);
}

.header-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: 6px;
  border-radius: var(--radius-button);
  transition: all 0.2s;
}

.action-btn:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.messages-thread {
  flex: 1;
  padding: 16px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.message-card {
  display: flex;
  gap: 12px;
  max-width: 80%;
  align-self: flex-start;
}

.message-card.mine {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.message-body {
  display: flex;
  flex-direction: column;
}

.message-card.mine .message-body {
  align-items: flex-end;
}

.message-meta {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 4px;
}

.sender-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.send-time {
  font-size: 10px;
  color: var(--color-text-muted);
}

.message-content {
  background-color: var(--color-surface-hover);
  padding: 9px 12px;
  border-radius: 12px;
  border-top-left-radius: 0;
  color: var(--color-text-primary);
  font-size: 14px;
  border: 1px solid var(--color-border);
}

.message-card.mine .message-content {
  background-color: var(--color-accent);
  color: white;
  border-top-left-radius: 12px;
  border-top-right-radius: 0;
  border: none;
}

.attachment-preview {
  display: flex;
  align-items: center;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  padding: 8px 12px;
  border-radius: 8px;
  margin-top: 8px;
  min-width: 200px;
}

.message-card.mine .attachment-preview {
  background-color: rgba(255, 255, 255, 0.15);
  border-color: transparent;
}

.message-card.mine .attachment-preview .text-primary {
  color: #fff;
}

.chat-input-area {
  padding: 12px 16px;
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface);
}

.input-actions-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.input-form {
  display: flex;
  gap: 8px;
}

.chat-input {
  border: 2px solid var(--color-border) !important;
  border-radius: 8px !important;
  height: 40px !important;
  padding-inline: 12px !important;
}

.btn-send {
  width: 40px;
  height: 40px;
  border: none;
  background-color: var(--color-accent);
  color: white;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
}

.btn-send:hover {
  background-color: var(--color-accent-hover);
}

.video-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  height: 380px;
}

.video-feed {
  background-color: #0f172a;
  border-radius: 12px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid #334155;
  position: relative;
}

.feed-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.feed-name {
  color: #e2e8f0;
  font-size: 14px;
  font-weight: 500;
  background-color: rgba(15, 23, 42, 0.6);
  padding: 4px 10px;
  border-radius: 6px;
}

@media (max-width: 900px) {
  .chat-container {
    width: calc(100% - 24px);
    height: calc(100vh - 112px);
    margin: 12px auto;
  }

  .chat-sidebar {
    width: 210px;
  }
}

@media (max-width: 720px) {
  .chat-container {
    flex-direction: column;
    height: auto;
    min-height: calc(100vh - 112px);
  }

  .chat-sidebar {
    width: 100%;
    max-height: 240px;
    border-right: 0;
    border-bottom: 1px solid var(--color-border);
  }

  .messages-thread {
    min-height: 420px;
  }
}
</style>

<style>
.add-friend-dialog {
  background-color: #111c2d !important;
  border: 1px solid rgba(56, 189, 248, 0.15) !important;
  border-radius: 12px !important;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.4) !important;
  overflow: hidden !important;
}

.add-friend-dialog .el-dialog__header {
  margin-right: 0 !important;
  padding: 20px 24px 16px !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06) !important;
  background-color: #0b1523 !important;
}

.add-friend-dialog .el-dialog__headerbtn {
  top: 20px !important;
  right: 20px !important;
  margin-top: 0 !important;
}

.add-friend-dialog .el-dialog__title {
  display: flex !important;
  align-items: center !important;
  gap: 10px !important;
}

.add-friend-dialog .dialog-header span {
  font-size: 15px !important;
  font-weight: 600 !important;
  color: #e2e8f0 !important;
}

.add-friend-dialog .dialog-header i {
  color: #38bdf8 !important;
  text-shadow: 0 0 10px rgba(56, 189, 248, 0.5) !important;
}

.add-friend-dialog .el-dialog__body {
  padding: 24px !important;
  background-color: #111c2d !important;
}

.add-friend-dialog .my-invite-card {
  background: linear-gradient(135deg, rgba(56, 189, 248, 0.06), rgba(56, 189, 248, 0.01)) !important;
  border: 1px solid rgba(56, 189, 248, 0.15) !important;
  border-radius: 8px !important;
  padding: 16px !important;
}

.add-friend-dialog .info-row {
  display: flex !important;
  align-items: center !important;
  justify-content: space-between !important;
  font-size: 13px !important;
  gap: 12px !important;
}

.add-friend-dialog .info-label {
  color: #9fb0c8 !important;
  font-weight: 500 !important;
  width: 90px !important;
  flex-shrink: 0 !important;
}

.add-friend-dialog .info-value-wrapper {
  display: flex !important;
  align-items: center !important;
  gap: 10px !important;
  flex: 1 !important;
  min-width: 0 !important;
  justify-content: flex-end !important;
}

.add-friend-dialog .info-code {
  background-color: rgba(56, 189, 248, 0.12) !important;
  color: #38bdf8 !important;
  padding: 3px 10px !important;
  border-radius: 4px !important;
  font-family: monospace !important;
  font-weight: 600 !important;
  border: 1px solid rgba(56, 189, 248, 0.2) !important;
  font-size: 13px !important;
}

.add-friend-dialog .info-link {
  color: #e2e8f0 !important;
  font-size: 12px !important;
  text-decoration: none !important;
  border-bottom: 1px dashed rgba(255, 255, 255, 0.2) !important;
  flex: 1 !important;
  min-width: 0 !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
  text-align: right !important;
}

.add-friend-dialog .copy-btn-link {
  background: transparent !important;
  border: none !important;
  color: #38bdf8 !important;
  cursor: pointer !important;
  font-size: 12px !important;
  font-weight: 600 !important;
  padding: 4px 8px !important;
  border-radius: 4px !important;
  display: inline-flex !important;
  align-items: center !important;
  gap: 6px !important;
  white-space: nowrap !important;
  flex-shrink: 0 !important;
  transition: all 0.2s ease !important;
  box-shadow: none !important;
}

.add-friend-dialog .copy-btn-link:hover {
  background-color: rgba(56, 189, 248, 0.12) !important;
  color: #7dd3fc !important;
}

.add-friend-dialog .custom-friend-input {
  flex: 1 !important;
  border: 1px solid rgba(255, 255, 255, 0.1) !important;
  border-radius: 6px !important;
  background-color: #0b131f !important;
  padding: 0 14px !important;
  color: #f8fafc !important;
  font-size: 13px !important;
  height: 38px !important;
  outline: none !important;
  transition: all 0.2s ease !important;
}

.add-friend-dialog .custom-friend-input:focus {
  border-color: #38bdf8 !important;
  box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.15) !important;
  background-color: #070d16 !important;
}

.add-friend-dialog .field-label {
  display: block !important;
  font-size: 10px !important;
  font-weight: 700 !important;
  text-transform: uppercase !important;
  letter-spacing: 0.08em !important;
  color: #64748b !important;
  margin-bottom: 10px !important;
}

.add-friend-dialog .requests-list {
  display: flex !important;
  flex-direction: column !important;
  gap: 10px !important;
}

.add-friend-dialog .request-item {
  display: flex !important;
  align-items: center !important;
  padding: 12px 16px !important;
  border: 1px solid rgba(255, 255, 255, 0.05) !important;
  border-radius: 8px !important;
  background-color: rgba(255, 255, 255, 0.02) !important;
  gap: 12px !important;
  transition: all 0.2s ease !important;
}

.add-friend-dialog .request-item:hover {
  background-color: rgba(255, 255, 255, 0.04) !important;
  border-color: rgba(255, 255, 255, 0.08) !important;
}

.add-friend-dialog .btn-save {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  line-height: 1 !important;
  background: linear-gradient(135deg, #0ea5e9, #0284c7) !important;
  border: none !important;
  color: #ffffff !important;
  font-weight: 600 !important;
  font-size: 13px !important;
  height: 38px !important;
  padding: 0 20px !important;
  border-radius: 6px !important;
  cursor: pointer !important;
  transition: all 0.2s ease !important;
  white-space: nowrap !important;
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.2) !important;
}

.add-friend-dialog .btn-save:hover {
  background: linear-gradient(135deg, #38bdf8, #0ea5e9) !important;
  transform: translateY(-1px) !important;
  box-shadow: 0 6px 16px rgba(56, 189, 248, 0.3) !important;
}

.add-friend-dialog .btn-action-accept {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  line-height: 1 !important;
  background: linear-gradient(135deg, #0ea5e9, #0284c7) !important;
  border: none !important;
  color: #fff !important;
  font-weight: 600 !important;
  font-size: 12px !important;
  height: 30px !important;
  padding: 0 14px !important;
  border-radius: 6px !important;
  cursor: pointer !important;
  white-space: nowrap !important;
  box-shadow: 0 3px 8px rgba(14, 165, 233, 0.15) !important;
  transition: all 0.2s ease !important;
}

.add-friend-dialog .btn-action-accept:hover {
  background: linear-gradient(135deg, #38bdf8, #0ea5e9) !important;
  transform: translateY(-1px) !important;
  box-shadow: 0 4px 12px rgba(56, 189, 248, 0.25) !important;
}

.add-friend-dialog .btn-action-decline {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  line-height: 1 !important;
  background: rgba(255, 255, 255, 0.05) !important;
  border: 1px solid rgba(255, 255, 255, 0.1) !important;
  color: #9fb0c8 !important;
  font-weight: 500 !important;
  font-size: 12px !important;
  height: 30px !important;
  padding: 0 14px !important;
  border-radius: 6px !important;
  cursor: pointer !important;
  white-space: nowrap !important;
  transition: all 0.2s ease !important;
}

.add-friend-dialog .btn-action-decline:hover {
  background: rgba(255, 255, 255, 0.08) !important;
  color: #f1f5f9 !important;
  border-color: rgba(255, 255, 255, 0.2) !important;
}
</style>


