<template>
  <div class="status-page">
    <div class="page-header">
      <div class="header-main">
        <h1>Cập nhật trạng thái</h1>
        <div class="header-actions">
          <button class="secondary-btn">Cập nhật</button>
          <button class="secondary-btn">Viết thông tin cập nhật</button>
        </div>
      </div>
      <div class="tabs">
        <button class="tab-btn" :class="{ active: currentTab === 'projects' }" @click="currentTab = 'projects'">Projects</button>
        <button class="tab-btn" :class="{ active: currentTab === 'goals' }" @click="currentTab = 'goals'">Goals</button>
      </div>
    </div>

    <div class="page-content">
      <!-- MAIN COLUMN -->
      <div class="main-column">
        <!-- Banner removed as requested -->

        <!-- Timeline / Stats -->
        <div class="timeline-section">
          <div class="timeline-header">
            <button class="icon-btn"><i class="fa-solid fa-arrow-left"></i></button>
            <h2 class="timeline-title">Tuần trước</h2>
            <button class="icon-btn" disabled><i class="fa-solid fa-arrow-right"></i></button>
          </div>

          <p class="stats-subtitle">Bạn đang theo dõi 4 {{ itemType }} đang hoạt động, dưới đây là phần phân tích.</p>

          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-number green">1</div>
              <div class="stat-info">
                <div class="stat-label green">Đúng tiến độ</div>
                <div class="stat-desc">-2 so với tuần trước</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-number orange">1</div>
              <div class="stat-info">
                <div class="stat-label orange">Có rủi ro</div>
                <div class="stat-desc">+1 so với tuần trước</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-number red">1</div>
              <div class="stat-info">
                <div class="stat-label red">Không đúng tiến độ</div>
                <div class="stat-desc">Không có thay đổi</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-number gray">0</div>
              <div class="stat-info">
                <div class="stat-label gray">{{ currentTab === 'goals' ? 'Đang chờ cập nhật' : 'Không có bản cập nhật' }}</div>
                <div class="stat-desc">Không có thay đổi</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-number gray">0</div>
              <div class="stat-info">
                <div class="stat-label gray">Đã hủy</div>
                <div class="stat-desc">Không có thay đổi</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-number blue">1</div>
              <div class="stat-info">
                <div class="stat-label blue">Đã hoàn tất 🎉</div>
                <div class="stat-desc">+1 so với tuần trước</div>
              </div>
            </div>
          </div>

          <!-- Items List -->
          <div class="tracked-items-list">
            <div class="tracked-item">
              <div class="item-header">
                <span class="item-icon">🍉</span>
                <div class="item-name-col">
                  <span class="item-type-label">{{ itemTypeCapitalized }}</span>
                  <span class="item-name">Space shuttle autopilot AI</span>
                </div>
              </div>
              <div class="item-card-inner">
                <div class="item-body">
                  <div class="item-user">
                    <div class="item-avatar">LA</div>
                    <div class="item-user-info">
                      <span class="item-user-name">Lia Arroyo</span>
                      <span class="item-time">1 phút trước</span>
                    </div>
                  </div>
                  <div class="item-status-badge green-badge">ĐÚNG TIẾN ĐỘ</div>
                </div>
                <div class="item-message">
                  Đã thay đổi trạng thái <span class="old-status">ĐANG CHỜ XỬ LÝ</span> &rarr; <span class="new-status">ĐÚNG TIẾN ĐỘ</span>
                </div>
                <div class="item-footer">
                  <button class="footer-btn">Chia sẻ</button> <span class="dot-separator">•</span>
                  <button class="footer-btn">Bỏ theo dõi</button> <span class="dot-separator">•</span>
                  <div class="reactions">
                    <button class="reaction-btn">👍</button>
                    <button class="reaction-btn">👋</button>
                    <button class="reaction-btn">❤️</button>
                    <button class="reaction-btn">...</button>
                    <button class="reaction-btn add-reaction"><i class="fa-regular fa-face-smile"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- SIDEBAR COLUMN -->
      <div class="sidebar-column">
        <div class="sidebar-section">
          <h3>{{ itemTypeCapitalized }} của bạn</h3>
          <div class="sidebar-item">
            <div class="sidebar-item-left">
              <span class="sidebar-item-icon">😎</span>
              <span class="sidebar-item-name">ưqe</span>
            </div>
            <span class="sidebar-item-meta">1 người theo dõi</span>
          </div>
        </div>

        <div class="sidebar-section">
          <h3>{{ itemTypeCapitalized }} mới</h3>
          <div class="sidebar-item">
            <div class="sidebar-item-left">
              <span class="sidebar-item-icon">😎</span>
              <span class="sidebar-item-name">ưqe</span>
            </div>
            <span class="sidebar-item-badge">T</span>
          </div>
          <div class="sidebar-item">
            <div class="sidebar-item-left">
              <span class="sidebar-item-icon">😎</span>
              <span class="sidebar-item-name">e</span>
            </div>
            <span class="sidebar-item-badge">T</span>
          </div>
        </div>

        <div class="sidebar-section">
          <h3>{{ itemTypeCapitalized }} đã hoàn tất</h3>
          <div class="sidebar-item">
            <div class="sidebar-item-left">
              <span class="sidebar-item-icon">😎</span>
              <span class="sidebar-item-name">e</span>
            </div>
            <span class="sidebar-item-badge">T</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const currentTab = ref('projects')

