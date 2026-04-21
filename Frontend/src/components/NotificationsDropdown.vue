<template>
  <el-dropdown trigger="click" popper-class="notifications-dropdown-popper" @visible-change="handleDropdownOpen">
    <div class="nav-icon notification-trigger">
      <el-badge :value="unreadCount" class="notification-badge" :hidden="unreadCount === 0">
        <i class="fa-solid fa-bell"></i>
      </el-badge>
    </div>

    <template #dropdown>
      <div class="jira-notifications-menu">
        <div class="notif-header">
          <h2 class="notif-title">Thông báo</h2>
          <div class="header-actions">
            <span class="unread-toggle-label">Chỉ hiện chưa đọc</span>
            <el-switch v-model="onlyUnread" size="small" />
            <el-button v-if="unreadCount > 0" type="primary" link size="small" @click="markAllAsRead">Đánh dấu đã đọc</el-button>
          </div>
        </div>

        <div class="notif-scroll-area">
          <div v-if="loading" class="notif-empty-state">
            <div class="empty-icon"><i class="fa-solid fa-spinner fa-spin"></i></div>
            <p>Loading notifications...</p>
          </div>

          <div v-else-if="filteredNotifications.length === 0" class="notif-empty-state">
            <div class="empty-icon"><i class="fa-solid fa-flag"></i></div>
            <p>Không có thông báo phù hợp trong 30 ngày gần đây.</p>
          </div>

          <div v-else class="notif-section">
            <button
              v-for="notification in filteredNotifications"
              :key="notification.id"
              type="button"
              class="notif-item"
              :class="{ unread: !notification.isRead }"
              @click="openNotification(notification)"
            >
              <div class="notif-avatar">
                {{ getInitials(notification.triggeredByName || 'HT') }}
              </div>
              <div class="notif-content">
                <div class="notif-text">
                  <span class="user-name">{{ notification.triggeredByName || 'Hệ thống' }}</span>
                  <span>{{ notification.content }}</span>
                </div>
                <div class="notif-context">
                  <i class="fa-solid fa-bell"></i>
                  <span>{{ notification.title }}</span>
                  <span class="time-ago">{{ formatTimeAgo(notification.createdAt) }}</span>
                </div>
              </div>
              <div v-if="!notification.isRead" class="unread-dot"></div>
            </button>
          </div>
        </div>
      </div>
    </template>
  </el-dropdown>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import * as signalR from '@microsoft/signalr'

const router = useRouter()
const notifications = ref([])
const onlyUnread = ref(false)
const loading = ref(false)
const connection = ref(null)

const unreadCount = computed(() => notifications.value.filter(item => !item.isRead).length)
const filteredNotifications = computed(() => {
  if (onlyUnread.value) return notifications.value.filter(item => !item.isRead)
  return notifications.value
})

const getInitials = (name) => {
  if (!name) return '?'
  return name.split(' ').map(part => part[0]).join('').slice(0, 2).toUpperCase()
}

