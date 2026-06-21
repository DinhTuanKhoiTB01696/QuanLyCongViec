<template>
  <HomeSiteLayout>
    <div class="jira-for-you-page">
      <!-- Top banner -->
      <div class="welcome-banner">
        <div class="banner-content">
          <div class="date-text">{{ currentDate }}</div>
          <h1 class="welcome-text">{{ t('Welcome') }} {{ userName }}</h1>
        </div>
      </div>

      <div class="content-container">
        <!-- Ứng dụng của bạn -->
        <section class="dashboard-section">
          <div class="section-header">
            <h2>{{ t('Your apps') }}</h2>
            <a href="#" class="view-all-link">{{ t('View all apps') }} &rarr;</a>
          </div>
          <div class="apps-container">
            <div class="app-card" v-for="site in filteredSites.slice(0, 4)" :key="site.id" @click="goToSite(site.id)">
              <div class="app-icon">
                <div class="jira-icon-wrapper">
                  <i class="fa-brands fa-jira"></i>
                </div>
              </div>
              <div class="app-info">
                <div class="app-name">SprintA</div>
                <div class="app-url">{{ site.name }}</div>
              </div>
            </div>
            <div class="app-card create-new" @click="openCreateModal">
              <div class="create-icon"><i class="fa-solid fa-plus"></i></div>
              <div class="app-info">
                <div class="app-name">{{ t('Create new') }}</div>
              </div>
            </div>
          </div>
        </section>

        <!-- Thường xuyên truy cập -->
        <section class="dashboard-section">
          <div class="section-header">
            <h2>{{ t('Frequently accessed') }}</h2>
          </div>
          <div class="recent-access-container">
            <!-- Mock recent access -->
            <div class="recent-access-card" v-for="i in 1" :key="i">
              <div class="recent-icon purple">
                <i class="fa-solid fa-rotate"></i>
              </div>
              <div class="recent-info">
                <div class="recent-title">(Example) Billing System...</div>
                <div class="recent-subtitle">Không gian • SprintA</div>
              </div>
            </div>
          </div>
        </section>

        <!-- Tiếp theo là gì -->
        <section class="dashboard-section">
          <div class="section-header space-between">
            <h2>{{ t('What\'s next') }}</h2>
            <div class="tabs">
              <button class="tab-btn active">{{ t('Worked on') }}</button>
              <button class="tab-btn">{{ t('Viewed') }}</button>
            </div>
          </div>

          <div class="audit-list">
            <div class="time-group">
              <h3 class="time-label">{{ t('Today') }}</h3>
              <div class="audit-item">
                <div class="item-icon yellow-bg">😎</div>
                <div class="item-details">
                  <div class="item-title">e</div>
                  <div class="item-path">Dự án • Dự án • TUA46-8</div>
                </div>
                <div class="item-meta">
                  <span class="status-badge pending">PENDING</span>
                  <span class="time-ago">8 phút trước</span>
                </div>
              </div>
            </div>

            <div class="time-group">
              <h3 class="time-label">{{ t('This week') }}</h3>
              <div class="audit-item">
                <div class="item-icon light-blue"><i class="fa-solid fa-file-lines"></i></div>
                <div class="item-details">
                  <div class="item-title">Chưa có tiêu đề</div>
                  <div class="item-path">Content • Confluence • Dự Án Tốt Nghiệp</div>
                </div>
                <div class="item-meta">
                  <span class="status-badge draft">BẢN NHÁP</span>
                  <span class="time-ago">2 ngày trước</span>
                </div>
              </div>
              <div class="audit-item">
                <div class="item-icon light-blue"><i class="fa-solid fa-file-lines"></i></div>
                <div class="item-details">
                  <div class="item-title">Dự Án Tốt Nghiệp Home</div>
                  <div class="item-path">Trang • Confluence • Dự Án Tốt Nghiệp</div>
                </div>
                <div class="item-meta">
                  <span class="time-ago">2 ngày trước</span>
                </div>
              </div>
              <div class="audit-item">
                <div class="item-icon light-blue square"><i class="fa-solid fa-check"></i></div>
                <div class="item-details">
                  <div class="item-title">fw</div>
                  <div class="item-path">Task • SprintA • DTN-12 • Dự Án Tốt Nghiệp</div>
                </div>
                <div class="item-meta">
                  <span class="status-badge todo">TO DO</span>
                  <span class="time-ago">3 ngày trước</span>
                </div>
              </div>
            </div>

            <button class="view-all-btn" @click="router.push('/home/recent')">{{ t('View all') }}</button>
          </div>
        </section>
      </div>

      <!-- Create Site Modal -->
      <div class="modal-overlay" v-if="isCreateModalVisible" @click.self="isCreateModalVisible = false">
        <div class="modal-dialog">
          <div class="modal-header">
            <h2>{{ t('Create a new site') }}</h2>
            <button class="close-btn" @click="isCreateModalVisible = false"><i class="fa-solid fa-xmark"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>{{ t('Site Name') }} <span class="required">*</span></label>
              <input type="text" v-model="newSiteName" :placeholder="t('e.g. My Awesome Team')" class="text-input" :class="{ 'error': errorMessage }" />
              <div v-if="errorMessage" class="error-message">
                <i class="fa-solid fa-triangle-exclamation"></i> {{ errorMessage }}
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="secondary-btn" @click="isCreateModalVisible = false">{{ t('Cancel') }}</button>
            <button class="primary-btn" :disabled="isCreating || !newSiteName.trim()" @click="submitCreateSite">
              {{ isCreating ? t('Creating...') : t('Create') }}
            </button>
          </div>
        </div>
      </div>

    </div>
  </HomeSiteLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import HomeSiteLayout from '@/views/HomeSite/HomeSiteLayout.vue'
