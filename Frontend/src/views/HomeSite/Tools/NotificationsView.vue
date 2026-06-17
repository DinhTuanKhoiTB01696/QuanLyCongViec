<template>
  <div class="notifications-page">
    <div class="page-header">
      <h1>Thông báo</h1>
    </div>

    <div class="notifications-layout">
      <!-- Left Sidebar Filters -->
      <aside class="filters-sidebar">
        <div class="filter-group">
          <button class="filter-btn" :class="{ active: currentFilter === 'all' }" @click="currentFilter = 'all'">
            <i class="fa-regular fa-square-check"></i>
            Tất cả
          </button>
          <button class="filter-btn" :class="{ active: currentFilter === 'direct' }" @click="currentFilter = 'direct'">
            <i class="fa-regular fa-flag"></i>
            Trực tiếp
          </button>
          <button class="filter-btn" :class="{ active: currentFilter === 'watching' }" @click="currentFilter = 'watching'">
            <i class="fa-regular fa-eye"></i>
            Đang theo dõi
          </button>
        </div>

        <div class="filter-section">
          <h3 class="section-title">ỨNG DỤNG</h3>
          <button class="filter-btn" :class="{ active: currentApp === 'all' }" @click="currentApp = 'all'">
            <i class="fa-solid fa-layer-group"></i>
            Tất cả ứng dụng
          </button>
          <button class="filter-btn" :class="{ active: currentApp === 'sprinta' }" @click="currentApp = 'sprinta'">
            <i class="fa-brands fa-jira" style="color: #0052CC;"></i>
            SprintA
          </button>
        </div>
      </aside>

      <!-- Main Notifications Content -->
      <main class="notifications-main">
        <div class="main-header">
          <h2>Mới nhất</h2>
          <div class="header-actions">
            <button class="text-action-btn">Đánh dấu tất cả là đã đọc</button>
            <div class="toggle-wrapper">
              <span class="toggle-label">Chỉ hiển thị chưa đọc</span>
              <label class="switch">
                <input type="checkbox" v-model="showUnreadOnly">
                <span class="slider round"></span>
              </label>
            </div>
          </div>
        </div>

        <div class="notifications-list">
          <div class="time-group-title">Mới nhất</div>

          <div class="notification-item">
            <div class="notif-avatar">AH</div>
            <div class="notif-content">
              <div class="notif-header">
                <strong>Anh Quan Ng Hoang</strong> changed a subtask from In Progress to Done <span class="notif-time">3 ngày trước</span>
              </div>
              <div class="notif-link">
                <i class="fa-solid fa-code-branch text-blue"></i>
                <a href="#">Làm trang quản lý riêng cho space</a>
              </div>
              <div class="notif-meta">DTN-9 • Done</div>
              <div class="notif-updates">
                <div class="small-avatar">AH</div>
                <a href="#">+2 lượt chuyển đổi từ Anh Quan Ng Hoang</a>
              </div>
            </div>
            <div class="notif-status unread"></div>
          </div>

          <div class="notification-item">
            <div class="notif-avatar">AH</div>
            <div class="notif-content">
              <div class="notif-header">
                <strong>Anh Quan Ng Hoang</strong> updated a task <span class="notif-time">4 ngày trước</span>
              </div>
              <div class="notif-link">
                <i class="fa-solid fa-check-square text-blue"></i>
                <a href="#">Thiết Kế giao diện Trang Admin</a>
              </div>
              <div class="notif-meta">DTN-8 • In Progress</div>
              <div class="notif-updates">
                <div class="small-avatar">AH</div>
                <a href="#">+2 thông tin cập nhật từ Anh Quan Ng Hoang</a>
              </div>
            </div>
            <div class="notif-status unread"></div>
          </div>

          <div class="notification-item">
            <div class="notif-avatar orange">TB</div>
            <div class="notif-content">
              <div class="notif-header">
                <strong>Thịnh Phát Bùi</strong> replied to a comment <span class="notif-time">1 tuần trước</span>
              </div>
              <div class="notif-link">
                <i class="fa-solid fa-comment text-blue"></i>
                <a href="#">ok, tôi sẽ kiểm tra lại phần này.</a>
              </div>
              <div class="notif-meta">DTN-7 • In Review</div>
              <div class="notif-updates">
                <div class="small-avatar orange">TB</div>
                <a href="#">+2 thông tin cập nhật từ Thịnh Phát Bùi</a>
              </div>
            </div>
            <div class="notif-status unread"></div>
          </div>

          <div class="notification-item">
            <div class="notif-avatar blue-icon">
              <i class="fa-solid fa-layer-group"></i>
            </div>
            <div class="notif-content">
              <div class="notif-header">
                <strong>1 mục của bạn đã quá hạn</strong> <span class="notif-time">1 tuần trước</span>
              </div>
              <div class="notif-link">
                <a href="#">Xem công việc</a>
              </div>
              <div class="notif-meta">Trình điều hướng công việc</div>
            </div>
            <div class="notif-status unread"></div>
          </div>

        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const currentFilter = ref('all')
