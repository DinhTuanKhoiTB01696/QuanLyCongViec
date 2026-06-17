<template>
  <div class="teams-dashboard">
    <div class="dashboard-content">
      <!-- Empty State matching Team1_Tab1_ForYou1.jpeg -->
      <div v-if="teams.length === 0" class="empty-state-banner">
        <div class="empty-banner-content">
          <div class="empty-banner-text">
            <h2>Đội ngũ của bạn</h2>
            <p>Nơi hội tụ các thành viên, nỗ lực và tài nguyên.</p>
            <div class="empty-banner-actions">
              <button class="primary-btn" @click="openCreateTeamModal">Bắt đầu một đội ngũ</button>
              <router-link to="/home/teams/list" class="secondary-btn">Duyệt xem các đội ngũ</router-link>
            </div>
          </div>
          <div class="empty-banner-illustration">
            <div class="mock-illustration">
              <i class="fa-solid fa-users-viewfinder"></i>
            </div>
          </div>
        </div>
      </div>

      <!-- Content when user has teams -->
      <div v-else class="teams-sections">


        <section class="dashboard-section">
          <div class="section-header" style="margin-bottom: 24px;">
            <h2 style="font-size: 16px; margin-bottom: 16px; color: #172B4D; font-weight: 500;">Đội ngũ của bạn</h2>
            
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div class="search-box" style="position: relative; width: 300px;">
                <i class="fa-solid fa-magnifying-glass search-icon" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #6B778C; z-index: 1;"></i>
                <input type="text" placeholder="Tìm kiếm các đội ngũ" class="search-input" style="width: 100%; padding: 8px 12px 8px 36px !important; border: 1px solid #DFE1E6; border-radius: 3px; outline: none; font-size: 14px; color: #172B4D; height: 36px; transition: border-color 0.2s;" />
              </div>
              
              <div class="view-toggle" style="display: flex; border: 1px solid #DFE1E6; border-radius: 3px; overflow: hidden; height: 32px;">
                <button class="icon-btn" style="border: none; border-radius: 0; background: #DEEBFF; color: #0052CC; padding: 0 12px; height: 100%; display: flex; align-items: center; justify-content: center;"><i class="fa-solid fa-table-cells-large"></i></button>
                <div style="width: 1px; background-color: #DFE1E6; height: 100%;"></div>
                <button class="icon-btn" style="border: none; border-radius: 0; background: white; color: #6B778C; padding: 0 12px; height: 100%; display: flex; align-items: center; justify-content: center;"><i class="fa-solid fa-list"></i></button>
              </div>
            </div>
          </div>
          
          <div class="team-cards-grid">
            <div class="team-card" v-for="team in teams" :key="team.id" @click="goToTeam(team.id)">
              <div class="team-card-cover" :style="{ backgroundColor: '#0052cc' }"></div>
              <div class="team-card-content">
                <div class="team-avatar">{{ team.avatarText }}</div>
                <h3 class="team-name">{{ team.name }}</h3>
                <p class="team-meta">{{ team.memberCount }} thành viên</p>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>

    <!-- Create Team Popover -->
    <div class="modal-overlay invisible-backdrop" v-if="isCreateModalOpen" @click.self="isCreateModalOpen = false; isMemberDropdownOpen = false">
      <div class="modal-content side-popover">
        <div class="popover-header">
          <button class="icon-btn-small" @click="isCreateModalOpen = false"><i class="fa-solid fa-arrow-left"></i></button>
          <span class="popover-title"><i class="fa-solid fa-user-group"></i> Đội ngũ</span>
        </div>
        <div class="popover-body">
          <p class="required-subtitle">Các trường bắt buộc được đánh dấu bằng dấu sao <span class="required">*</span></p>

          <div class="form-group">
            <label>Tên <span class="required">*</span></label>
            <input type="text" v-model="newTeamData.name" />
          </div>

          <div class="form-group" style="position: relative;">
            <label>Thêm thành viên đội <span class="required">*</span></label>
            <div class="tags-input-container" @click="focusMemberInput">
              <div class="tag-chip" v-for="member in newTeamData.members" :key="member.id">
                 <div class="member-avatar-micro" :style="{ backgroundColor: member.color || '#0052CC' }">{{ member.initials }}</div>
                 {{ member.name }}
                 <i class="fa-solid fa-xmark remove-tag" @click.stop="removeMember(member.id)"></i>
              </div>
              <input type="text" ref="memberInputRef" placeholder="Nhập tên" v-model="memberSearchQuery" @focus="isMemberDropdownOpen = true" />
            </div>
            <!-- Member Dropdown -->
            <div class="member-dropdown" v-if="isMemberDropdownOpen">
              <div class="member-dropdown-item" v-for="user in filteredUsers" :key="user.id" @click="addMember(user)">
                <div class="member-avatar-micro" :style="{ backgroundColor: user.color || '#0052CC' }">{{ user.initials }}</div>
                <span>{{ user.name }}</span>
              </div>
              <div class="member-dropdown-empty" v-if="filteredUsers.length === 0">Không tìm thấy thành viên</div>
            </div>
          </div>

          <div class="form-group">
            <label>Loại <span class="required">*</span></label>
            <div class="select-wrapper">
              <select v-model="newTeamData.type" class="jira-select">
                <option value="Đội ngũ chính thức">Đội ngũ chính thức</option>
                <option value="Nhóm">Nhóm</option>
              </select>
            </div>
          </div>
          
          <div class="recaptcha-text">
            Trang web này được bảo vệ bằng reCAPTCHA và được điều chỉnh theo <a href="#">Chính sách Quyền riêng tư <i class="fa-solid fa-arrow-up-right-from-square" style="font-size: 10px;"></i></a> và <a href="#">Điều khoản dịch vụ <i class="fa-solid fa-arrow-up-right-from-square" style="font-size: 10px;"></i></a> của Google.
          </div>
        </div>
        <div class="popover-footer">
          <button class="cancel-btn" @click="isCreateModalOpen = false">Hủy</button>
          <button class="primary-btn" :disabled="!newTeamData.name || newTeamData.members.length === 0 || isCreating" @click="submitCreateTeam">Tạo</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useTeamStore } from '@/store/useTeamStore'
