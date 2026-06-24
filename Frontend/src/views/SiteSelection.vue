<template>
  <div class="start-page-wrapper">
    <header class="start-header">
      <div class="header-left">
        <div class="atlassian-brand-block">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="white" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2L2 22H9L12 16L15 22H22L12 2Z"/>
          </svg>
        </div>
        <div class="app-logo">
          <img src="@/assets/logo_QLCV.png" alt="SprintA Logo" class="sprinta-logo-img" />
          <span class="logo-text">SprintA</span>
        </div>
      </div>
      <div class="header-right">
        <button class="pill-btn blue" @click="goToSpaceProject(recentSite?.id)" :disabled="!recentSite">{{ t('siteSelection.goToSprintA') }}</button>
        <div class="user-profile" v-if="userName">
          <div class="user-avatar-circle">{{ userInitials }}</div>
          <span class="user-name-text">{{ userName }}</span>
        </div>
      </div>
    </header>

    <main class="start-content">
      <div class="welcome-container">
        <h1 class="welcome-title">
          {{ t('siteSelection.welcomeBack') }}<template v-if="userName"> <span class="highlight-wrapper">{{ userName }}.
            <svg class="squiggly-line" width="100%" height="12" viewBox="0 0 100 12" preserveAspectRatio="none" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M0 6 Q 12 0, 25 6 T 50 6 T 75 6 T 100 6" stroke="#FFAB00" stroke-width="3" stroke-linecap="round" fill="none"/>
            </svg>
          </span></template>
        </h1>
      </div>

      <div class="card-section">
        <div class="card-header-row">
          <span class="pickup-text">
            {{ t('siteSelection.pickUpIn') }}
            <img src="@/assets/logo_QLCV.png" alt="SprintA Logo" class="sprinta-logo-img small" />
            <strong>SprintA</strong>
          </span>
          <a href="#" class="create-site-link" @click.prevent="openCreateModal">{{ t('siteSelection.createNewSite') }}</a>
        </div>

        <!-- Loading state -->
        <div v-if="siteStore.loading" class="state-box loading-box">
          <i class="fa-solid fa-circle-notch fa-spin"></i>
          <span>{{ t('siteSelection.loadingSites') }}</span>
        </div>

        <!-- Error state -->
        <div v-else-if="siteStore.error" class="state-box error-box">
          <i class="fa-solid fa-triangle-exclamation"></i>
          <span>{{ t('siteSelection.fetchError') }}</span>
          <button class="retry-btn" @click="siteStore.fetchSites()">Thử lại</button>
        </div>

        <!-- No sites -->
        <div v-else-if="!recentSite" class="state-box empty-box">
          <i class="fa-regular fa-folder-open"></i>
          <span>{{ t('siteSelection.noSites') }}</span>
        </div>

        <!-- Recent site card -->
        <div class="recent-site-card" v-else>
          <div class="site-card-left">
            <div class="site-avatar-square" :style="{ backgroundColor: recentSite.color || '#0052cc' }">
              {{ siteAvatarText }}
            </div>
            <div class="site-info-stack">
              <span class="site-name-bold">{{ recentSite.name }}</span>
              <div class="member-avatars">
                <!-- Chỉ hiển thị người dùng hiện tại. Chưa có API danh sách member → không thêm avatar/"+N" giả. -->
                <div v-if="userInitials" class="member-circle" style="background-color: #00875A">{{ userInitials }}</div>
              </div>
            </div>
          </div>
          <div class="site-card-right">
            <button class="pill-btn orange" @click="goToSpaceProject(recentSite.id)">{{ t('siteSelection.goToSprintA') }}</button>
          </div>
        </div>

        <div class="card-footer-row">
          <a href="#" class="different-site-link" @click.prevent="switchToJoinModal">{{ t('siteSelection.lookingForDifferent') }} &rarr;</a>

          <div class="decorative-stars">
            <svg width="60" height="60" viewBox="0 0 60 60" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M40 10 L43 20 L53 23 L43 26 L40 36 L37 26 L27 23 L37 20 Z" stroke="#FFAB00" stroke-width="2" stroke-linejoin="round"/>
              <path d="M15 35 L17 40 L22 42 L17 44 L15 49 L13 44 L8 42 L13 40 Z" stroke="#FFAB00" stroke-width="2" stroke-linejoin="round"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="explore-section">
        <p>{{ t('siteSelection.exploreQuestion') }}</p>
        <!-- Chưa có route/tài liệu thật cho tính năng này → disable rõ ràng, không để nút bấm im lặng. -->
        <button class="explore-btn" disabled :title="t('siteSelection.notSupportedTitle')">{{ t('siteSelection.exploreFeatures') }}</button>
      </div>
    </main>

    <!-- Create Site Modal -->
    <div class="modal-overlay" v-if="isCreateModalOpen" @click.self="closeCreateModal">
      <div class="jira-modal">
        <div class="jira-modal-body">
          <h1 class="jira-modal-title text-center">
            {{ t('siteSelection.welcomeBack') }}<template v-if="userName"> <span class="highlight-wrapper">{{ userName }}
              <svg class="squiggly-line" width="100%" height="12" viewBox="0 0 100 12" preserveAspectRatio="none" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M0 6 Q 12 0, 25 6 T 50 6 T 75 6 T 100 6" stroke="#FFAB00" stroke-width="3" stroke-linecap="round" fill="none"/>
              </svg>
            </span></template>
          </h1>
          <p class="jira-subtitle">{{ t('siteSelection.pickUpShort') }}</p>

          <div class="form-group">
            <label class="jira-label">{{ t('siteSelection.yourSite') }}</label>
            <div class="jira-input-wrapper" :class="validationState">
              <input
                type="text"
                v-model="newSiteName"
                class="jira-input"
              />
              <div class="jira-input-suffix">
                <span class="domain-text">.sprinta.vn</span>
                <i class="fa-solid fa-circle-notch fa-spin" v-if="validationState === 'checking'"></i>
                <i class="fa-solid fa-circle-check" v-else-if="validationState === 'success'"></i>
                <i class="fa-solid fa-triangle-exclamation" v-else-if="validationState === 'error'"></i>
              </div>
            </div>
            <div class="jira-error-text" v-if="validationState === 'error'">
              {{ errorMessage }}
            </div>
          </div>

          <button
            class="pill-btn blue full-width jira-continue-btn"
            :disabled="isCreating || validationState !== 'success'"
            @click="submitCreateSite"
          >
            {{ isCreating ? t('siteSelection.creating') : t('siteSelection.continueBtn') }}
          </button>

          <div class="jira-modal-footer">
            <span class="or-text">{{ t('siteSelection.or') }}</span><a href="#" class="join-link" @click.prevent="switchToJoinModal">{{ t('siteSelection.joinExisting') }}</a>
          </div>
        </div>
      </div>
    </div>

    <!-- Join / Pick Site Modal -->
    <div class="modal-overlay" v-if="isJoinModalOpen" @click.self="closeJoinModal">
      <div class="jira-modal join-modal">
        <div class="jira-modal-body">
          <h1 class="jira-modal-title text-center">
            {{ t('siteSelection.welcomeBack') }}<template v-if="userName"> <span class="highlight-wrapper">{{ userName }}
              <svg class="squiggly-line" width="100%" height="12" viewBox="0 0 100 12" preserveAspectRatio="none" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M0 6 Q 12 0, 25 6 T 50 6 T 75 6 T 100 6" stroke="#FFAB00" stroke-width="3" stroke-linecap="round" fill="none"/>
              </svg>
            </span></template>
          </h1>
          <p class="jira-subtitle">{{ t('siteSelection.pickUpShort') }}</p>

          <p class="logged-in-text" v-if="userEmail">
            {{ t('siteSelection.loggedInAs') }} <strong>{{ userEmail }}</strong>.
            <!-- switchAccount chưa có flow thật → disabled rõ ràng -->
            <button
              class="switch-account-btn"
              disabled
              :title="t('siteSelection.notSupportedTitle')"
            >{{ t('siteSelection.switchAccount') }}</button>
          </p>

          <div class="site-list-container">
            <div v-if="siteStore.loading" class="state-box loading-box">
              <i class="fa-solid fa-circle-notch fa-spin"></i>
              <span>{{ t('siteSelection.loadingSites') }}</span>
            </div>
            <div v-else-if="siteStore.error" class="state-box error-box">
              <i class="fa-solid fa-triangle-exclamation"></i>
              <span>{{ t('siteSelection.fetchError') }}</span>
            </div>
            <div v-else-if="!sites.length" class="state-box empty-box">
              <span>{{ t('siteSelection.noSites') }}</span>
            </div>
            <div class="site-list-item" v-for="site in sites" :key="site.id" v-else>
              <div class="site-list-item-left">
                <div class="site-list-item-title">SprintA</div>
                <div class="site-list-item-url">{{ site.name }}</div>
              </div>
              <div class="site-list-item-right">
                <button class="pill-btn blue small" @click="goToSpaceProject(site.id)">{{ t('siteSelection.goToSprintA') }}</button>
              </div>
            </div>
          </div>

          <div class="jira-modal-footer">
            <span class="or-text">{{ t('siteSelection.or') }}</span><a href="#" class="join-link" @click.prevent="switchToCreateModal">{{ t('siteSelection.startNewSite') }}</a>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSiteStore } from '@/store/useSiteStore'
