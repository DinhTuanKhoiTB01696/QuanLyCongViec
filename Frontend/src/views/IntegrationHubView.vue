<template>
  <main class="integration-page">
      <section class="hero-shell">
        <div class="hero-copy">
          <p class="kicker">Khôi · Integration Hub</p>
          <h1>{{ t('Trung tâm tích hợp công việc', 'Work Integration Hub') }}</h1>
          <p class="hero-subtitle">
            {{ t(
              'Kết nối Google Calendar thật để đưa cuộc họp, deadline và lịch sprint vào một inbox công việc có thể tạo task ngay.',
              'Connect real Google Calendar events into a work inbox that can turn signals into tasks.'
            ) }}
          </p>
        </div>

        <div class="hero-action-card" :class="{ connected: isGoogleConnected }">
          <span class="hero-status">{{ googleStatusLabel }}</span>
          <strong>{{ isGoogleConnected ? t('Google Calendar đã sẵn sàng', 'Google Calendar is ready') : t('Bắt đầu bằng Google Calendar', 'Start with Google Calendar') }}</strong>
          <p>
            {{ isGoogleConnected
              ? t('Đồng bộ sự kiện mới nhất để cập nhật Unified Inbox.', 'Sync the latest events to update Unified Inbox.')
              : t('Kết nối một lần, sau đó Khôi có thể đồng bộ event thật bất cứ lúc nào.', 'Connect once, then sync real events whenever needed.') }}
          </p>
          <div class="hero-actions">
            <button
              v-if="!isGoogleConnected"
              class="primary cta"
              type="button"
              :disabled="connecting"
              @click="connectGoogleCalendar"
            >
              <i :class="connecting ? 'fa-solid fa-circle-notch fa-spin' : 'fa-brands fa-google'"></i>
              {{ connecting ? t('Đang mở Google...', 'Opening Google...') : t('Kết nối Google Calendar', 'Connect Google Calendar') }}
            </button>
            <button
              v-else
              class="primary cta"
              type="button"
              :disabled="syncing"
              @click="syncGoogleCalendar"
            >
              <i class="fa-solid fa-rotate" :class="{ 'fa-spin': syncing }"></i>
              {{ syncing ? t('Đang đồng bộ', 'Syncing') : t('Đồng bộ ngay', 'Sync now') }}
            </button>
          </div>
        </div>
      </section>

      <section v-if="notice" class="notice" :class="notice.type">
        <i :class="notice.type === 'error' ? 'fa-solid fa-triangle-exclamation' : 'fa-solid fa-circle-info'"></i>
        <span>{{ notice.message }}</span>
        <button type="button" @click="notice = null" :aria-label="t('Đóng thông báo', 'Close notice')">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </section>

      <section class="stats-grid" aria-label="Integration metrics">
        <article>
          <span>{{ t('Ứng dụng đã kết nối', 'Connected apps') }}</span>
          <strong>{{ connectedCount }}/3</strong>
        </article>
        <article>
          <span>{{ t('Inbox chưa đọc', 'Unread inbox') }}</span>
          <strong>{{ unreadCount }}</strong>
        </article>
        <article>
          <span>{{ t('Sự kiện lịch', 'Calendar events') }}</span>
          <strong>{{ sourceCount('calendar') }}</strong>
        </article>
        <article>
          <span>{{ t('Lần sync gần nhất', 'Last sync') }}</span>
          <strong>{{ formatDate(googleCalendar?.lastSyncedAt) || t('Chưa có', 'Not yet') }}</strong>
        </article>
      </section>

      <section class="workspace-grid">
        <aside class="panel apps-panel">
          <div class="panel-head">
            <div>
              <h2>{{ t('Ứng dụng kết nối', 'Connected apps') }}</h2>
              <p>{{ t('Google Calendar là provider thật trong MVP. Gmail và Slack sẽ hỗ trợ sau.', 'Google Calendar is the real MVP provider. Gmail and Slack are coming later.') }}</p>
            </div>
            <button class="icon-btn" type="button" :disabled="loadingIntegrations" @click="loadIntegrations" :title="t('Tải lại kết nối', 'Reload connections')">
              <i class="fa-solid fa-arrows-rotate" :class="{ 'fa-spin': loadingIntegrations }"></i>
            </button>
          </div>

          <div v-if="loadingIntegrations" class="provider-list">
            <article v-for="n in 3" :key="n" class="provider-card skeleton-card">
              <span></span>
              <div><b></b><small></small><small></small></div>
            </article>
          </div>

          <div v-else-if="integrationsError" class="inline-state error">
            <i class="fa-solid fa-triangle-exclamation"></i>
            <strong>{{ integrationsError }}</strong>
            <button class="ghost small" type="button" @click="loadIntegrations">{{ t('Thử lại', 'Try again') }}</button>
          </div>

          <div v-else class="provider-list">
            <article
              v-for="provider in providers"
              :key="provider.provider"
              class="provider-card"
              :class="[provider.status, { featured: provider.provider === 'google-calendar' }]"
            >
              <div class="provider-icon" :class="provider.source">
                <i :class="providerIcon(provider.provider)"></i>
              </div>

              <div class="provider-body">
                <div class="provider-top">
                  <strong>{{ provider.name }}</strong>
                  <span class="status-badge" :class="provider.status">{{ providerStatus(provider) }}</span>
                </div>
                <p>{{ providerDescription(provider) }}</p>
                <div class="provider-meta">
                  <span v-if="provider.accountEmail"><i class="fa-regular fa-user"></i>{{ provider.accountEmail }}</span>
                  <span v-if="provider.lastSyncedAt"><i class="fa-regular fa-clock"></i>{{ formatDate(provider.lastSyncedAt) }}</span>
                  <span v-if="provider.status === 'coming_soon'"><i class="fa-solid fa-hourglass-half"></i>{{ t('Sẽ hỗ trợ sau', 'Coming soon') }}</span>
                  <span v-if="provider.status === 'not_connected'"><i class="fa-solid fa-link-slash"></i>{{ t('Chưa kết nối', 'Not connected') }}</span>
                </div>
              </div>

              <div class="provider-actions">
                <button
                  v-if="provider.provider === 'google-calendar' && provider.status !== 'connected'"
                  class="primary small"
                  type="button"
                  :disabled="connecting"
                  @click="connectGoogleCalendar"
                >
                  <i :class="connecting ? 'fa-solid fa-circle-notch fa-spin' : 'fa-brands fa-google'"></i>
                  {{ connecting ? t('Đang mở...', 'Opening...') : t('Kết nối Google Calendar', 'Connect Google Calendar') }}
                </button>

                <template v-if="provider.provider === 'google-calendar' && provider.status === 'connected'">
                  <button class="primary small" type="button" :disabled="syncing" @click="syncGoogleCalendar">
                    <i class="fa-solid fa-rotate" :class="{ 'fa-spin': syncing }"></i>
                    {{ syncing ? t('Đang đồng bộ', 'Syncing') : t('Đồng bộ ngay', 'Sync now') }}
                  </button>
                  <button class="ghost small danger-text" type="button" @click="disconnect(provider.id)">
                    <i class="fa-solid fa-link-slash"></i>
                    {{ t('Ngắt kết nối', 'Disconnect') }}
                  </button>
                </template>

                <button
                  v-if="provider.provider !== 'google-calendar'"
                  class="ghost small"
                  type="button"
                  disabled
                >
                  {{ t('Sẽ hỗ trợ sau', 'Coming soon') }}
                </button>
              </div>
            </article>
          </div>

          <section class="sync-history">
            <div class="section-title">
              <h3>{{ t('Lịch sử đồng bộ', 'Sync history') }}</h3>
              <span>{{ syncHistory.length }}</span>
            </div>

            <div v-if="syncHistory.length === 0" class="history-empty">
              <i class="fa-regular fa-clock"></i>
              <span>{{ t('Chưa có lần đồng bộ nào.', 'No sync has run yet.') }}</span>
            </div>

            <div v-else class="history-list">
              <article v-for="log in syncHistory" :key="log.id" class="history-row" :class="log.status">
                <i :class="log.status === 'success' ? 'fa-solid fa-circle-check' : 'fa-solid fa-circle-exclamation'"></i>
                <div>
                  <strong>{{ log.provider }} · {{ syncStatusLabel(log.status) }}</strong>
                  <span>{{ log.message || t('Không có ghi chú', 'No message') }}</span>
                  <small>{{ formatDate(log.startedAt) }}</small>
                </div>
              </article>
            </div>
          </section>
        </aside>

        <section class="panel inbox-panel">
          <div class="panel-head inbox-title">
            <div>
              <h2>{{ t('Unified Inbox', 'Unified Inbox') }}</h2>
              <p>{{ t('Tất cả mục đều đọc từ API và database thật.', 'Every item is read from the real API and database.') }}</p>
            </div>
            <button class="ghost small" type="button" :disabled="loadingInbox" @click="loadInbox">
              <i class="fa-solid fa-arrows-rotate" :class="{ 'fa-spin': loadingInbox }"></i>
              {{ t('Tải lại', 'Reload') }}
            </button>
          </div>

          <nav class="tabs" aria-label="Unified Inbox filters">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              type="button"
              :class="{ active: activeTab === tab.id }"
              @click="activeTab = tab.id"
            >
              <i :class="tab.icon"></i>
              {{ tab.label }}
              <span>{{ sourceCount(tab.id) }}</span>
            </button>
          </nav>

          <div v-if="loadingInbox" class="inbox-list">
            <article v-for="n in 4" :key="n" class="inbox-skeleton">
              <span></span>
              <div><b></b><small></small><small></small></div>
            </article>
          </div>

          <div v-else-if="inboxError" class="empty-state error">
            <i class="fa-solid fa-triangle-exclamation"></i>
            <strong>{{ inboxError }}</strong>
            <p>{{ t('Không tải được Unified Inbox. Hãy thử lại sau vài giây.', 'Unified Inbox could not load. Try again in a few seconds.') }}</p>
            <button class="primary small" type="button" @click="loadInbox">{{ t('Thử lại', 'Try again') }}</button>
          </div>

          <div v-else-if="filteredInbox.length === 0" class="empty-state">
            <i :class="emptyStateIcon"></i>
            <strong>{{ emptyTitle }}</strong>
            <p>{{ emptyBody }}</p>
            <button
              v-if="!isGoogleConnected"
              class="primary small"
              type="button"
              :disabled="connecting"
              @click="connectGoogleCalendar"
            >
              <i :class="connecting ? 'fa-solid fa-circle-notch fa-spin' : 'fa-brands fa-google'"></i>
              {{ t('Kết nối Google Calendar', 'Connect Google Calendar') }}
            </button>
            <button
              v-else-if="activeTab === 'all' || activeTab === 'calendar'"
              class="primary small"
              type="button"
              :disabled="syncing"
              @click="syncGoogleCalendar"
            >
              <i class="fa-solid fa-rotate" :class="{ 'fa-spin': syncing }"></i>
              {{ t('Đồng bộ lại', 'Sync again') }}
            </button>
          </div>

          <div v-else class="inbox-list">
            <button
              v-for="item in filteredInbox"
              :key="item.id"
              type="button"
              class="inbox-item"
              :class="{ selected: selectedItem?.id === item.id, unread: !item.isRead }"
              @click="selectItem(item)"
            >
              <span class="source-icon" :class="item.source"><i :class="sourceIcon(item.source)"></i></span>
              <span class="item-main">
                <span class="item-title">
                  <strong>{{ item.title }}</strong>
                  <small>{{ formatDate(item.startsAt || item.createdAt) }}</small>
                </span>
                <span class="item-copy">{{ previewText(item.content) }}</span>
                <span class="item-tags">
                  <span>{{ sourceLabel(item.source) }}</span>
                  <span v-if="!item.isRead" class="unread-label">{{ t('Chưa đọc', 'Unread') }}</span>
                  <span v-if="item.createdTaskId" class="linked">{{ t('Đã tạo task', 'Task created') }}</span>
                </span>
              </span>
              <span v-if="!item.isRead" class="unread-dot"></span>
            </button>
          </div>
        </section>

        <aside class="panel detail-panel">
          <template v-if="selectedItem">
            <div class="detail-head">
              <span class="source-icon large" :class="selectedItem.source"><i :class="sourceIcon(selectedItem.source)"></i></span>
              <div>
                <p>{{ sourceLabel(selectedItem.source) }}</p>
                <h2>{{ selectedItem.title }}</h2>
              </div>
            </div>

            <dl class="detail-list">
              <div>
                <dt>{{ t('Provider', 'Provider') }}</dt>
                <dd>{{ selectedItem.provider }}</dd>
              </div>
              <div v-if="selectedItem.startsAt">
                <dt>{{ t('Bắt đầu', 'Starts') }}</dt>
                <dd>{{ formatFullDate(selectedItem.startsAt) }}</dd>
              </div>
              <div v-if="selectedItem.endsAt">
                <dt>{{ t('Kết thúc', 'Ends') }}</dt>
                <dd>{{ formatFullDate(selectedItem.endsAt) }}</dd>
              </div>
              <div v-if="selectedItem.location">
                <dt>{{ t('Địa điểm', 'Location') }}</dt>
                <dd>{{ selectedItem.location }}</dd>
              </div>
            </dl>

            <section class="content-box">
              <h3>{{ t('Nội dung gốc', 'Original content') }}</h3>
              <p>{{ selectedItem.content || t('Provider không trả mô tả cho mục này.', 'The provider did not return a description for this item.') }}</p>
            </section>

            <button
              class="primary full"
              type="button"
              :disabled="creatingTask || !!selectedItem.createdTaskId"
              @click="createTask(selectedItem)"
            >
              <i :class="creatingTask ? 'fa-solid fa-circle-notch fa-spin' : 'fa-solid fa-square-plus'"></i>
              {{ selectedItem.createdTaskId ? t('Đã tạo task', 'Task created') : creatingTask ? t('Đang tạo task', 'Creating task') : t('Tạo task từ mục này', 'Create task from this item') }}
            </button>
          </template>

          <div v-else class="empty-state detail-empty">
            <i class="fa-regular fa-hand-pointer"></i>
            <strong>{{ t('Chọn một mục inbox', 'Select an inbox item') }}</strong>
            <p>{{ t('Chi tiết nguồn, thời gian và hành động tạo task sẽ hiện ở đây.', 'Source details, time, and task action will appear here.') }}</p>
          </div>
        </aside>
      </section>
  </main>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import { useI18nStore } from '@/store/useI18nStore'