const itemType = computed(() => currentTab.value === 'projects' ? 'dự án' : 'mục tiêu')
const itemTypeCapitalized = computed(() => currentTab.value === 'projects' ? 'Dự án' : 'Mục tiêu')
</script>

<style scoped>
.status-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #172B4D;
  background-color: #FFFFFF;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 32px 40px 0;
  border-bottom: 1px solid #DFE1E6;
}

.header-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-main h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.secondary-btn {
  background: #F4F5F7;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  color: #42526E;
  cursor: pointer;
  transition: background-color 0.2s;
}

.secondary-btn:hover {
  background: #EBECF0;
}

.tabs {
  display: flex;
  gap: 24px;
}

.tab-btn {
  background: transparent;
  border: none;
  padding: 0 0 12px 0;
  font-size: 14px;
  font-weight: 500;
  color: #5E6C84;
  cursor: pointer;
  position: relative;
}

.tab-btn:hover {
  color: #172B4D;
}

.tab-btn.active {
  color: #0052CC;
}

.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background-color: #0052CC;
}

.page-content {
  display: flex;
  flex: 1;
  padding: 32px 40px;
  gap: 40px;
}

/* MAIN COLUMN */
.main-column {
  flex: 1;
  max-width: 800px;
}

/* Banner */
.info-banner {
  display: flex;
  gap: 16px;
  padding: 16px;
  background-color: #E6FCFF;
  border: 1px solid #B3F5FF;
  border-radius: 3px;
  margin-bottom: 40px;
}

.banner-icon {
  flex-shrink: 0;
}

.blue-icon-bg {
  width: 24px;
  height: 24px;
  background-color: #0052CC;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 12px;
}

.banner-content h3 {
  margin: 0 0 8px 0;
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
}

.banner-content p {
  margin: 0 0 16px 0;
  font-size: 14px;
  color: #172B4D;
  line-height: 1.5;
}

.banner-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
}

.primary-btn:hover {
  background-color: #0047B3;
}

.text-btn {
  background: transparent;
  border: none;
  color: #0052CC;
  font-size: 14px;
  cursor: pointer;
}

.text-btn:hover {
  text-decoration: underline;
}

/* Timeline */
.timeline-header {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 24px;
  margin-bottom: 24px;
}

.timeline-title {
  font-size: 20px;
  font-weight: 500;
  color: #5E6C84;
  margin: 0;
}