import { getStoredUser } from '@/utils/permissions'
import { useI18n } from '@/composables/useI18n'

const router = useRouter()
const siteStore = useSiteStore()
const { t } = useI18n()

// ── User thật – không có fallback fake ──────────────────────────────────────
const currentUser = getStoredUser()
const userName = currentUser?.username || ''
const userEmail = currentUser?.email || ''
const userInitials = userName ? userName.substring(0, 1).toUpperCase() : ''

// ── Sites từ store (API thật) ───────────────────────────────────────────────
const recentSite = computed(() => siteStore.recentSite)
const sites = computed(() => siteStore.sites)

// Avatar text tính từ tên site thật (không có fallback cứng)
const siteAvatarText = computed(() => {
  if (!recentSite.value?.name) return '?'
  return recentSite.value.name.substring(0, 2).toUpperCase()
})

onMounted(async () => {
  await siteStore.fetchSites()
})

// ── Modal state ─────────────────────────────────────────────────────────────
const isCreateModalOpen = ref(false)
const isJoinModalOpen = ref(false)
const newSiteName = ref('')
const isCreating = ref(false)
const errorMessage = ref('')
const validationState = ref('idle') // idle, checking, success, error
let debounceTimer = null

watch(newSiteName, (newVal) => {
  if (!newVal) {
    validationState.value = 'idle'
    errorMessage.value = ''
    return
  }

  validationState.value = 'checking'
  const formattedName = newVal.toLowerCase().replace(/[^a-z0-9-]/g, '')

  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    if (formattedName.length < 3) {
      validationState.value = 'error'
      errorMessage.value = t('siteSelection.siteNameMinLength')
    } else {
      validationState.value = 'success'
      errorMessage.value = ''
    }
  }, 500)
})

