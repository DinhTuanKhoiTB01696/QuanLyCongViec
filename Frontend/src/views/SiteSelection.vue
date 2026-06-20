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

        <button class="pill-btn blue" @click="router.push('/')">{{ t('Go to SprintA') }}</button>

        <button class="pill-btn blue" @click="goToSpaceProject(recentSite?.id)" :disabled="!recentSite">{{ t('siteSelection.goToSprintA') }}</button>

        <div class="user-profile">
          <div class="user-avatar-circle" :style="{ backgroundColor: avatarColor }">{{ userInitials }}</div>
          <span class="user-name-text">{{ userEmailPrefix }}</span>
        </div>
      </div>
    </header>

    <main class="start-content">
      <div class="welcome-container">
        <h1 class="welcome-title">
          {{ t('Welcome back,') }} <span class="highlight-wrapper">{{ userEmailPrefix }}</span>.
          {{ t('siteSelection.welcomeBack') }} <span class="highlight-wrapper">{{ userName }}</span>.
          <svg class="squiggly-line" width="100%" height="12" viewBox="0 0 100 12" preserveAspectRatio="none" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M0 6 Q 12 0, 25 6 T 50 6 T 75 6 T 100 6" stroke="#FFAB00" stroke-width="3" stroke-linecap="round" fill="none"/>
          </svg>
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

        <div class="recent-site-card" v-if="recentSite">
          <div class="site-card-left">
            <div class="site-avatar-square" :style="{ backgroundColor: recentSite.color || '#b3df72' }">
              {{ recentSite.avatarText || (recentSite.name ? recentSite.name.substring(0, 2).toUpperCase() : userInitials) }}
            </div>
            <div class="site-info-stack">
              <span class="site-name-bold">{{ recentSite.name || userName }}</span>
              <div class="member-avatars">

                <div 
                  class="member-circle" 
                  v-for="(member, idx) in visibleMembers" 
                  :key="member.id" 
                  :style="{ backgroundColor: getMemberColor(member.fullName || member.name, idx) }"
                >
                  {{ member.avatar || getInitials(member.fullName || member.name) }}
                </div>
                <div class="member-count" v-if="hiddenMembersCount > 0">+{{ hiddenMembersCount }}</div>

                <!-- Chỉ hiển thị người dùng hiện tại (dữ liệu thật). Chưa có API danh sách member nên không thêm avatar/"+N" giả. -->
                <div class="member-circle" style="background-color: #00875A">{{ userInitials }}</div>

              </div>
            </div>
          </div>
          <div class="site-card-right">

            <button class="pill-btn orange" @click="goToSpaceProject(recentSite.id)">{{ t('Go to Space') }}</button>

            <button class="pill-btn orange" @click="goToSpaceProject(recentSite.id)">{{ t('siteSelection.goToSprintA') }}</button>

          </div>
        </div>

        <div class="card-footer-row">

          <router-link to="/home" class="different-site-link">{{ t('Looking for a different site?') }} &rarr;</router-link>

          <router-link to="/sites" class="different-site-link">{{ t('siteSelection.lookingForDifferent') }} &rarr;</router-link>

          
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
        <!-- Chưa có route/tài liệu thật cho tính năng này -> disable rõ ràng, không để nút bấm im lặng. -->
        <button class="explore-btn" disabled :title="t('siteSelection.notSupportedTitle')">{{ t('siteSelection.exploreFeatures') }}</button>
      </div>
    </main>

    <!-- Create Site Modal -->
    <div class="modal-overlay" v-if="isCreateModalOpen" @click.self="closeCreateModal">
      <div class="jira-modal">
        <div class="jira-modal-body">
          <h1 class="jira-modal-title text-center">Tạo không gian mới</h1>
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
            <span class="or-text">or </span><a href="#" class="join-link" @click.prevent="switchToJoinModal">{{ t('siteSelection.joinExisting') }}</a>
          </div>
        </div>
      </div>
    </div>
    <!-- Join Site Modal -->
    <div class="modal-overlay" v-if="isJoinModalOpen" @click.self="closeJoinModal">
      <div class="jira-modal join-modal">
        <div class="jira-modal-body">
          <h1 class="jira-modal-title text-center">Tham gia không gian hiện có</h1>
          <p class="jira-subtitle">{{ t('siteSelection.pickUpShort') }}</p>

          <p class="logged-in-text">
            {{ t('siteSelection.loggedInAs') }} <strong>{{ userEmail }}</strong>. <a href="#" class="switch-account-link">{{ t('siteSelection.switchAccount') }}</a>
          </p>

          <div class="site-list-container">
            <div class="site-list-item" v-for="site in sites" :key="site.id">
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
            <span class="or-text">or </span><a href="#" class="join-link" @click.prevent="switchToCreateModal">{{ t('siteSelection.startNewSite') }}</a>
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
import { usePeopleStore } from '@/store/usePeopleStore'
import { getStoredUser } from '@/utils/permissions'
import { useI18n } from '@/composables/useI18n'

