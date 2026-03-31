<template>
  <el-dropdown trigger="click" popper-class="notifications-dropdown-popper">
    <div class="nav-icon notification-trigger">
      <el-badge :value="0" class="notification-badge" :hidden="true">
        <i class="fa-solid fa-bell"></i>
      </el-badge>
    </div>
    <template #dropdown>
      <div class="jira-notifications-menu">
        <div class="notif-header">
          <h2 class="notif-title">Thông báo</h2>
          <div class="header-actions">
            <span class="unread-toggle-label">Chỉ hiện chưa đọc</span>
            <el-switch v-model="onlyUnread" size="small" />
            <i class="fa-solid fa-arrow-up-right-from-square icon-btn"></i>
            <i class="fa-solid fa-ellipsis icon-btn"></i>
          </div>
        </div>

        <div class="notif-tabs">
          <div class="notif-tab active">Trực tiếp</div>
          <div class="notif-tab">Đang theo dõi</div>
        </div>

        <div class="notif-scroll-area">
          <!-- Notifications are empty by default -->

          <!-- Empty State -->
          <div class="notif-empty-state">
            <div class="empty-icon">
              <i class="fa-solid fa-flag"></i>
            </div>
            <p>Đó là tất cả thông báo của bạn từ 30 ngày qua.</p>
          </div>
        </div>

        <div class="notif-footer">
          <div class="shortcuts-info">
            Nhấn <span class="key-badge"><i class="fa-solid fa-arrow-down"></i></span> <span class="key-badge"><i class="fa-solid fa-arrow-up"></i></span> để di chuyển qua các thông báo.
          </div>
          <button class="btn-all-shortcuts">Xem tất cả phím tắt</button>
        </div>
      </div>
    </template>
  </el-dropdown>
</template>

<script setup>
import { ref } from 'vue'

const onlyUnread = ref(false)
</script>

<style scoped>
.jira-notifications-menu {
  width: 480px;
  background-color: var(--bg-card);
  color: var(--text-primary);
  display: flex;
  flex-direction: column;
  max-height: 85vh;
}

.notif-header {
  padding: 16px 20px 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.notif-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.unread-toggle-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.icon-btn {
  color: var(--text-secondary);
  font-size: 14px;
  cursor: pointer;
  padding: 4px;
}
.icon-btn:hover { color: var(--text-primary); }

.notif-tabs {
  display: flex;
  padding: 0 20px;
  border-bottom: 2px solid var(--border-color);
  gap: 24px;
}

.notif-tab {
  padding: 8px 0 12px;
  font-size: 14px;
  color: var(--text-secondary);
  cursor: pointer;
  position: relative;
}

.notif-tab.active {
  color: #3b82f6;
  font-weight: 600;
}

.notif-tab.active::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  right: 0;
  height: 2px;
  background-color: #3b82f6;
}

.notif-scroll-area {
  flex: 1;
  overflow-y: auto;
  min-height: 400px;
}

.notif-section {
  padding: 16px 0 0;
}

.section-label {
  padding: 0 20px 8px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-secondary);
  margin: 0;
}

.notif-item {
  display: flex;
  padding: 12px 20px;
  gap: 16px;
  cursor: pointer;
  transition: background-color 0.2s;
  position: relative;
}

.notif-item:hover {
  background-color: var(--hover-bg);
}

.notif-item.unread {
  background-color: rgba(7, 71, 166, 0.05);
}

.notif-avatar {
  width: 32px;
  height: 32px;
  background-color: #0052cc;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: white;
}

.notif-content {
  flex: 1;
}

.notif-text {
  font-size: 14px;
  line-height: 1.5;
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.user-name {
  font-weight: 600;
  color: var(--text-primary);
}

.time-ago {
  color: var(--text-muted);
  font-size: 12px;
  margin-left: 4px;
}

.notif-context {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-muted);
}

.unread-dot {
  width: 6px;
  height: 6px;
  background-color: #3b82f6;
  border-radius: 50%;
  margin-top: 8px;
}

.notif-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  text-align: center;
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 32px;
  color: var(--text-muted);
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty-icon i { transform: rotate(-10deg); }

.notif-empty-state p {
  font-size: 14px;
  max-width: 240px;
  line-height: 1.6;
}

.notif-footer {
  padding: 16px 20px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.shortcuts-info {
  font-size: 13px;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 4px;
}

.key-badge {
  background-color: var(--hover-bg);
  border: 1px solid var(--border-color);
  border-radius: 3px;
  padding: 1px 6px;
  font-size: 11px;
}

.btn-all-shortcuts {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
}
.btn-all-shortcuts:hover { background: var(--hover-bg); }

/* Navbar Icon Style */
.nav-icon {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.nav-icon i { font-size: 18px; }
.nav-icon:hover { background-color: var(--hover-bg); color: var(--text-primary); }

.notification-badge :deep(.el-badge__content) {
  background-color: #f87171;
  border: none;
  font-size: 9px;
  height: 14px;
  line-height: 14px;
  padding: 0 4px;
  top: 4px;
}
</style>

<style>
.el-popper.notifications-dropdown-popper {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-color) !important;
  padding: 0 !important;
  border-radius: 8px !important;
  box-shadow: 0 12px 32px rgba(0,0,0,0.2) !important;
}
</style>
