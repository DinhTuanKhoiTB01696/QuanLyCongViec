<template>
  <NexusLayout class="profile-page">
    <!-- Hidden file inputs -->
    <input ref="avatarInput" type="file" style="display: none" accept="image/*" @change="uploadAvatar" />
    <input ref="coverInput" type="file" style="display: none" accept="image/*" @change="uploadCover" />

    <div class="profile-body-container">
      <div class="profile-container" v-loading="isLoading">
        <div class="profile-header-section sharp-card">
          <!-- Cover photo banner -->
          <div
            class="header-image-box"
            :style="coverBannerStyle"
            @click="triggerCoverUpload"
            title="Click to change cover"
          >
            <!-- Avatar (absolute-positioned over the banner) -->
            <div class="avatar-inside-wrapper" @click.stop>
              <el-dropdown trigger="click" @command="handleAvatarCommand">
                <div class="large-profile-avatar" :style="avatarStyle">
                  {{ profileData.avatarUrl ? '' : getInitials(profileData.fullName) }}
                  <div class="avatar-hover-overlay">
                    <i class="fa-solid fa-camera"></i>
                  </div>
                </div>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="upload">
                      <i class="fa-solid fa-plus"></i> Add profile photo
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>

            <div class="banner-upload-prompt" v-if="!profileData.coverUrl">
              <i class="fa-regular fa-image"></i>
              <span>{{ profileData.coverUrl ? 'Change cover' : 'Add cover' }}</span>
            </div>
          </div>

          <div class="header-footer-privacy">
            <div class="profile-name-block">
              <strong>{{ profileData.publicName || profileData.fullName || 'Member' }}</strong>
              <span>{{ profileData.jobTitle || 'Update your job title' }}</span>
            </div>
            <div class="header-privacy-info">
              <span>Who can see your profile photo?</span>
              <el-dropdown trigger="click">
                <span class="privacy-select"><i class="fa-solid fa-globe"></i> Anyone <i class="fa-solid fa-chevron-down"></i></span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item>Anyone</el-dropdown-item>
                    <el-dropdown-item>Organization only</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </div>
        </div>

        <div class="profile-content-form">
          <h2 class="section-title">Giới thiệu về bạn</h2>

          <div class="form-grid">
            <div class="form-row">
              <div class="field-label">Họ tên</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.fullName" />
                <div class="field-privacy">
                  <span class="privacy-label">Ai có thể thấy được nội dung này?</span>
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown>
                      <el-dropdown-menu>
                        <el-dropdown-item>Bất kỳ ai</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <div class="form-row">
              <div class="field-label">Tên công khai <i class="fa-solid fa-circle-info info-icon"></i></div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.publicName" />
              </div>
            </div>

            <div class="form-row">
              <div class="field-label">Chức danh</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.jobTitle" placeholder="Chức danh của bạn" />
              </div>
            </div>

            <div class="form-row">
              <div class="field-label">Phòng ban</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.department" placeholder="Phòng ban của bạn" />
              </div>
            </div>

            <div class="form-row">
              <div class="field-label">Tổ chức</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.organization" placeholder="Tổ chức của bạn" />
              </div>
            </div>

            <div class="form-row">
              <div class="field-label">Cộng tác với bạn</div>
              <div class="field-input-wrapper">
                <el-input
                  v-model="profileData.collaboration"
                  type="textarea"
                  :rows="3"
                  placeholder="Giúp người khác biết cách phối hợp tốt hơn với bạn"
                />
              </div>
            </div>

            <div class="form-row">
              <div class="field-label"></div>
              <div class="field-input-wrapper save-row">
                <el-button type="primary" :loading="isSaving" @click="saveProfile">Lưu thay đổi</el-button>
              </div>
            </div>
          </div>

          <h2 class="section-title mt-40">Liên hệ</h2>
          <div class="contact-card">
            <div class="form-row">
              <div class="field-label">Địa chỉ email</div>
              <div class="field-input-wrapper">
                <div class="email-value">{{ profileData.email }}</div>
                <div class="field-privacy">
                  <span class="privacy-label">Ai có thể thấy được nội dung này?</span>
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-lock"></i> Chỉ bạn và quản trị viên <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown>
                      <el-dropdown-menu>
                        <el-dropdown-item>Riêng tư</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