const openCreateModal = () => {
  isCreateModalOpen.value = true
  const baseName = userEmail
    ? userEmail.split('@')[0]
    : userName.toLowerCase().replace(/[^a-z0-9]/g, '')
  const randomSuffix = Math.floor(1000 + Math.random() * 9000)
  newSiteName.value = baseName ? `${baseName}-${randomSuffix}` : ''
  validationState.value = 'idle'
  errorMessage.value = ''
}

const closeCreateModal = () => { isCreateModalOpen.value = false }
const closeJoinModal = () => { isJoinModalOpen.value = false }

const switchToJoinModal = () => {
  isCreateModalOpen.value = false
  isJoinModalOpen.value = true
}

const switchToCreateModal = () => {
  isJoinModalOpen.value = false
  openCreateModal()
}

const submitCreateSite = async () => {
  if (validationState.value !== 'success') return
  isCreating.value = true
  errorMessage.value = ''
  try {
    const site = await siteStore.createSite({ name: newSiteName.value })
    goToSpaceProject(site.id)
  } catch (error) {
    validationState.value = 'error'
    errorMessage.value = error.message || t('siteSelection.createSiteFailed')
  } finally {
    isCreating.value = false
  }
}

const goToSpaceProject = (siteId) => {
  if (!siteId) return
  siteStore.setRecentSite(siteStore.sites.find(s => s.id === siteId) || { id: siteId })
  router.push('/dashboard')
}
</script>

<style scoped>
.start-page-wrapper {
  min-height: 100vh;
  background-color: #f4f5f7;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  display: flex;
  flex-direction: column;
}