import { useSiteStore } from '@/store/useSiteStore'
import { useI18nStore } from '@/store/useI18nStore'
import { getStoredUser } from '@/utils/permissions'

const router = useRouter()
const siteStore = useSiteStore()
const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

const currentUser = getStoredUser()
const userName = currentUser?.username || 'Tua20000'

// Format current date in Vietnamese
const currentDate = computed(() => {
  return new Intl.DateTimeFormat('vi-VN', {
    weekday: 'long',
    day: 'numeric',
    month: 'long'
  }).format(new Date())
})

const loading = ref(false)

const isCreateModalVisible = ref(false)
const newSiteName = ref('')
const isCreating = ref(false)
const errorMessage = ref('')

const loadSites = async () => {
  loading.value = true
  try {
    await siteStore.fetchSites()
  } catch (error) {
    console.error('Fetch sites error:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSites()
})

const filteredSites = computed(() => {
  return siteStore.sites
})

const goToSite = (id) => {
  siteStore.setRecentSite(siteStore.sites.find(s => s.id === id) || { id })
  router.push(`/space/${id}`)
}

const openCreateModal = () => {
  isCreateModalVisible.value = true
  newSiteName.value = ''
  errorMessage.value = ''
}

const submitCreateSite = async () => {
  if (!newSiteName.value.trim()) {
    errorMessage.value = 'Site name is required'
    return
  }
  isCreating.value = true
  errorMessage.value = ''
  try {
    const site = await siteStore.createSite({ name: newSiteName.value })
    isCreateModalVisible.value = false
    goToSite(site.id)
  } catch (error) {
    errorMessage.value = error.message || 'Failed to create site'
  } finally {
    isCreating.value = false
  }
}
</script>

<style scoped>
.jira-for-you-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #172B4D;
  background-color: #FFFFFF;
  min-height: calc(100vh - 56px);
  padding: 0;
  display: flex;
  flex-direction: column;
}

/* Welcome Banner */
.welcome-banner {
  background-color: #FFC400;
  background-image: url('data:image/svg+xml;utf8,<svg width="100%" height="100%" xmlns="http://www.w3.org/2000/svg"><defs><pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse"><path d="M 40 0 L 0 0 0 40" fill="none" stroke="rgba(0,0,0,0.05)" stroke-width="1"/></pattern></defs><rect width="100%" height="100%" fill="url(%23grid)"/><path d="M 600 120 L 700 40 L 800 100 L 900 20" stroke="%23172B4D" stroke-width="3" fill="none" /><circle cx="900" cy="20" r="4" fill="%23172B4D" /></svg>');
  background-position: right center;
  background-repeat: no-repeat;
  padding: 32px 40px;
  min-height: 120px;
  display: flex;
  align-items: center;
  border-radius: 4px;
  margin: 24px 40px;
  position: relative;
  overflow: hidden;
}

.banner-content {
  position: relative;
  z-index: 2;
}

.date-text {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
  margin-bottom: 4px;
  text-transform: capitalize;
}

.welcome-text {
  font-size: 24px;
  font-weight: 600;
  color: #172B4D;
  margin: 0;
}

.content-container {
  padding: 0 40px 40px;
  max-width: 1000px;
}

.dashboard-section {
  margin-bottom: 40px;
}

.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.section-header.space-between {
  justify-content: space-between;
}

.section-header h2 {
  font-size: 16px;
  font-weight: 600;
  color: #172B4D;
  margin: 0;
  margin-right: auto;
}

.view-all-link {
  font-size: 13px;
  color: #5E6C84;
  text-decoration: none;
}

.view-all-link:hover {
  text-decoration: underline;
}

/* Apps Container */
.apps-container {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}