const formatTimeAgo = (dateStr) => {
  const diffMs = new Date() - new Date(dateStr)
  if (diffMs < 60000) return 'Vừa xong'
  const diffMins = Math.floor(diffMs / 60000)
  if (diffMins < 60) return `${diffMins} phút trước`
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours} giờ trước`
  return `${Math.floor(diffHours / 24)} ngày trước`
}

const normalizeLink = (notification) => {
  if (notification.linkUrl?.startsWith('/space/')) return notification.linkUrl
  if (notification.relatedProjectId && notification.relatedTaskId) return `/space/${notification.relatedProjectId}?task=${notification.relatedTaskId}`
  if (notification.relatedProjectId) return `/space/${notification.relatedProjectId}`
  if (notification.linkUrl?.startsWith('/projects/')) {
    const parts = notification.linkUrl.split('/').filter(Boolean)
    if (parts[1]) return `/space/${parts[1]}`
  }
  return notification.linkUrl || null
}

const fetchNotifications = async () => {
  loading.value = true
  try {
    const response = await axiosClient.get('/notifications', {
      params: onlyUnread.value ? { unreadOnly: true } : {}
    })
    notifications.value = (response.data?.data || []).map(item => ({
      ...item,
      linkUrl: normalizeLink(item)
    }))
  } catch (error) {
    ElMessage.error('Could not load notifications')
  } finally {
    loading.value = false
  }
}

const openNotification = async (notification) => {
  try {
    if (!notification.isRead) {
      notification.isRead = true
      await axiosClient.put(`/notifications/${notification.id}/read`)
    }
    if (notification.linkUrl) router.push(notification.linkUrl)
  } catch (error) {
    ElMessage.error('Could not update notification')
  }
}

const markAllAsRead = async () => {
  try {
    await axiosClient.put('/notifications/read-all')
    notifications.value = notifications.value.map(item => ({ ...item, isRead: true }))
  } catch (error) {
    ElMessage.error('Could not mark all notifications as read')
  }
}

const handleDropdownOpen = (visible) => {
  if (visible) fetchNotifications()
}

const initSignalR = () => {
  const token = localStorage.getItem('accessToken') || localStorage.getItem('token')
  if (!token) return

  connection.value = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5136/notification-hub', {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .build()

  connection.value.on('ReceiveNotification', (notification) => {
    notifications.value.unshift({
      ...notification,
      isRead: false,
      linkUrl: normalizeLink(notification)
    })
  })

  connection.value.start().catch(() => {})
}

watch(onlyUnread, () => {
  fetchNotifications()
})

onMounted(() => {
  fetchNotifications()
  initSignalR()
})

onUnmounted(() => {
  if (connection.value) connection.value.stop()
})
</script>

<style scoped>
.jira-notifications-menu {
  width: 480px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  background: var(--bg-card);
  color: var(--text-primary);
}

.notif-header,
.header-actions,
.notif-context {
  display: flex;
  align-items: center;
}

.notif-header {
  justify-content: space-between;
  gap: 12px;
  padding: 16px 20px 12px;
}

.header-actions {
  gap: 12px;
}

.notif-title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}

.unread-toggle-label {
  color: var(--text-secondary);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.notif-scroll-area {
  min-height: 360px;
  overflow-y: auto;
}

.notif-section {
  padding: 10px 0;
}

.notif-item {
  width: 100%;
  display: flex;
  gap: 16px;
  padding: 12px 20px;
  border: none;
  background: transparent;
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.notif-item:hover {
  background: var(--hover-bg);
}

.notif-item.unread {
  background: rgba(7, 71, 166, 0.06);
}

.notif-avatar {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: #2563eb;
  color: #fff;
  font-size: 12px;
  font-weight: 700;
}

.notif-content {
  flex: 1;
}

.notif-text {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.user-name {
  color: var(--text-primary);
  font-weight: 700;
}

.notif-context,
.time-ago {
  color: var(--text-muted);
  font-size: 12px;
  gap: 6px;
}

.unread-dot {
  width: 6px;
  height: 6px;
  margin-top: 10px;
  border-radius: 999px;
  background: #3b82f6;
}

.notif-empty-state {
  min-height: 280px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  text-align: center;
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 30px;
  color: var(--text-muted);
}

.nav-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  color: var(--text-secondary);
  cursor: pointer;
}

.nav-icon:hover {
  background: var(--hover-bg);
}

.notification-badge :deep(.el-badge__content) {
  background: #f87171;
  border: none;
  font-size: 9px;
  height: 14px;
  line-height: 14px;
}
</style>

<style>
.el-popper.notifications-dropdown-popper {
  padding: 0 !important;
  border: 1px solid var(--border-color) !important;
  border-radius: 8px !important;
  background: var(--bg-card) !important;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.22) !important;
}
</style>