const router = useRouter()
const siteStore = useSiteStore()
const peopleStore = usePeopleStore()
const { t } = useI18n()


const currentUser = getStoredUser()
const userEmail = currentUser?.email || ''
const userEmailPrefix = userEmail ? userEmail.split('@')[0] : 'User'
const userName = currentUser?.fullName || currentUser?.name || currentUser?.publicName || currentUser?.username || userEmailPrefix

const getInitials = (name) => {
  if (!name) return '?'
  return name
    .split(' ')
    .filter(Boolean)
    .map((word) => word[0])
    .join('')
    .substring(0, 2)
    .toUpperCase()
}

const userInitials = getInitials(userName)

const avatarColor = computed(() => {
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d']
  const name = userName || 'User'
  const index = name.length % colors.length
  return colors[index]
})

const recentSite = computed(() => siteStore.recentSite)
const sites = computed(() => siteStore.sites)

const siteMembers = computed(() => peopleStore.users)
const visibleMembers = computed(() => siteMembers.value.slice(0, 4))
const hiddenMembersCount = computed(() => Math.max(0, siteMembers.value.length - 4))

const getMemberColor = (name, index) => {
  const colors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d']
  if (!name) return colors[index % colors.length]
  return colors[name.length % colors.length]
}

onMounted(async () => {
  await siteStore.fetchSites()
  await peopleStore.fetchPeople()
})

const isCreateModalOpen = ref(false)
const isJoinModalOpen = ref(false)
const newSiteName = ref('')
const isCreating = ref(false)
const errorMessage = ref('')
const creationSuccess = ref(false)
const createdSite = ref(null)

const validationState = ref('idle') // idle, checking, success, error
const siteUrlPreview = ref('')
let debounceTimer = null

watch(newSiteName, (newVal) => {
  if (!newVal) {
    validationState.value = 'idle'
    siteUrlPreview.value = ''
    errorMessage.value = ''
    return
  }
  
  validationState.value = 'checking'
  const formattedName = newVal.toLowerCase().replace(/[^a-z0-9-]/g, '')
  siteUrlPreview.value = `${formattedName}.sprinta.vn`
  
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
  const baseName = userEmail ? userEmail.split('@')[0] : userName.toLowerCase().replace(/[^a-z0-9]/g, '');
  const randomSuffix = Math.floor(1000 + Math.random() * 9000);
  newSiteName.value = `${baseName}-${randomSuffix}`;
  validationState.value = 'idle'
  siteUrlPreview.value = ''
  errorMessage.value = ''
  creationSuccess.value = false
}

const closeCreateModal = () => {
  isCreateModalOpen.value = false
}

const closeJoinModal = () => {
  isJoinModalOpen.value = false
}

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
    // Direct redirect upon success instead of showing intermediate state
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
  // Ghi nhận site vừa chọn rồi đưa người dùng tới trang "Dành cho bạn" (/dashboard -> ForYou.vue),
  // không vào thẳng project dashboard mặc định.
  siteStore.setRecentSite(siteStore.sites.find(s => s.id === siteId) || { id: siteId })
  router.push('/dashboard')
}
</script>

<style scoped>
.start-page-wrapper {
  min-height: 100vh;
  background-color: var(--color-bg);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  display: flex;
  flex-direction: column;
}

.start-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
  background-color: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
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
  background-color: var(--color-accent);
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

.sprinta-logo-img { height: 32px; width: auto; }
.sprinta-logo-img.small { height: 16px; width: auto; margin: 0 6px; }

.sprinta-logo-img.small {
  height: 16px;
  width: auto;
  transform: scale(4);
  margin: 0 16px;
}

.app-logo .logo-text {
  font-size: 24px;
  font-weight: bold;
  color: var(--color-text-primary);
  letter-spacing: -0.5px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 24px;
}

.pill-btn {
  border-radius: var(--radius-sm);
  font-size: 14px;
  font-weight: 500;
  height: 32px;
  padding: 0 16px;
  border: none;
  cursor: pointer;
  transition: background-color 0.15s;
}