const isLoading = ref(false)
const isSaving = ref(false)
const avatarInput = ref(null)
const coverInput = ref(null)

const profileData = ref({
  fullName: '',
  publicName: '',
  jobTitle: '',
  department: '',
  organization: '',
  email: '',
  collaboration: '',
  avatarUrl: '',
  coverUrl: ''
})

const getBaseUrl = () => import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5136'

const avatarStyle = computed(() => {
  if (!profileData.value.avatarUrl) return {}
  return {
    backgroundImage: `url(${getBaseUrl()}${profileData.value.avatarUrl})`,
    backgroundSize: 'cover',
    backgroundPosition: 'center',
    color: 'transparent'
  }
})

const coverBannerStyle = computed(() => {
  if (!profileData.value.coverUrl) {
    return { background: 'var(--bg-tertiary)' }
  }
  return {
    backgroundImage: `url(${getBaseUrl()}${profileData.value.coverUrl})`,
    backgroundSize: 'cover',
    backgroundPosition: 'center'
  }
})

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

const handleAvatarCommand = (command) => {
  if (command === 'upload') {
    triggerAvatarUpload()
  }
}

const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

const triggerCoverUpload = () => {
  coverInput.value?.click()
}

const uploadAvatar = async (event) => {
  const avatarFile = event.target.files?.[0]
  if (!avatarFile) return
  
  const formData = new FormData()
  formData.append('file', avatarFile)
  
  try {
    const res = await axiosClient.put('/users/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    profileData.value.avatarUrl = res.data?.data?.avatarUrl || ''
    ElMessage.success('Đã cập nhật ảnh đại diện')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Lỗi khi tải ảnh đại diện')
  } finally {
    event.target.value = ''
  }
}

const uploadCover = async (event) => {
  const coverFile = event.target.files?.[0]
  if (!coverFile) return

  const formData = new FormData()
  formData.append('file', coverFile)

  try {
    const res = await axiosClient.put('/users/cover', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    profileData.value.coverUrl = res.data?.data?.coverUrl || ''
    ElMessage.success('Đã cập nhật ảnh bìa')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Lỗi khi tải ảnh bìa')
  } finally {
    event.target.value = ''
  }
}

const fetchProfile = async () => {
  try {
    isLoading.value = true
    const response = await axiosClient.get('/users/me')
    const data = response.data?.data || {}
    profileData.value = {
      fullName: data.fullName || '',
      publicName: data.publicName || '',
      jobTitle: data.jobTitle || '',
      department: data.departmentName || '',
      organization: data.organizationName || '',
      email: data.email || '',
      collaboration: data.collaborationRules || '',
      avatarUrl: data.avatarUrl || '',
      coverUrl: data.coverUrl || ''
    }
  } catch (error) {
    console.error('Lỗi khi tải profile', error)
    ElMessage.error('Không thể tải thông tin cá nhân.')
  } finally {
    isLoading.value = false
  }
}

const saveProfile = async () => {
  try {
    isSaving.value = true
    await axiosClient.put('/users/profile', {
      fullName: profileData.value.fullName,
      publicName: profileData.value.publicName,
      jobTitle: profileData.value.jobTitle,
      departmentName: profileData.value.department,
      organizationName: profileData.value.organization,
      collaborationRules: profileData.value.collaboration
    })
    ElMessage.success('Đã lưu thông tin hồ sơ.')
    await fetchProfile()
  } catch (error) {
    console.error('Lỗi khi lưu profile', error)
    ElMessage.error(error.response?.data?.message || 'Lưu thông tin thất bại.')
  } finally {
    isSaving.value = false
  }
}

onMounted(fetchProfile)
</script>

<style scoped>
.profile-page {
  height: 100%;
}

.profile-body-container {
  height: 100%;
  overflow-y: auto;
  padding: 40px 0;
  display: flex;
  justify-content: center;
}

.profile-container {
  width: 100%;
  max-width: 800px;
  display: flex;
  flex-direction: column;
}

.profile-header-section {
  position: relative;
  background-color: var(--bg-secondary);
  border-radius: 2px;
  overflow: visible;
  border: 1px solid var(--border-color);
  margin-bottom: 80px;
  width: 100%;
}

.header-image-box {
  width: 100%;
  height: 180px;
  display: flex;
  align-items: center;
  position: relative;
  cursor: pointer;
  border-radius: 2px 2px 0 0;
  transition: filter 0.2s;
}

.header-image-box:hover {
  filter: brightness(0.88);
}

.avatar-inside-wrapper {
  position: absolute;
  left: 30px;
  bottom: -50px;
  z-index: 10;
}

.large-profile-avatar {
  height: 120px;
  width: 120px;
  background-color: var(--color-accent);
  border-radius: 50%; /* Avatar remains circular for visual distinction */
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 40px;
  font-weight: 700;
  color: #ffffff;
  border: 4px solid var(--bg-primary);
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: filter 0.2s;
}

.large-profile-avatar:hover {
  filter: brightness(0.8);
}

.avatar-hover-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background 0.2s;
  color: white;
  font-size: 22px;
  opacity: 0;
}

.large-profile-avatar:hover .avatar-hover-overlay {
  background: rgba(0,0,0,0.4);
  opacity: 1;
}

.banner-upload-prompt {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: rgba(255, 255, 255, 0.7);
  font-weight: 600;
  text-shadow: 0 1px 3px rgba(0,0,0,0.3);
  pointer-events: none;
}

.header-footer-privacy {
  background-color: var(--bg-secondary);
  border-radius: 0 0 2px 2px;
  padding: 24px 24px 12px;
  display: flex;
  justify-content: space-between;
  gap: 24px;
  align-items: flex-start;
}

.profile-name-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.profile-name-block strong {
  color: var(--color-text-primary);
  font-size: 22px;
  font-weight: 700;
}

.profile-name-block span {
  color: var(--color-text-secondary);
  font-size: 14px;
}

.header-privacy-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  font-size: 13px;
  color: var(--color-text-muted);
}