.start-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
  background-color: #ffffff;
  border-bottom: 1px solid #dfe1e6;
  padding-right: 24px;
}

.header-left {
  display: flex;
  align-items: center;
  height: 100%;
}

.atlassian-brand-block {
  width: 64px;
  height: 100%;
  background-color: #2684ff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.app-logo {
  display: flex;
  align-items: center;
  gap: 0;
  margin-left: 24px;
}

.sprinta-logo-img {
  height: 40px;
  width: auto;
  object-fit: contain;
  transform: scale(4);
  margin-right: 8px;
}

.sprinta-logo-img.small {
  height: 16px;
  width: auto;
  transform: scale(4);
  margin: 0 16px;
}

.app-logo .logo-text {
  font-size: 24px;
  font-weight: bold;
  color: #172b4d;
  letter-spacing: -0.5px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 24px;
}

.pill-btn {
  border: none;
  border-radius: 24px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
  font-family: inherit;
}

.pill-btn.blue {
  background-color: #0052cc;
  color: white;
  padding: 8px 16px;
}
.pill-btn.blue:hover:not(:disabled) { background-color: #0047b3; }

.pill-btn.orange {
  background-color: #ff991f;
  color: #172b4d;
  padding: 8px 24px;
}
.pill-btn.orange:hover:not(:disabled) { background-color: #e2851e; }

.user-profile {
  display: flex;
  align-items: center;
  gap: 8px;
  border-left: 1px solid #dfe1e6;
  padding-left: 24px;
}

.user-avatar-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: #00875a;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 14px;
}

.user-name-text {
  font-size: 14px;
  font-weight: 600;
  color: #172b4d;
}

.start-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 64px;
}

.welcome-container {
  margin-bottom: 48px;
}

.welcome-title {
  font-size: 40px;
  font-weight: 800;
  color: #091e42;
  margin: 0;
  letter-spacing: -1px;
}

.highlight-wrapper {
  position: relative;
  display: inline-block;
}

.squiggly-line {
  position: absolute;
  bottom: -4px;
  left: 0;
  width: 100%;
}