import { getScopedCurrentProjectId } from '@/utils/projectContext'

const route = useRoute()
const router = useRouter()
const i18nStore = useI18nStore()

const providers = ref([])
const syncHistory = ref([])
const inboxItems = ref([])
const activeTab = ref('all')
const selectedItemId = ref('')
const loadingIntegrations = ref(false)
const loadingInbox = ref(false)
const connecting = ref(false)
const syncing = ref(false)
const creatingTask = ref(false)
const integrationsError = ref('')
const inboxError = ref('')
const notice = ref(null)

const t = (vi, en) => i18nStore.locale === 'en' ? en : vi

const tabs = computed(() => [
  { id: 'all', label: t('Tất cả', 'All'), icon: 'fa-solid fa-layer-group' },
  { id: 'calendar', label: t('Lịch', 'Calendar'), icon: 'fa-regular fa-calendar' },
  { id: 'email', label: 'Email', icon: 'fa-regular fa-envelope' },
  { id: 'slack', label: 'Slack', icon: 'fa-brands fa-slack' },
  { id: 'system', label: t('Hệ thống', 'System'), icon: 'fa-regular fa-bell' }
])

const googleCalendar = computed(() => providers.value.find(provider => provider.provider === 'google-calendar') || null)
const isGoogleConnected = computed(() => googleCalendar.value?.status === 'connected')
const connectedCount = computed(() => providers.value.filter(provider => provider.status === 'connected').length)
const unreadCount = computed(() => inboxItems.value.filter(item => !item.isRead).length)
const filteredInbox = computed(() => activeTab.value === 'all'
  ? inboxItems.value
  : inboxItems.value.filter(item => item.source === activeTab.value))