.pill-btn.blue  { background-color: var(--color-accent); color: #fff; }
.pill-btn.blue:hover { background-color: var(--color-accent-hover); }

.pill-btn.orange { background-color: #FF991F; color: var(--color-text-primary); }
.pill-btn.orange:hover { background-color: #e2851e; }

.user-profile {
  display: flex;
  align-items: center;
  gap: 8px;
  border-left: 1px solid var(--color-border);
  padding-left: 24px;
}

.user-avatar-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
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
  color: var(--color-text-primary);
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
  color: var(--color-text-primary);
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
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
}

.create-site-link {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-accent);
  text-decoration: none;
}
.create-site-link:hover { text-decoration: underline; }

.recent-site-card {
  background: white;
  border: 1px solid var(--color-text-primary)24;
  border-radius: 4px;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 1px var(--color-text-primary)0f;
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
  background-color: #b3df72;
  color: var(--color-text-primary);
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
  color: var(--color-text-primary);
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

.member-count {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid white;
  background-color: var(--color-border);
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
  margin-left: -6px;
}

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
  color: var(--color-text-primary);
  margin-bottom: 16px;
}

.explore-btn {
  background: transparent;
  border: 1px solid var(--color-text-primary);
  border-radius: 24px;
  padding: 8px 24px;
  font-weight: 600;
  font-size: 14px;
  color: var(--color-text-primary);
  cursor: pointer;
  transition: background-color 0.2s;
}

.explore-btn:hover { background-color: var(--color-text-primary)0a; }

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
/* Jira Modal Styles */
.jira-modal {
  background-color: var(--color-surface);
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

.text-center {
  text-align: center;
}

.jira-modal-title {
  font-size: 28px;
  font-weight: bold;
  color: var(--color-text-primary);
  margin: 0 0 8px 0;
  letter-spacing: -0.5px;
  line-height: 1.2;
}

.jira-subtitle {
  font-size: 14px;
  color: var(--color-text-secondary);
  margin: 0 0 48px 0;
}

.form-group {
  width: 100%;
  margin-bottom: 24px;
}

.jira-label {
  display: block;
  font-size: 12px;
  color: var(--color-text-secondary);
  margin-bottom: 8px;
  font-weight: 500;
}

.jira-input-wrapper {
  border: 2px solid var(--color-border);
  border-radius: var(--radius-sm);
  height: 40px;
  display: flex;
  align-items: center;
  background: var(--color-surface);
  overflow: hidden;
  transition: border-color 0.2s;
}

.jira-input-wrapper:focus-within { border-color: #4c9aff; }
.jira-input-wrapper.checking { border-color: var(--color-accent); }
.jira-input-wrapper.success { border-color: var(--color-success); }
.jira-input-wrapper.error { border-color: #de350b; }

.jira-input {
  flex: 1;
  border: none !important;
  outline: none !important;
  font-size: 16px !important;
  color: var(--color-text-primary) !important;
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
  color: var(--color-text-muted);
  font-size: 16px;
}

.jira-input-wrapper.checking .fa-spin { color: var(--color-accent); }
.jira-input-wrapper.success .fa-circle-check { color: var(--color-success); font-size: 18px; }
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
.pill-btn:disabled { background-color: rgba(9,30,66,0.04); color: var(--color-text-muted); cursor: not-allowed; }

.jira-modal-footer {
  margin-top: 48px;
  font-size: 14px;
  text-align: center;
}

.or-text {
  color: var(--color-text-secondary);
}

.join-link {
  color: var(--color-accent);
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
  color: var(--color-text-secondary);
  margin-top: -32px;
  margin-bottom: 24px;
  text-align: center;
}
.logged-in-text strong {
  color: var(--color-text-primary);
}
.switch-account-link {
  color: var(--color-accent);
  text-decoration: none;
}
.switch-account-link:hover { text-decoration: underline; }

.site-list-container {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 350px;
  overflow-y: auto;
  margin-bottom: 8px;
  /* Scrollbar styling */
  scrollbar-width: thin;
  scrollbar-color: var(--color-border) transparent;
}
.site-list-container::-webkit-scrollbar {
  width: 6px;
}
.site-list-container::-webkit-scrollbar-track {
  background: transparent;
}
.site-list-container::-webkit-scrollbar-thumb {
  background-color: var(--color-border);
  border-radius: 10px;
}

.site-list-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  padding: 12px 16px;
  background-color: white;
  transition: background-color 0.2s, box-shadow 0.2s;
}
.site-list-item:hover {
  background-color: var(--color-bg);
}

.site-list-item-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
  text-align: left;
}

.site-list-item-title {
  font-size: 12px;
  color: var(--color-text-secondary);
}

.site-list-item-url {
  font-size: 14px;
  color: var(--color-text-primary);
}

.pill-btn.small {
  padding: 6px 16px;
  font-size: 13px;
}
</style>