const currentApp = ref('sprinta')
const showUnreadOnly = ref(false)
</script>

<style scoped>
.notifications-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #172B4D;
  background-color: #FFFFFF;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 32px 40px 16px;
}

.page-header h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0;
}

.notifications-layout {
  display: flex;
  flex: 1;
  padding: 0 40px 40px;
  gap: 32px;
}

/* Left Sidebar Filters */
.filters-sidebar {
  width: 240px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.filter-section {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.section-title {
  font-size: 11px;
  font-weight: 600;
  color: #5E6C84;
  margin: 0 0 8px 12px;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  background: transparent;
  border: none;
  padding: 8px 12px;
  border-radius: 3px;
  font-size: 14px;
  color: #42526E;
  cursor: pointer;
  text-align: left;
  transition: background-color 0.2s, color 0.2s;
}

.filter-btn i {
  font-size: 16px;
  width: 20px;
  text-align: center;
  color: #6B778C;
}

.filter-btn:hover {
  background-color: #FAFBFC;
}

.filter-btn.active {
  background-color: #E6FCFF;
  color: #0052CC;
  font-weight: 500;
}

.filter-btn.active i {
  color: #0052CC;
}

/* Main Notifications Content */
.notifications-main {
  flex: 1;
  max-width: 900px;
}

.main-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 16px;
  border-bottom: 1px solid #DFE1E6;
  margin-bottom: 16px;
}

.main-header h2 {
  font-size: 16px;
  font-weight: 600;
  color: #172B4D;
  margin: 0;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 24px;
}

.text-action-btn {
  background: transparent;
  border: none;
  color: #5E6C84;
  font-size: 13px;
  cursor: pointer;
}

.text-action-btn:hover {
  text-decoration: underline;
}

.toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.toggle-label {
  font-size: 13px;
  color: #5E6C84;
}

/* Toggle Switch Styles */
.switch {
  position: relative;
  display: inline-block;
  width: 32px;
  height: 18px;
}

.switch input { 
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #A5ADBA;
  transition: .4s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 14px;
  width: 14px;
  left: 2px;
  bottom: 2px;
  background-color: white;
  transition: .4s;
}

input:checked + .slider {
  background-color: #36B37E;
}

input:checked + .slider:before {
  transform: translateX(14px);
}

.slider.round {
  border-radius: 18px;
}

.slider.round:before {
  border-radius: 50%;
}

/* Notifications List */
.time-group-title {
  font-size: 12px;
  color: #5E6C84;
  margin-bottom: 16px;
}

.notification-item {
  display: flex;
  gap: 16px;
  padding: 16px 0;
  border-bottom: 1px solid #DFE1E6;
}

.notif-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: #172B4D;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  flex-shrink: 0;
}

.notif-avatar.orange {
  background-color: #FF991F;
}

.notif-avatar.blue-icon {
  background-color: #0052CC;
}

.notif-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.notif-header {
  font-size: 14px;
  color: #172B4D;
}

.notif-time {
  color: #5E6C84;
  margin-left: 4px;
}

.notif-link {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
}

.notif-link a {
  color: #0052CC;
  text-decoration: none;
}

.notif-link a:hover {
  text-decoration: underline;
}

.text-blue {
  color: #0052CC;
}

.notif-meta {
  font-size: 12px;
  color: #5E6C84;
}

.notif-updates {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
}

.small-avatar {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background-color: #172B4D;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 8px;
  font-weight: 600;
}

.small-avatar.orange {
  background-color: #FF991F;
}

.notif-updates a {
  font-size: 12px;
  color: #0052CC;
  text-decoration: none;
}

.notif-updates a:hover {
  text-decoration: underline;
}

.notif-status {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
  margin-top: 6px;
}

.notif-status.unread {
  background-color: #0052CC;
}
</style>