.card-section {
  width: 100%;
  max-width: 680px;
  position: relative;
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.pickup-text {
  font-size: 14px;
  color: #172b4d;
  display: flex;
  align-items: center;
}

.create-site-link {
  font-size: 14px;
  font-weight: 600;
  color: #0052cc;
  text-decoration: none;
}
.create-site-link:hover { text-decoration: underline; }

/* Loading / Error / Empty states */
.state-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 20px 24px;
  border-radius: 4px;
  font-size: 14px;
  color: #5e6c84;
  background: white;
  border: 1px solid #dfe1e6;
}
.loading-box { color: #0052cc; }
.error-box { color: #de350b; border-color: #ff8f73; }
.empty-box { color: #5e6c84; }
.retry-btn {
  margin-left: auto;
  background: none;
  border: 1px solid #de350b;
  border-radius: 4px;
  color: #de350b;
  font-size: 13px;
  padding: 4px 12px;
  cursor: pointer;
}
.retry-btn:hover { background: #fff4f4; }

.recent-site-card {
  background: white;
  border: 1px solid #091e4224;
  border-radius: 4px;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 1px #091e420f;
}

.site-card-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.site-avatar-square {
  width: 56px;
  height: 56px;
  border-radius: 4px;
  background-color: #0052cc;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: bold;
}

.site-info-stack {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.site-name-bold {
  font-size: 18px;
  font-weight: 700;
  color: #172b4d;
}

.member-avatars {
  display: flex;
  align-items: center;
}

.member-circle {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid white;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
  margin-left: -6px;
}
.member-circle:first-child { margin-left: 0; }

.card-footer-row {
  margin-top: 16px;
  position: relative;
}

.different-site-link {
  font-size: 12px;
  color: #6554c0;
  text-decoration: none;
}
.different-site-link:hover { text-decoration: underline; }

.decorative-stars {
  position: absolute;
  right: -60px;
  top: 0;
}

.explore-section {
  margin-top: 80px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.explore-section p {
  font-size: 16px;
  color: #172b4d;
  margin-bottom: 16px;
}

.explore-btn {
  background: transparent;
  border: 1px solid #172b4d;
  border-radius: 24px;
  padding: 8px 24px;
  font-weight: 600;
  font-size: 14px;
  color: #172b4d;
  cursor: pointer;
  transition: background-color 0.2s;
}
.explore-btn:hover:not(:disabled) { background-color: #091e420a; }
.explore-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(9, 30, 66, 0.54);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.jira-modal {
  background-color: #ffffff;
  border-radius: 8px;
  width: 540px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25);
  padding: 64px 48px;
}

.jira-modal-body {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.text-center { text-align: center; }

.jira-modal-title {
  font-size: 28px;
  font-weight: bold;
  color: #172b4d;
  margin: 0 0 8px 0;
  letter-spacing: -0.5px;
  line-height: 1.2;
}

.jira-subtitle {
  font-size: 14px;
  color: #5e6c84;
  margin: 0 0 48px 0;
}

.form-group {
  width: 100%;
  margin-bottom: 24px;
}

.jira-label {
  display: block;
  font-size: 12px;
  color: #5e6c84;
  margin-bottom: 8px;
  font-weight: 500;
}

.jira-input-wrapper {
  display: flex;
  align-items: center;
  width: 100%;
  border: 2px solid #dfe1e6;
  border-radius: 24px;
  padding: 0 16px;
  height: 48px;
  box-sizing: border-box;
  transition: border-color 0.2s;
  background: white;
}

.jira-input-wrapper:focus-within { border-color: #4c9aff; }
.jira-input-wrapper.checking { border-color: #0052cc; }
.jira-input-wrapper.success { border-color: #00875a; }
.jira-input-wrapper.error { border-color: #de350b; }

.jira-input {
  flex: 1;
  border: none !important;
  outline: none !important;
  font-size: 16px !important;
  color: #172b4d !important;
  background: transparent !important;
  background-color: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
  width: 100%;
}

.jira-input-suffix {
  display: flex;
  align-items: center;
  gap: 8px;
}

.domain-text {
  color: #6b778c;
  font-size: 16px;
}

.jira-input-wrapper.checking .fa-spin { color: #0052cc; }
.jira-input-wrapper.success .fa-circle-check { color: #00875a; font-size: 18px; }
.jira-input-wrapper.error .fa-triangle-exclamation { color: #de350b; font-size: 18px; }

.jira-error-text {
  color: #de350b;
  font-size: 12px;
  margin-top: 8px;
  font-weight: 500;
}

.pill-btn.full-width.jira-continue-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  margin-top: 8px;
}
.pill-btn:disabled {
  background-color: #091e420a !important;
  color: #a5adba !important;
  cursor: not-allowed;
}

.jira-modal-footer {
  margin-top: 48px;
  font-size: 14px;
  text-align: center;
}

.or-text { color: #5e6c84; }

.join-link {
  color: #0052cc;
  text-decoration: none;
  font-weight: 500;
}
.join-link:hover { text-decoration: underline; }

.join-modal {
  width: 480px;
  padding: 48px 40px;
}

.logged-in-text {
  font-size: 12px;
  color: #5e6c84;
  margin-top: -32px;
  margin-bottom: 24px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  flex-wrap: wrap;
}
.logged-in-text strong { color: #172b4d; }

/* switchAccount – chưa có flow → disabled dạng nút nhỏ */
.switch-account-btn {
  background: none;
  border: none;
  color: #a5adba;
  font-size: 12px;
  cursor: not-allowed;
  text-decoration: underline;
  padding: 0;
  font-family: inherit;
}

.site-list-container {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 350px;
  overflow-y: auto;
  margin-bottom: 8px;
  scrollbar-width: thin;
  scrollbar-color: #dfe1e6 transparent;
}
.site-list-container::-webkit-scrollbar { width: 6px; }
.site-list-container::-webkit-scrollbar-track { background: transparent; }
.site-list-container::-webkit-scrollbar-thumb {
  background-color: #dfe1e6;
  border-radius: 10px;
}

.site-list-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  padding: 12px 16px;
  background-color: white;
  transition: background-color 0.2s, box-shadow 0.2s;
}
.site-list-item:hover { background-color: #f4f5f7; }

.site-list-item-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
  text-align: left;
}

.site-list-item-title {
  font-size: 12px;
  color: #5e6c84;
}

.site-list-item-url {
  font-size: 14px;
  color: #172b4d;
}

.pill-btn.small {
  padding: 6px 16px;
  font-size: 13px;
}
</style>