import { usePeopleStore } from '@/store/usePeopleStore'

const router = useRouter()
const teamStore = useTeamStore()
const peopleStore = usePeopleStore()

const isCreateModalOpen = ref(false)
const isCreating = ref(false)
const memberSearchQuery = ref('')
const isMemberDropdownOpen = ref(false)
const memberInputRef = ref(null)

const newTeamData = reactive({
  name: '',
  description: '',
  type: 'Đội ngũ chính thức',
  members: [
    { id: 'currentUser', name: 'Tua20000', initials: 'T', color: '#00875A' }
  ]
})

const filteredUsers = computed(() => {
  const allUsers = peopleStore.users.map(u => ({
    id: u.id,
    name: u.fullName || u.email,
    initials: u.avatar || 'U',
    color: '#0052CC' // default color or fetch from user
  }))
  if (!memberSearchQuery.value) return allUsers.filter(u => !newTeamData.members.find(m => m.id === u.id))
  const q = memberSearchQuery.value.toLowerCase()
  return allUsers.filter(u => u.name.toLowerCase().includes(q) && !newTeamData.members.find(m => m.id === u.id))
})

const focusMemberInput = () => {
  memberInputRef.value?.focus()
  isMemberDropdownOpen.value = true
}

const addMember = (user) => {
  newTeamData.members.push(user)
  memberSearchQuery.value = ''
  isMemberDropdownOpen.value = false
}

const removeMember = (id) => {
  newTeamData.members = newTeamData.members.filter(m => m.id !== id)
}

const teams = computed(() => teamStore.allTeams.map(t => ({
  id: t.id,
  name: t.name,
  avatarText: t.name ? t.name.substring(0, 2).toUpperCase() : 'T',
  memberCount: t.members?.length || 0,
  coverImage: t.coverImage || ''
})))

onMounted(() => {
  teamStore.fetchAllTeams()
  peopleStore.fetchPeople()
  window.addEventListener('global-create-click', openCreateTeamModal)
})

onUnmounted(() => {
  window.removeEventListener('global-create-click', openCreateTeamModal)
})

const goToTeam = (id) => {
  router.push(`/home/teams/${id}`)
}

const goToPerson = (id) => {
  router.push(`/home/people/${id}`)
}

const openCreateTeamModal = () => {
  newTeamData.name = ''
  newTeamData.description = ''
  newTeamData.type = 'Đội ngũ chính thức'
  newTeamData.members = [{ id: 'currentUser', name: 'Tua20000', initials: 'T', color: '#00875A' }]
  isCreateModalOpen.value = true
  isMemberDropdownOpen.value = false
  memberSearchQuery.value = ''
}

const submitCreateTeam = async () => {
  if (!newTeamData.name) return
  isCreating.value = true
  try {
    const createdTeam = await teamStore.createTeam({
      name: newTeamData.name,
      description: newTeamData.description,
      type: newTeamData.type,
      members: newTeamData.members
    })
    isCreateModalOpen.value = false
    router.push(`/home/teams/${createdTeam.id}`)
  } catch (err) {
    console.error(err)
  } finally {
    isCreating.value = false
  }
}
</script>

<style scoped>
.teams-dashboard {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

.dashboard-content {
  display: flex;
  flex-direction: column;
}

/* Empty State Banner matching the screenshot */
.empty-state-banner {
  background-color: #FAFBFC;
  border-radius: 8px;
  overflow: hidden;
  margin-top: 24px;
}

.empty-banner-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 40px 64px;
  max-width: 1000px;
  margin: 0 auto;
}

.empty-banner-text {
  flex: 1;
  max-width: 400px;
}

.empty-banner-text h2 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 16px 0;
}

.empty-banner-text p {
  font-size: 16px;
  color: #42526E;
  margin: 0 0 32px 0;
  line-height: 1.5;
}