const selectedItem = computed(() => inboxItems.value.find(item => item.id === selectedItemId.value) || filteredInbox.value[0] || null)
const googleStatusLabel = computed(() => isGoogleConnected.value ? t('Đã kết nối', 'Connected') : t('Chưa kết nối', 'Not connected'))
const emptyStateIcon = computed(() => !isGoogleConnected.value ? 'fa-solid fa-plug-circle-bolt' : 'fa-regular fa-calendar-xmark')
const emptyTitle = computed(() => {
  if (!isGoogleConnected.value) return t('Bạn chưa kết nối ứng dụng nào', 'No app is connected yet')
  if (activeTab.value === 'calendar' || activeTab.value === 'all') return t('Chưa tìm thấy sự kiện nào', 'No calendar events found')
  return t('Chưa có dữ liệu trong bộ lọc này', 'No items in this filter')
})
const emptyBody = computed(() => {
  if (!isGoogleConnected.value) {
    return t(
      'Hãy kết nối Google Calendar để bắt đầu đồng bộ cuộc họp, deadline và lịch sprint vào Unified Inbox.',
      'Connect Google Calendar to start syncing meetings, deadlines, and sprint schedules into Unified Inbox.'
    )
  }
  if (activeTab.value === 'calendar' || activeTab.value === 'all') {
    return t(
      'Chưa tìm thấy sự kiện nào trong Google Calendar. Hãy tạo event trong lịch hoặc thử đồng bộ lại.',
      'No Google Calendar event was found. Create an event in Calendar or try syncing again.'
    )
  }
  return t('Gmail và Slack đang ở trạng thái sẽ hỗ trợ sau, nên chưa có dữ liệu thật cho tab này.', 'Gmail and Slack are coming later, so this tab has no real data yet.')
})