.app-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border: 1px solid #DFE1E6;
  border-radius: 4px;
  background: #FFFFFF;
  cursor: pointer;
  transition: box-shadow 0.2s, background-color 0.2s;
  min-width: 220px;
}

.app-card:hover {
  background-color: #FAFBFC;
  box-shadow: 0 1px 2px rgba(9, 30, 66, 0.25);
}

.app-icon .jira-icon-wrapper {
  width: 24px;
  height: 24px;
  background-color: #0052CC;
  color: white;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.create-new {
  border-style: dashed;
  color: #0052CC;
}

.create-new .app-name {
  color: #0052CC;
}

.create-icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.app-name {
  font-size: 12px;
  color: #172B4D;
  font-weight: 500;
}

.app-url {
  font-size: 11px;
  color: #5E6C84;
}

/* Recent Access */
.recent-access-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border: 1px solid #DFE1E6;
  border-radius: 4px;
  background: #FFFFFF;
  max-width: 300px;
}

.recent-icon.purple {
  width: 32px;
  height: 32px;
  background-color: #EAE6FF;
  color: #403294;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.recent-title {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
}

.recent-subtitle {
  font-size: 12px;
  color: #5E6C84;
}

/* Audit List Tabs */
.tabs {
  display: flex;
  background: #F4F5F7;
  border-radius: 3px;
  padding: 2px;
}

.tab-btn {
  background: transparent;
  border: none;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #5E6C84;
  border-radius: 3px;
  cursor: pointer;
}

.tab-btn.active {
  background: #FFFFFF;
  color: #172B4D;
  box-shadow: 0 1px 1px rgba(9, 30, 66, 0.25);
}

/* Audit List */
.audit-list {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.time-group {
  display: flex;
  flex-direction: column;
}

.time-label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  margin: 0 0 8px 0;
}

.audit-item {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #DFE1E6;
  gap: 16px;
}

.audit-item:last-child {
  border-bottom: none;
}

.item-icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  font-size: 12px;
}

.item-icon.square {
  border-radius: 3px;
}

.yellow-bg {
  background: #FFFAE6;
  font-size: 16px;
}

.light-blue {
  background: #E6FCFF;
  color: #00B8D9;
}

.item-details {
  flex: 1;
}

.item-title {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
  margin-bottom: 2px;
}

.item-path {
  font-size: 12px;
  color: #5E6C84;
}

.item-meta {
  display: flex;
  align-items: center;
  gap: 16px;
}

.status-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 3px;
}

.status-badge.pending {
  background: #DFE1E6;
  color: #42526E;
}

.status-badge.draft {
  background: #DFE1E6;
  color: #42526E;
}

.status-badge.todo {
  background: #DFE1E6;
  color: #42526E;
}

.time-ago {
  font-size: 12px;
  color: #5E6C84;
  min-width: 80px;
  text-align: right;
}

.view-all-btn {
  background: transparent;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 8px 16px;
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
  cursor: pointer;
  align-self: flex-start;
  transition: background-color 0.2s;
}

.view-all-btn:hover {
  background: #F4F5F7;
}

/* Modal styles preserved from original */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(9, 30, 66, 0.54);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.modal-dialog {
  background-color: #FFFFFF;
  border-radius: 3px;
  width: 400px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25), 0 0 1px rgba(9, 30, 66, 0.31);
}
.modal-header {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #DFE1E6;
}
.modal-header h2 { margin: 0; font-size: 20px; font-weight: 500; color: #172B4D; }
.close-btn { background: none; border: none; font-size: 16px; color: #6B778C; cursor: pointer; padding: 4px; border-radius: 3px; }
.close-btn:hover { background-color: rgba(9, 30, 66, 0.08); }
.modal-body { padding: 24px; }
.form-group label { display: block; font-size: 12px; font-weight: 600; color: #5E6C84; margin-bottom: 8px; }
.required { color: #DE350B; }
.text-input { width: 100%; padding: 8px 12px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; color: #091E42; box-sizing: border-box; outline: none; }
.text-input:focus { border-color: #4C9AFF; }
.error-message { color: #DE350B; font-size: 12px; margin-top: 8px; }
.modal-footer { padding: 16px 24px; display: flex; justify-content: flex-end; gap: 8px; border-top: 1px solid #DFE1E6; }
.primary-btn { background-color: #0052CC; color: white; border: none; padding: 6px 12px; border-radius: 3px; cursor: pointer; }
.primary-btn:hover { background-color: #0047B3; }
.primary-btn:disabled { background-color: #EBECF0; color: #A5ADBA; cursor: not-allowed; }
.secondary-btn { background: #F4F5F7; border: none; padding: 6px 12px; border-radius: 3px; cursor: pointer; }
.secondary-btn:hover { background: #EBECF0; }
</style>
