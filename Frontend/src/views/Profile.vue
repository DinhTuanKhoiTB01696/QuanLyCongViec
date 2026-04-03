<template>
  <NexusLayout class="profile-page">
    <div class="profile-body-container" style="padding: 40px 0; display: flex; justify-content: center;">
      <div class="profile-container">
        <!-- Header Banner & Avatar Section -->
        <div class="profile-header-section">
          <el-dropdown trigger="click" class="header-image-dropdown">
            <div class="header-image-box">
              <div class="avatar-inside-wrapper">
                <el-dropdown trigger="click">
                  <div class="large-profile-avatar">DN</div>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item><i class="fa-solid fa-plus"></i> Thêm ảnh hồ sơ</el-dropdown-item>
                      <el-dropdown-item><i class="fa-solid fa-signature"></i> Tạo hình đại diện có tên viết tắt</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </div>
              
              <div class="banner-upload-prompt">
                <i class="fa-regular fa-image"></i>
                <span>Add your header image</span>
              </div>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item><i class="fa-solid fa-upload"></i> Tải ảnh lên</el-dropdown-item>
                <el-dropdown-item class="danger-item"><i class="fa-solid fa-trash"></i> Xóa ảnh</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          
          <div class="header-footer-privacy">
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

        <!-- Main Content Form -->
        <div class="profile-content-form">
          <h2 class="section-title">Giới thiệu về bạn</h2>

          <div class="form-grid">
            <!-- Full Name -->
            <div class="form-row">
              <div class="field-label">Họ tên</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.fullName" />
                <div class="field-privacy">
                  <span class="privacy-label">Ai có thể thấy được nội dung này?</span>
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <!-- Public Name -->
            <div class="form-row">
              <div class="field-label">Tên công khai <i class="fa-solid fa-circle-info info-icon"></i></div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.publicName" />
                <div class="field-privacy">
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <!-- Job Title -->
            <div class="form-row">
              <div class="field-label">Chức danh</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.jobTitle" placeholder="Chức danh của bạn" />
                <div class="field-privacy">
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <!-- Department -->
            <div class="form-row">
              <div class="field-label">Phòng ban</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.department" placeholder="Phòng ban của bạn" />
                <div class="field-privacy">
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <!-- Organization -->
            <div class="form-row">
              <div class="field-label">Tổ chức</div>
              <div class="field-input-wrapper">
                <el-input v-model="profileData.organization" placeholder="Tổ chức của bạn" />
                <div class="field-privacy">
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>

            <!-- Collaboration -->
            <div class="form-row">
              <div class="field-label">Cộng tác với bạn</div>
              <div class="field-input-wrapper">
                <p class="field-hint">Giúp người khác nắm được thời điểm và cách thức cộng tác với bạn</p>
                <div class="field-privacy">
                  <el-dropdown trigger="click">
                    <span class="privacy-select"><i class="fa-solid fa-globe"></i> Bất kỳ ai <i class="fa-solid fa-chevron-down"></i></span>
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Bất kỳ ai</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>
          </div>

          <!-- Contact Section -->
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
                    <template #dropdown><el-dropdown-menu><el-dropdown-item>Riêng tư</el-dropdown-item></el-dropdown-menu></template>
                  </el-dropdown>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div> <!-- Closes profile-container -->
    </div> <!-- Closes profile-body-container -->
  </NexusLayout>
</template>

<script setup>
import { ref } from 'vue'
import logoImg from '../assets/logo_QLCV.png'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const profileData = ref({
  fullName: '',
  publicName: '',
  jobTitle: '',
  department: '',
  organization: '',
  email: ''
})
</script>

<style scoped>


.profile-container {
  width: 100%;
  max-width: 800px;
  display: flex;
  flex-direction: column;
}

/* Header Section Styling */
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
  display: block; /* Important for full width */
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
  padding: 40px 24px 12px; /* Increased top padding for avatar overlap */
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

/* Form Styling */
.profile-content-form {
  padding: 0 40px;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 24px;
}

.mt-40 { margin-top: 40px; }

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

.info-icon { font-size: 12px; color: #8b949e; }

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

.email-value {
  padding: 10px 0;
  font-size: 14px;
}

.field-hint {
  font-size: 13px;
  color: #8b949e;
  line-height: 1.5;
}

.danger-item { color: #f87171 !important; }


/* Component-specific overrides removed as they are now globalized in style.css */

@media (max-width: 768px) {
  .nav-brand span {
    display: none;
  }
  .main-body {
    padding: 20px 10px;
  }
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
    left: 20px;
    bottom: -40px;
  }
  .avatar-inside-wrapper {
    left: 20px;
    bottom: -40px;
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
  .top-nav .nav-right :not(:first-child) {
    display: none;
  }
}
</style>