const getPayload = (response) => response.data?.data ?? response.data
const asArray = (value) => {
  if (Array.isArray(value)) return value
  if (Array.isArray(value?.$values)) return value.$values
  if (Array.isArray(value?.items)) return value.items
  if (Array.isArray(value?.data)) return value.data
  if (Array.isArray(value?.data?.$values)) return value.data.$values
  return []
}

const loadIntegrations = async () => {
  loadingIntegrations.value = true
  integrationsError.value = ''
  try {
    const response = await axiosClient.get('/integrations')
    const data = getPayload(response)
    providers.value = asArray(data?.providers)
    syncHistory.value = asArray(data?.syncHistory)
  } catch (error) {
    integrationsError.value = error.response?.data?.message || t('Không tải được Integration Hub.', 'Could not load Integration Hub.')
  } finally {
    loadingIntegrations.value = false
  }
}

const loadInbox = async () => {
  loadingInbox.value = true
  inboxError.value = ''
  try {
    const response = await axiosClient.get('/inbox')
    inboxItems.value = asArray(getPayload(response))
    selectedItemId.value = selectedItemId.value || inboxItems.value[0]?.id || ''
  } catch (error) {
    inboxError.value = error.response?.data?.message || t('Không tải được Unified Inbox.', 'Could not load Unified Inbox.')
  } finally {
    loadingInbox.value = false
  }
}

const connectGoogleCalendar = async () => {
  connecting.value = true
  notice.value = null
  try {
    const response = await axiosClient.get('/integrations/google-calendar/connect')
    const payload = getPayload(response)
    const authorizationUrl = payload?.authorizationUrl || payload?.authUrl
    if (!authorizationUrl) {
      throw new Error(t('Backend không trả URL Google OAuth.', 'Backend did not return a Google OAuth URL.'))
    }
    window.location.href = authorizationUrl
  } catch (error) {
    notice.value = { type: 'error', message: error.response?.data?.message || error.message }
  } finally {
    connecting.value = false
  }
}

const completeGoogleOAuth = async () => {
  if (route.query.connected === 'success' && route.query.provider === 'google-calendar') {
    notice.value = { type: 'success', message: t('Đã kết nối Google Calendar thành công.', 'Google Calendar connected successfully.') }
    ElMessage.success(notice.value.message)
    await loadIntegrations()
    router.replace('/integrations').catch(() => {})
    return
  }

  if (route.query.connected === 'error' && route.query.provider === 'google-calendar') {
    const message = route.query.message || t('Không kết nối được Google Calendar.', 'Could not connect Google Calendar.')
    notice.value = { type: 'error', message }
    ElMessage.error(message)
    await loadIntegrations()
    router.replace('/integrations').catch(() => {})
    return
  }

  const code = route.query.code
  const state = route.query.state
  if (!code || !state) return

  try {
    await axiosClient.get('/integrations/google-calendar/callback', { params: { code, state } })
    notice.value = { type: 'success', message: t('Đã kết nối Google Calendar thành công.', 'Google Calendar connected successfully.') }
    ElMessage.success(notice.value.message)
    await loadIntegrations()
    router.replace({ path: '/integrations', query: { provider: 'google-calendar', connected: 'success' } }).catch(() => {})
  } catch (error) {
    notice.value = { type: 'error', message: error.response?.data?.message || t('Không hoàn tất Google OAuth.', 'Could not complete Google OAuth.') }
    router.replace('/integrations').catch(() => {})
  }
}

