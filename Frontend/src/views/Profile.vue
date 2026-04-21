<template>
  <NexusLayout class="profile-page">
    <div class="profile-body-container">
      <div class="profile-container" v-loading="isLoading">
        <div class="profile-header-section">
          <el-dropdown trigger="click" class="header-image-dropdown">
            <div class="header-image-box">
              <div class="avatar-inside-wrapper">
                <el-dropdown trigger="click">
                  <div class="large-profile-avatar" :style="avatarStyle">
                    {{ profileData.avatarUrl ? '' : getInitials(profileData.fullName) }}
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item @click="triggerAvatarUpload">
                        <i class="fa-solid fa-plus"></i> Thêm ảnh hồ sơ
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                  <input ref="avatarInput" type="file" style="display: none" accept="image/*" @change="uploadAvatar" />
                </el-dropdown>
              </div>

              <div class="banner-upload-prompt">
                <i class="fa-regular fa-image"></i>
                <span>Cập nhật ảnh bìa sau</span>
              </div>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="triggerAvatarUpload">
                  <i class="fa-solid fa-upload"></i> Tải ảnh lên
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <div class="header-footer-privacy">
            <div class="profile-name-block">
              <strong>{{ profileData.publicName || profileData.fullName || 'Thành viên' }}</strong>
              <span>{{ profileData.jobTitle || 'Cập nhật chức danh của bạn' }}</span>
            </div>
            <div class="header-privacy-info">
              <span>Ai có thể xem ảnh hồ sơ của bạn?</span>
              <el-dropdown trigger="click">
                <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item>Bất kỳ ai</el-dropdown-item>
                    <el-dropdown-item>Chỉ người trong tổ chức</el-dropdown-item>
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

const profileData = ref({
  fullName: '',
  publicName: '',
  jobTitle: '',
  department: '',
  organization: '',
  email: '',
  collaboration: '',
  avatarUrl: ''
})

const getBaseUrl = () => 'http://localhost:5136'

const avatarStyle = computed(() => {
  if (!profileData.value.avatarUrl) {
    return ''
  }

  return `background-image: url(${getBaseUrl()}${profileData.value.avatarUrl}); background-size: cover; color: transparent; background-position: center;`
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

const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

const uploadAvatar = async (event) => {
  const file = event.target.files?.[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const res = await axiosClient.put('/users/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    profileData.value.avatarUrl = res.data?.data?.avatarUrl || ''
    ElMessage.success('Đã cập nhật ảnh đại diện')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Lỗi khi tải ảnh')
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
      avatarUrl: data.avatarUrl || ''
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
  background-color: var(--bg-card);
  border-radius: 12px;
  overflow: visible;
  border: 1px solid var(--border-color);
  margin-bottom: 80px;
  width: 100%;
}

.header-image-dropdown {
  display: block;
  width: 100%;
}

.header-image-box {
  width: 100%;
  height: 180px;
  background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);
  display: flex;
  align-items: center;
  position: relative;
  cursor: pointer;
  border-radius: 12px 12px 0 0;
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
  background-color: #f59e0b;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 40px;
  font-weight: 700;
  color: #1d2125;
  border: 4px solid var(--bg-layout);
}

.banner-upload-prompt {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: rgba(29, 33, 37, 0.4);
  font-weight: 600;
}

.header-footer-privacy {
  background-color: var(--bg-card);
  border-radius: 0 0 12px 12px;
  padding: 40px 24px 12px;
  display: flex;
  justify-content: space-between;
  gap: 24px;
  align-items: flex-start;
}

.profile-name-block {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile-name-block strong {
  color: var(--text-primary);
  font-size: 22px;
  font-weight: 700;
}

.profile-name-block span {
  color: #8b949e;
  font-size: 14px;
}

.header-privacy-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  font-size: 13px;
  color: #8b949e;
}

.privacy-select {
  color: #8b949e;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
}

.profile-content-form {
  padding: 0 40px;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 24px;
}

.mt-40 {
  margin-top: 40px;
}

.form-grid {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-row {
  display: grid;
  grid-template-columns: 200px 1fr;
  gap: 32px;
}

.field-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: 6px;
}

.info-icon {
  font-size: 12px;
  color: #8b949e;
}

.field-input-wrapper {
  max-width: 500px;
}

.field-privacy {
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #8b949e;
}

.save-row {
  margin-top: 16px;
}

.contact-card {
  padding-bottom: 40px;
}

.email-value {
  padding: 10px 0;
  font-size: 14px;
}

.danger-item {
  color: #f87171 !important;
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
    gap: 8px;
  }

  .field-input-wrapper {
    max-width: 100%;
  }
}
</style>