.empty-banner-actions {
  display: flex;
  gap: 16px;
  align-items: center;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover:not(:disabled) {
  background-color: #0047B3;
}

.secondary-btn {
  background-color: rgba(9, 30, 66, 0.04);
  color: #42526E;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
  transition: background-color 0.2s;
}

.secondary-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.empty-banner-illustration {
  flex: 1;
  display: flex;
  justify-content: flex-end;
}

.mock-illustration {
  width: 280px;
  height: 200px;
  background-color: #E6FCFF;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mock-illustration i {
  font-size: 64px;
  color: #0052CC;
}

/* Filled State */
.dashboard-section {
  display: flex;
  flex-direction: column;
}

.mt-32 {
  margin-top: 32px;
}

.section-header h2 {
  font-size: 20px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 16px 0;
}

/* People List */
.people-list {
  display: flex;
  gap: 16px;
  overflow-x: auto;
  padding-bottom: 8px;
}

.person-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  cursor: pointer;
  min-width: 200px;
  transition: background-color 0.2s, box-shadow 0.2s;
}

.person-card:hover {
  background-color: #FAFBFC;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.person-avatar {
  width: 32px;
  height: 32px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.person-info {
  display: flex;
  flex-direction: column;
}

.person-name {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
}

.person-role {
  font-size: 12px;
  color: #5E6C84;
}

/* Team Cards */
.team-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 24px;
}

.team-card {
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
  display: flex;
  flex-direction: column;
}

.team-card:hover {
  box-shadow: 0 4px 8px -2px rgba(9, 30, 66, 0.25), 0 0 1px rgba(9, 30, 66, 0.31);
  transform: translateY(-2px);
}

.team-card-cover {
  height: 64px;
}

.team-card-content {
  padding: 0 16px 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-top: -24px;
}

.team-avatar {
  width: 48px;
  height: 48px;
  background-color: #00875A;
  color: white;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
  border: 2px solid #FFFFFF;
  margin-bottom: 12px;
}

.team-name {
  margin: 0 0 4px 0;
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
  text-align: center;
}

.team-meta {
  margin: 0;
  font-size: 12px;
  color: #5E6C84;
}

/* Modal/Popover Styles */
.modal-overlay.invisible-backdrop {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: transparent;
  z-index: 1000;
}

.side-popover {
  position: absolute;
  top: 60px; /* Below topbar */
  right: 16px;
  width: 380px;
  background-color: #FFFFFF;
  border-radius: 3px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25), 0 0 1px rgba(9, 30, 66, 0.31);
  display: flex;
  flex-direction: column;
}

.popover-header {
  padding: 16px 20px 12px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.icon-btn-small {
  background: none;
  border: none;
  color: #42526E;
  cursor: pointer;
  padding: 4px 6px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.icon-btn-small:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.popover-title {
  font-size: 16px;
  font-weight: 600;
  color: #172B4D;
  display: flex;
  align-items: center;
  gap: 8px;
}

.popover-body {
  padding: 0 20px 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.required-subtitle {
  margin: 0;
  font-size: 12px;
  color: #5E6C84;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
}

.required {
  color: #DE350B;
}

.form-group input {
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  outline: none;
  transition: border-color 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.form-group input:focus {
  border-color: #4C9AFF;
}

/* Tags Input */
.tags-input-container {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
  padding: 4px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  min-height: 40px;
  box-sizing: border-box;
  cursor: text;
  position: relative;
  transition: border-color 0.2s;
}

.tags-input-container:focus-within {
  border-color: #4C9AFF;
}

.tags-input-container input {
  border: none !important;
  outline: none !important;
  flex: 1;
  min-width: 80px;
  padding: 4px 8px !important;
}

.tag-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 24px;
  padding: 2px 8px 2px 2px;
  font-size: 12px;
  color: #172B4D;
  font-weight: 500;
}

.member-avatar-micro {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.remove-tag {
  color: #5E6C84;
  cursor: pointer;
  font-size: 10px;
  margin-left: 4px;
}

.remove-tag:hover {
  color: #DE350B;
}

/* Member Dropdown */
.member-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #FFFFFF;
  border-radius: 3px;
  box-shadow: 0 4px 8px -2px rgba(9, 30, 66, 0.25), 0 0 1px rgba(9, 30, 66, 0.31);
  margin-top: 4px;
  z-index: 10;
  max-height: 200px;
  overflow-y: auto;
}

.member-dropdown-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.member-dropdown-item:hover {
  background-color: #FAFBFC;
}

.member-dropdown-item span {
  font-size: 14px;
  color: #172B4D;
}

.member-dropdown-empty {
  padding: 12px;
  text-align: center;
  font-size: 13px;
  color: #5E6C84;
}

/* Select */
.select-wrapper {
  position: relative;
}

.jira-select {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  appearance: none;
  outline: none;
  cursor: pointer;
  transition: border-color 0.2s;
}

.jira-select:focus {
  border-color: #4C9AFF;
}

/* Recaptcha */
.recaptcha-text {
  font-size: 11px;
  color: #5E6C84;
  line-height: 1.5;
  margin-top: 8px;
}

.recaptcha-text a {
  color: #0052CC;
  text-decoration: none;
}

.recaptcha-text a:hover {
  text-decoration: underline;
}

.popover-footer {
  padding: 16px 20px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