.privacy-select {
  color: var(--color-text-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  font-weight: 500;
}

.profile-content-form {
  padding: 0 40px;
}

.section-title {
  font-size: 18px;
  font-weight: 800;
  color: var(--color-text-primary);
  margin-bottom: 24px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.mt-40 {
  margin-top: 40px;
}

.form-grid {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: grid;
  grid-template-columns: 160px 1fr;
  gap: 12px; /* Tightened from 20px */
}

.field-label {
  font-size: 12px;
  font-weight: 800;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.info-icon {
  font-size: 11px;
  color: var(--color-text-muted);
}

.field-input-wrapper {
  max-width: 500px;
}

.field-privacy {
  margin-top: 6px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 11px;
  color: var(--color-text-muted);
}

.privacy-label {
  color: var(--color-text-muted);
}

.save-row {
  margin-top: 12px;
}

.contact-card {
  padding-bottom: 24px;
}

.email-value {
  padding: 6px 0;
  font-size: 14px;
  color: var(--color-text-primary);
  font-weight: 600;
}

@media (max-width: 768px) {
  .profile-header-section {
    margin-bottom: 60px;
  }

  .header-image-box {
    height: 120px;
  }

  .large-profile-avatar {
    height: 80px;
    width: 80px;
    font-size: 24px;
  }

  .avatar-inside-wrapper {
    left: 20px;
    bottom: -40px;
  }

  .header-footer-privacy {
    flex-direction: column;
    align-items: stretch;
  }

  .header-privacy-info {
    align-items: flex-start;
  }

  .profile-content-form {
    padding: 0 16px;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 4px;
  }

  .field-input-wrapper {
    max-width: 100%;
  }
}
</style>