const syncGoogleCalendar = async () => {
  if (!isGoogleConnected.value) {
    notice.value = { type: 'error', message: t('Bạn cần kết nối Google Calendar trước.', 'Connect Google Calendar first.') }
    return
  }

  syncing.value = true
  try {
    const response = await axiosClient.post('/integrations/google-calendar/sync')
    const imported = getPayload(response)?.imported ?? 0
    notice.value = {
      type: 'success',
      message: t(`Đã đồng bộ ${imported} sự kiện thật từ Google Calendar.`, `Synced ${imported} real Google Calendar events.`)
    }
    ElMessage.success(notice.value.message)
    await Promise.all([loadIntegrations(), loadInbox()])
  } catch (error) {
    notice.value = { type: 'error', message: error.response?.data?.message || t('Không đồng bộ được Google Calendar.', 'Could not sync Google Calendar.') }
  } finally {
    syncing.value = false
  }
}

const disconnect = async (id) => {
  if (!id) return
  try {
    await axiosClient.delete(`/integrations/${id}`)
    notice.value = { type: 'success', message: t('Đã ngắt kết nối Google Calendar.', 'Google Calendar disconnected.') }
    ElMessage.success(notice.value.message)
    await Promise.all([loadIntegrations(), loadInbox()])
  } catch (error) {
    notice.value = { type: 'error', message: error.response?.data?.message || t('Không ngắt kết nối được.', 'Could not disconnect.') }
  }
}

const selectItem = async (item) => {
  selectedItemId.value = item.id
  if (!item.isRead) {
    item.isRead = true
    try {
      await axiosClient.patch(`/inbox/${item.id}/read`)
    } catch {
      item.isRead = false
    }
  }
}

const createTask = async (item) => {
  creatingTask.value = true
  try {
    const projectId = getScopedCurrentProjectId()
    const body = projectId ? { projectId } : {}
    const response = await axiosClient.post(`/inbox/${item.id}/create-task`, body)
    item.createdTaskId = getPayload(response)?.id || item.createdTaskId
    ElMessage.success(t('Đã tạo task thật từ mục inbox.', 'Created a real task from the inbox item.'))
    await loadInbox()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('Không tạo được task.', 'Could not create task.'))
  } finally {
    creatingTask.value = false
  }
}

const sourceCount = (source) => source === 'all'
  ? inboxItems.value.length
  : inboxItems.value.filter(item => item.source === source).length

const providerIcon = (provider) => ({
  'google-calendar': 'fa-brands fa-google',
  gmail: 'fa-regular fa-envelope',
  slack: 'fa-brands fa-slack'
}[provider] || 'fa-solid fa-plug')

const sourceIcon = (source) => ({
  calendar: 'fa-regular fa-calendar',
  email: 'fa-regular fa-envelope',
  slack: 'fa-brands fa-slack',
  system: 'fa-regular fa-bell'
}[source] || 'fa-solid fa-inbox')

const sourceLabel = (source) => ({
  calendar: t('Lịch', 'Calendar'),
  email: 'Email',
  slack: 'Slack',
  system: t('Hệ thống', 'System')
}[source] || source)

const providerStatus = (provider) => {
  if (provider.status === 'connected') return t('Đã kết nối', 'Connected')
  if (provider.status === 'coming_soon') return t('Sẽ hỗ trợ sau', 'Coming soon')
  return t('Chưa kết nối', 'Not connected')
}

const providerDescription = (provider) => {
  if (provider.provider === 'google-calendar') {
    return t(
      'Kết nối Google Calendar để đồng bộ cuộc họp, deadline và lịch sprint vào Unified Inbox.',
      'Connect Google Calendar to sync meetings, deadlines, and sprint schedules into Unified Inbox.'
    )
  }
  if (provider.provider === 'gmail') {
    return t('Email sẽ được hỗ trợ ở giai đoạn sau. MVP này không tạo dữ liệu Gmail giả.', 'Email support is coming later. This MVP does not create fake Gmail data.')
  }
  return t('Slack sẽ được hỗ trợ ở giai đoạn sau. MVP này không tạo dữ liệu Slack giả.', 'Slack support is coming later. This MVP does not create fake Slack data.')
}

const syncStatusLabel = (status) => {
  if (status === 'success') return t('Thành công', 'Success')
  if (status === 'error') return t('Thất bại', 'Failed')
  return status || t('Đang chạy', 'Running')
}

const previewText = (value) => {
  const text = value || t('Không có mô tả từ provider.', 'No provider description.')
  return text.length > 130 ? `${text.slice(0, 130)}...` : text
}

