<template>
  <div class="recent-page">
    <header class="page-header">
      <h1>Gần đây</h1>
      
      <div class="tabs">
        <button class="tab-btn active">Làm việc trên</button>
        <button class="tab-btn">Đã xem</button>
      </div>
    </header>

    <div class="page-content">
      <div class="filter-container">
        <div class="search-input-wrapper">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input type="text" placeholder="Lọc theo tiêu đề" class="search-input" />
        </div>
      </div>

      <div class="audit-list">
        <div class="time-group">
          <h3 class="time-label">Hôm nay</h3>
          <div class="audit-item">
            <div class="item-icon yellow-bg">😎</div>
            <div class="item-details">
              <div class="item-title">e</div>
              <div class="item-path">Dự án • Dự án • TUA46-8</div>
            </div>
            <div class="item-meta">
              <span class="status-badge pending">PENDING</span>
              <span class="time-ago">10 phút trước</span>
            </div>
          </div>
        </div>

        <div class="time-group">
          <h3 class="time-label">Tuần này</h3>
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
              <div class="item-path">Task • Jira • DTN-12 • Dự Án Tốt Nghiệp</div>
            </div>
            <div class="item-meta">
              <span class="status-badge todo">TO DO</span>
              <span class="time-ago">3 ngày trước</span>
            </div>
          </div>
          <div class="audit-item">
            <div class="item-icon light-blue square"><i class="fa-solid fa-check"></i></div>
            <div class="item-details">
              <div class="item-title">qdw</div>
              <div class="item-path">Task • Jira • DTN-11 • Dự Án Tốt Nghiệp</div>
            </div>
            <div class="item-meta">
              <span class="status-badge todo">TO DO</span>
              <span class="time-ago">3 ngày trước</span>
            </div>
          </div>
          <div class="audit-item">
            <div class="item-icon green-bg square"><i class="fa-solid fa-code-branch"></i></div>
            <div class="item-details">
              <div class="item-title">Làm trang quản lý riêng cho space</div>
              <div class="item-path">Subtask • Jira • DTN-9 • Dự Án Tốt Nghiệp</div>
            </div>
            <div class="item-meta">
              <span class="status-badge done">DONE</span>
              <span class="time-ago">5 ngày trước</span>
            </div>
          </div>
          <div class="audit-item">
            <div class="item-icon light-blue square"><i class="fa-solid fa-code-branch"></i></div>
            <div class="item-details">
              <div class="item-title">Làm trang quản lý system</div>
              <div class="item-path">Subtask • Jira • DTN-10 • Dự Án Tốt Nghiệp</div>
            </div>
            <div class="item-meta">
              <span class="status-badge in-progress">IN PROGRESS</span>
              <span class="time-ago">5 ngày trước</span>
            </div>
          </div>
          <div class="audit-item">
            <div class="item-icon light-blue square"><i class="fa-solid fa-check"></i></div>
            <div class="item-details">
              <div class="item-title">Thiết Kế giao diện Trang Admin</div>
              <div class="item-path">Task • Jira • DTN-8 • Dự Án Tốt Nghiệp</div>
            </div>
            <div class="item-meta">
              <div class="assignee-avatar">AH</div>
              <span class="status-badge in-progress">IN PROGRESS</span>
              <span class="time-ago">5 ngày trước</span>
            </div>
          </div>
        </div>

        <div class="time-group">
          <h3 class="time-label">Cũ hơn</h3>
          <div class="audit-item">
            <div class="item-icon yellow-bg">😎</div>
            <div class="item-details">
              <div class="item-title">##É</div>
              <div class="item-path">Dự án • Dự án • TUA46-3</div>
            </div>
            <div class="item-meta">
              <span class="status-badge completed">COMPLETED <i class="fa-solid fa-check"></i></span>
              <span class="time-ago">7 ngày trước</span>
            </div>
          </div>
          <div class="audit-item">
            <div class="item-icon yellow-bg">😃</div>
            <div class="item-details">
              <div class="item-title">ưeq</div>
              <div class="item-path">Dự án • Dự án • TUA46-4</div>
            </div>
            <div class="item-meta">
              <span class="status-badge completed">COMPLETED <i class="fa-solid fa-check"></i></span>
              <span class="time-ago">7 ngày trước</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = i18nStore.t
</script>

<style scoped>
.recent-page {
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

.page-header h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 24px 0;
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
  padding: 24px 40px;
  max-width: 1000px;
}

.filter-container {
  margin-bottom: 24px;
}

.search-input-wrapper {
  position: relative;
  width: 240px;
}

.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
  font-size: 14px;
}

.search-input {
  width: 100%;
  padding: 6px 12px 6px 44px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  background-color: #FFFFFF;
  color: #091E42;
  font-size: 14px;
  outline: none;
  transition: background-color 0.2s, border-color 0.2s;
  box-sizing: border-box;
}

.search-input:hover {
  background-color: #FAFBFC;
}

.search-input:focus {
  background-color: #FFFFFF;
  border-color: #4C9AFF;
}

.audit-list {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.time-group {
  display: flex;
  flex-direction: column;
}

.time-label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  margin: 0 0 12px 0;
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

.yellow-bg { background: #FFFAE6; font-size: 16px; }
.light-blue { background: #E6FCFF; color: #00B8D9; }
.green-bg { background: #E3FCEF; color: #36B37E; }

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

.assignee-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #172B4D;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.status-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 3px;
}

.status-badge.pending,
.status-badge.draft,
.status-badge.todo { background: #DFE1E6; color: #42526E; }
.status-badge.in-progress { background: #DEEBFF; color: #0052CC; }
.status-badge.done { background: #E3FCEF; color: #006644; }
.status-badge.completed { background: #DFE1E6; color: #42526E; }

.time-ago {
  font-size: 12px;
  color: #5E6C84;
  min-width: 80px;
  text-align: right;
}
</style>