.icon-btn {
  background: transparent;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #5E6C84;
  cursor: pointer;
}

.icon-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background: #FAFBFC;
}

.icon-btn:not(:disabled):hover {
  background: #F4F5F7;
}

.stats-subtitle {
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
  margin: 0 0 16px 0;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 40px;
}

.stat-card {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 12px 16px;
  display: flex;
  gap: 12px;
  background: #FFFFFF;
}

.stat-number {
  font-size: 24px;
  font-weight: 500;
  line-height: 1;
}

.stat-number.green, .stat-label.green { color: #00875A; }
.stat-number.orange, .stat-label.orange { color: #FF991F; }
.stat-number.red, .stat-label.red { color: #DE350B; }
.stat-number.gray, .stat-label.gray { color: #5E6C84; }
.stat-number.blue, .stat-label.blue { color: #0052CC; }

.stat-info {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.stat-label {
  font-size: 12px;
  font-weight: 600;
  color: #172B4D;
  margin-bottom: 4px;
}

.stat-desc {
  font-size: 11px;
  color: #5E6C84;
}

/* Tracked Items */
.tracked-items-list {
  display: flex;
  flex-direction: column;
}

.tracked-item {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.item-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.item-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.item-icon {
  font-size: 16px;
}

.item-name-col {
  display: flex;
  flex-direction: column;
}

.item-type-label {
  font-size: 11px;
  color: #5E6C84;
}

.item-name {
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
}

.item-card-inner {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-left: 24px;
}

.item-body {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.item-user {
  display: flex;
  align-items: center;
  gap: 12px;
}

.item-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: #00875A;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.item-user-info {
  display: flex;
  flex-direction: column;
}

.item-user-name {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
}

.item-time {
  font-size: 12px;
  color: #5E6C84;
}

.item-status-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 3px;
}

.green-badge {
  background-color: #E3FCEF;
  color: #006644;
}

.item-message {
  font-size: 14px;
  color: #172B4D;
  background-color: #FAFBFC;
  border: 1px solid #DFE1E6;
  padding: 12px;
  border-radius: 3px;
}

.old-status {
  text-decoration: line-through;
  color: #5E6C84;
  font-size: 12px;
  font-weight: 600;
  background: #DFE1E6;
  padding: 2px 4px;
  border-radius: 2px;
}

.new-status {
  color: #172B4D;
  font-size: 12px;
  font-weight: 600;
  background: #DFE1E6;
  padding: 2px 4px;
  border-radius: 2px;
}

.item-footer {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #5E6C84;
}

.footer-btn {
  background: transparent;
  border: none;
  color: #5E6C84;
  font-size: 12px;
  cursor: pointer;
  padding: 0;
}

.footer-btn:hover {
  text-decoration: underline;
}

.dot-separator {
  color: #DFE1E6;
}

.reactions {
  display: flex;
  gap: 4px;
  margin-left: 8px;
}

.reaction-btn {
  background: #FAFBFC;
  border: 1px solid #DFE1E6;
  border-radius: 12px;
  padding: 2px 6px;
  font-size: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.reaction-btn:hover {
  background: #EBECF0;
}

.add-reaction {
  border-style: dashed;
  color: #5E6C84;
}

/* SIDEBAR COLUMN */
.sidebar-column {
  width: 300px;
  flex-shrink: 0;
  border-left: 1px solid #DFE1E6;
  padding-left: 44px;
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.sidebar-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.sidebar-section h3 {
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
  margin: 0;
}

.sidebar-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.sidebar-item-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.sidebar-item-icon {
  font-size: 16px;
}

.sidebar-item-name {
  font-size: 14px;
  color: #172B4D;
}

.sidebar-item-meta {
  font-size: 12px;
  color: #5E6C84;
}

.sidebar-item-badge {
  width: 16px;
  height: 16px;
  background-color: #00875A;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}
</style>