const formatDate = (value) => {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  return date.toLocaleString(i18nStore.locale === 'en' ? 'en-US' : 'vi-VN', {
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFullDate = (value) => {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  return date.toLocaleString(i18nStore.locale === 'en' ? 'en-US' : 'vi-VN', {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(async () => {
  await Promise.all([loadIntegrations(), loadInbox()])
  await completeGoogleOAuth()
})
</script>

<style scoped>
.integration-page {
  min-height: calc(100vh - 64px);
  padding: 28px;
  background:
    radial-gradient(circle at 8% -8%, color-mix(in srgb, var(--color-accent) 22%, transparent), transparent 36rem),
    radial-gradient(circle at 92% 2%, color-mix(in srgb, #22d3ee 16%, transparent), transparent 32rem),
    linear-gradient(180deg, color-mix(in srgb, var(--color-bg) 72%, var(--color-surface)), var(--color-bg));
  color: var(--color-text-primary);
}

.hero-shell,
.hero-action-card,
.stats-grid,
.workspace-grid,
.panel-head,
.provider-card,
.provider-top,
.provider-meta,
.provider-actions,
.tabs,
.inbox-item,
.item-title,
.item-tags,
.detail-head,
.history-row,
.notice,
.hero-actions,
.section-title {
  display: flex;
}

.hero-shell {
  align-items: stretch;
  justify-content: space-between;
  gap: 18px;
  max-width: 1580px;
  margin: 0 auto 16px;
}

.hero-copy {
  flex: 1;
  min-width: 0;
  padding: 8px 0;
}

.kicker {
  margin: 0 0 7px;
  color: var(--color-accent);
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

h1,
h2,
h3,
p {
  margin-top: 0;
}

.hero-copy h1 {
  margin-bottom: 8px;
  font-size: clamp(28px, 3.2vw, 46px);
  line-height: 1.06;
  text-wrap: balance;
}

.hero-subtitle {
  max-width: 780px;
  margin-bottom: 0;
  color: var(--color-text-secondary);
  font-size: 15px;
  line-height: 1.55;
}

.hero-action-card {
  width: min(390px, 100%);
  flex-direction: column;
  justify-content: space-between;
  gap: 10px;
  padding: 18px;
  border: 1px solid color-mix(in srgb, var(--color-accent) 22%, var(--color-border));
  border-radius: 18px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-accent) 10%, var(--color-surface)), var(--color-surface)),
    var(--color-surface);
  box-shadow:
    0 22px 60px color-mix(in srgb, var(--color-accent) 16%, transparent),
    inset 0 1px 0 rgba(255,255,255,0.16);
}

.hero-action-card.connected {
  border-color: color-mix(in srgb, var(--color-success) 28%, var(--color-border));
}

.hero-action-card strong {
  font-size: 18px;
}

.hero-action-card p {
  margin-bottom: 4px;
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.hero-status {
  width: max-content;
  padding: 5px 9px;
  border-radius: 8px;
  background: var(--sa-primary-soft);
  color: var(--color-accent);
  font-size: 12px;
  font-weight: 900;
}

.primary,
.ghost,
.icon-btn,
.tabs button {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  min-height: 36px;
  font-weight: 850;
  cursor: pointer;
  transition: transform .22s cubic-bezier(.2,.8,.2,1), border-color .22s, background .22s, box-shadow .22s;
}

.primary,
.ghost {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 12px;
}

.primary {
  background: var(--color-accent);
  border-color: var(--color-accent);
  color: #fff;
  box-shadow: 0 10px 24px color-mix(in srgb, var(--color-accent) 22%, transparent);
}

.primary:hover:not(:disabled),
.ghost:hover:not(:disabled),
.icon-btn:hover:not(:disabled),
.tabs button:hover:not(:disabled) {
  transform: translateY(-1px);
}

.primary:active:not(:disabled),
.ghost:active:not(:disabled),
.icon-btn:active:not(:disabled),
.tabs button:active:not(:disabled) {
  transform: translateY(0) scale(.98);
}

.ghost,
.icon-btn {
  background: var(--color-surface);
  color: var(--color-text-primary);
}

.small {
  min-height: 32px;
  padding: 6px 10px;
  font-size: 12px;
}

.cta {
  min-width: 210px;
}

.full {
  width: 100%;
}

button:disabled {
  opacity: .62;
  cursor: not-allowed;
  box-shadow: none;
}

.icon-btn {
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
}

.danger-text {
  color: var(--color-danger);
}

.notice {
  max-width: 1580px;
  align-items: center;
  gap: 10px;
  margin: 0 auto 14px;
  padding: 11px 12px;
  border-radius: 10px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
}

.notice.success {
  border-color: color-mix(in srgb, var(--color-success) 35%, var(--color-border));
  color: var(--color-success);
}

.notice.error {
  border-color: color-mix(in srgb, var(--color-danger) 35%, var(--color-border));
  color: var(--color-danger);
}

.notice button {
  margin-left: auto;
  border: 0;
  background: transparent;
  color: inherit;
  cursor: pointer;
}

.stats-grid {
  max-width: 1580px;
  margin: 0 auto 16px;
  gap: 12px;
}

.stats-grid article {
  flex: 1;
  min-width: 0;
  padding: 15px 16px;
  border: 1px solid var(--color-border);
  border-radius: 16px;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 96%, #fff), color-mix(in srgb, var(--color-surface-hover) 42%, var(--color-surface)));
  box-shadow: 0 14px 38px color-mix(in srgb, #020617 8%, transparent);
}

.stats-grid span,
.detail-list dt {
  display: block;
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 850;
}

.stats-grid strong {
  display: block;
  margin-top: 8px;
  font-variant-numeric: tabular-nums;
  font-size: 24px;
}

.workspace-grid {
  max-width: 1580px;
  margin: 0 auto;
  align-items: stretch;
  gap: 14px;
  min-height: calc(100vh - 302px);
}

.panel {
  min-width: 0;
  border: 1px solid color-mix(in srgb, var(--color-border) 86%, transparent);
  border-radius: 18px;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 94%, transparent), color-mix(in srgb, var(--color-surface-hover) 36%, var(--color-surface)));
  box-shadow:
    0 20px 58px color-mix(in srgb, #020617 10%, transparent),
    inset 0 1px 0 rgba(255,255,255,0.12);
  overflow: hidden;
}

.apps-panel {
  width: 430px;
  padding: 16px;
  overflow-y: auto;
}

.inbox-panel {
  flex: 1;
}

.detail-panel {
  width: 410px;
  padding: 16px;
}

.panel-head {
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.inbox-title {
  margin: 0;
  padding: 16px;
  border-bottom: 1px solid var(--color-border);
}

.panel-head h2,
.detail-head h2 {
  margin-bottom: 5px;
  font-size: 18px;
}

.panel-head p,
.provider-body p,
.empty-state p,
.detail-head p,
.content-box p,
.history-row span,
.history-row small {
  color: var(--color-text-secondary);
}

.provider-list {
  display: grid;
  gap: 11px;
}

.provider-card {
  align-items: flex-start;
  gap: 12px;
  flex-wrap: wrap;
  padding: 14px;
  border: 1px solid color-mix(in srgb, var(--color-border) 84%, transparent);
  border-radius: 16px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-surface-hover) 78%, var(--color-surface)), var(--color-surface));
  box-shadow: inset 0 1px 0 rgba(255,255,255,0.10);
}

.provider-card.featured {
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-accent) 7%, var(--color-surface-hover)), var(--color-surface-hover));
}

.provider-card.connected {
  border-color: color-mix(in srgb, var(--color-success) 34%, var(--color-border));
}

.provider-card.coming_soon {
  opacity: .78;
}

.provider-icon,
.source-icon {
  width: 38px;
  height: 38px;
  flex: 0 0 38px;
  display: grid;
  place-items: center;
  border-radius: 10px;
}

.calendar {
  background: rgba(16, 185, 129, .14);
  color: #059669;
}

.email {
  background: rgba(14, 165, 233, .14);
  color: #0284c7;
}

.slack {
  background: rgba(126, 87, 194, .14);
  color: #7e57c2;
}

.system {
  background: rgba(245, 158, 11, .14);
  color: #d97706;
}

.provider-body,
.item-main {
  flex: 1;
  min-width: 0;
}

.provider-body {
  min-width: min(250px, calc(100% - 54px));
}

.provider-top {
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.provider-body p {
  margin: 7px 0;
  font-size: 12px;
  line-height: 1.5;
}

.provider-meta {
  gap: 8px;
  flex-wrap: wrap;
}

.provider-meta span {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  color: var(--color-text-muted);
  font-size: 11px;
}

.provider-actions {
  width: 100%;
  flex: 1 0 100%;
  justify-content: flex-start;
  gap: 7px;
  flex-wrap: wrap;
  padding-left: 50px;
}

.provider-actions .primary,
.provider-actions .ghost {
  white-space: nowrap;
  width: auto;
  min-width: max-content;
}

.provider-actions .primary {
  flex: 1 1 148px;
}

.status-badge,
.item-tags span,
.section-title span {
  border-radius: 8px;
  padding: 3px 8px;
  font-size: 11px;
  font-weight: 900;
}

.status-badge.connected,
.item-tags .linked {
  color: var(--color-success);
  background: var(--color-success-bg);
}

.status-badge.not_connected {
  color: var(--color-danger);
  background: var(--color-danger-bg);
}

.status-badge.coming_soon {
  color: var(--color-warning);
  background: var(--color-warning-bg);
}

.sync-history {
  margin-top: 18px;
}

.section-title {
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.section-title h3 {
  margin: 0;
  font-size: 14px;
}

.section-title span {
  background: var(--color-surface-hover);
  color: var(--color-text-secondary);
}

.history-empty,
.inline-state {
  display: grid;
  place-items: center;
  gap: 8px;
  min-height: 116px;
  padding: 18px;
  border: 1px dashed var(--color-border);
  border-radius: 12px;
  background: var(--color-surface-hover);
  text-align: center;
  color: var(--color-text-secondary);
}

.inline-state.error {
  color: var(--color-danger);
}

.history-list {
  display: grid;
  gap: 8px;
}

.history-row {
  align-items: flex-start;
  gap: 10px;
  padding: 10px;
  border-radius: 10px;
  background: var(--color-surface-hover);
}

.history-row i {
  margin-top: 2px;
  color: var(--color-accent);
}

.history-row.error i {
  color: var(--color-danger);
}

.history-row strong,
.history-row span,
.history-row small {
  display: block;
  font-size: 12px;
}

.tabs {
  gap: 8px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
  flex-wrap: wrap;
}

.tabs button {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 7px 10px;
  background: var(--color-surface);
  color: var(--color-text-secondary);
}

.tabs button.active {
  color: var(--color-accent);
  border-color: color-mix(in srgb, var(--color-accent) 35%, var(--color-border));
  background: var(--sa-primary-soft);
}

.tabs span {
  min-width: 19px;
  padding: 1px 5px;
  border-radius: 999px;
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
  font-size: 11px;
}

.inbox-list {
  height: calc(100vh - 376px);
  overflow-y: auto;
}

.inbox-item {
  width: 100%;
  align-items: flex-start;
  gap: 12px;
  padding: 15px 16px;
  border: 0;
  border-bottom: 1px solid var(--color-border);
  background: transparent;
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.inbox-item:hover,
.inbox-item.selected {
  background:
    linear-gradient(90deg, color-mix(in srgb, var(--color-accent) 8%, var(--color-surface-hover)), var(--color-surface-hover));
}

.inbox-item.selected {
  box-shadow: inset 3px 0 0 var(--color-accent);
}

.item-title {
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.item-title strong {
  font-size: 14px;
  line-height: 1.35;
}

.item-title small {
  flex: 0 0 auto;
  color: var(--color-text-muted);
  font-size: 11px;
}

.item-copy {
  display: block;
  margin: 6px 0 9px;
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.45;
}

.item-tags {
  gap: 6px;
  flex-wrap: wrap;
}

.item-tags span {
  color: var(--color-text-secondary);
  background: var(--color-surface);
}

.item-tags .unread-label {
  color: var(--color-accent);
  background: var(--sa-primary-soft);
}

.unread-dot {
  width: 8px;
  height: 8px;
  margin-top: 12px;
  border-radius: 999px;
  background: var(--color-accent);
}

.empty-state {
  min-height: 330px;
  display: grid;
  place-items: center;
  align-content: center;
  gap: 10px;
  margin: 16px;
  padding: 28px;
  border: 1px dashed color-mix(in srgb, var(--color-accent) 28%, var(--color-border));
  border-radius: 14px;
  background:
    radial-gradient(circle at top, color-mix(in srgb, var(--color-accent) 8%, transparent), transparent 17rem),
    var(--color-surface-hover);
  text-align: center;
}

.empty-state > i {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: var(--sa-primary-soft);
  color: var(--color-accent);
}

.empty-state strong {
  font-size: 16px;
}

.empty-state p {
  max-width: 480px;
  margin-bottom: 4px;
  line-height: 1.55;
}

.empty-state.error {
  border-color: color-mix(in srgb, var(--color-danger) 30%, var(--color-border));
}

.empty-state.error > i {
  color: var(--color-danger);
  background: var(--color-danger-bg);
}

.detail-head {
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.detail-head h2 {
  line-height: 1.35;
}

.source-icon.large {
  width: 46px;
  height: 46px;
  flex-basis: 46px;
  font-size: 18px;
}

.detail-list,
.content-box {
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-surface-hover);
}

.detail-list {
  display: grid;
  gap: 10px;
  margin: 0 0 14px;
  padding: 14px;
}

.detail-list dd {
  margin: 3px 0 0;
  word-break: break-word;
}

.content-box {
  margin-bottom: 14px;
  padding: 14px;
}

.content-box h3 {
  margin-bottom: 8px;
  font-size: 13px;
}

.content-box p {
  margin: 0;
  line-height: 1.55;
}

.detail-empty {
  min-height: 460px;
  margin: 0;
}

.skeleton-card,
.inbox-skeleton {
  pointer-events: none;
}

.skeleton-card span,
.inbox-skeleton span,
.skeleton-card b,
.skeleton-card small,
.inbox-skeleton b,
.inbox-skeleton small {
  display: block;
  border-radius: 10px;
  background: linear-gradient(90deg, var(--color-surface-hover), color-mix(in srgb, var(--color-border) 55%, var(--color-surface)), var(--color-surface-hover));
  background-size: 200% 100%;
  animation: shimmer 1.1s infinite linear;
}

.skeleton-card > span,
.inbox-skeleton > span {
  width: 38px;
  height: 38px;
  flex: 0 0 38px;
}

.skeleton-card div,
.inbox-skeleton div {
  flex: 1;
}

.skeleton-card b,
.inbox-skeleton b {
  width: 46%;
  height: 14px;
  margin-bottom: 10px;
}

.skeleton-card small,
.inbox-skeleton small {
  width: 82%;
  height: 10px;
  margin-top: 7px;
}

.inbox-skeleton {
  display: flex;
  gap: 12px;
  padding: 15px 16px;
  border-bottom: 1px solid var(--color-border);
}

@keyframes shimmer {
  to { background-position: -200% 0; }
}

@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    transition-duration: .01ms !important;
    animation-duration: .01ms !important;
    animation-iteration-count: 1 !important;
  }
}

@media (max-width: 1280px) {
  .workspace-grid {
    grid-template-columns: 1fr;
    flex-direction: column;
  }

  .apps-panel,
  .detail-panel {
    width: 100%;
  }

  .inbox-list {
    height: auto;
    max-height: 620px;
  }
}

@media (max-width: 820px) {
  .integration-page {
    padding: 16px;
  }

  .hero-shell,
  .stats-grid,
  .panel-head,
  .provider-card,
  .item-title {
    flex-direction: column;
  }

  .hero-action-card {
    width: 100%;
  }

  .stats-grid article {
    width: 100%;
  }

  .provider-actions,
  .hero-actions {
    justify-content: flex-start;
    padding-left: 0;
  }

  .primary,
  .ghost {
    width: 100%;
    min-width: 0;
  }
}
</style>
