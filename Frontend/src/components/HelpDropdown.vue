<template>
  <el-dropdown trigger="click" popper-class="help-dropdown-popper global-help-popper">
    <div class="help-trigger-btn">
      <i class="fa-solid fa-circle-question"></i>
    </div>
    <template #dropdown>
      <el-dropdown-menu class="dark-help-menu">
        <div class="help-header">
          <h3>Trợ giúp</h3>
          <i class="fa-solid fa-xmark close-btn"></i>
        </div>
        <el-dropdown-item><i class="fa-solid fa-lightbulb"></i> Tìm hiểu cập nhật mới</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-file-lines"></i> Đọc về điều hướng mới</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-book"></i> Duyệt tài liệu hướng dẫn</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-desktop"></i> Học kỹ năng điều hành dự án</el-dropdown-item>
        <el-dropdown-item divided><i class="fa-solid fa-comment"></i> Hỏi cộng đồng trực tuyến</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-triangle-exclamation"></i> Liên hệ hỗ trợ</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-comments"></i> Phản hồi về SprintA</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-keyboard"></i> Phím tắt</el-dropdown-item>
        <el-dropdown-item><i class="fa-solid fa-paper-plane"></i> Tải ứng dụng Mobile</el-dropdown-item>
        <div class="help-footer">
          <a href="#">Giới thiệu</a>
          <a href="#">Điều khoản</a>
          <a href="#">Bảo mật</a>
        </div>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup>
// No extra logic needed for now
</script>

<style scoped>
.help-trigger-btn {
  color: var(--text-secondary);
  font-size: 18px;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  transition: all 0.2s;
}

.help-trigger-btn:hover {
  color: var(--text-primary);
  background-color: var(--hover-bg);
}

.dark-help-menu {
  background: var(--bg-card);
  border: none;
  width: 320px;
  padding: 0;
}

.help-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  color: var(--text-primary);
  border-bottom: 1px solid var(--border-color);
}

.help-header h3 {
  font-size: 14px;
  font-weight: 600;
  margin: 0;
}

.close-btn {
  cursor: pointer;
  font-size: 14px;
  color: var(--text-secondary);
}

.close-btn:hover {
  color: var(--text-primary);
}

:deep(.el-dropdown-menu__item) {
  color: var(--text-secondary) !important;
  padding: 10px 20px !important;
  font-size: 14px !important;
  display: flex;
  align-items: center;
  gap: 12px;
}

:deep(.el-dropdown-menu__item i) {
  font-size: 16px;
  width: 16px;
  text-align: center;
}

:deep(.el-dropdown-menu__item:hover) {
  background-color: var(--hover-bg) !important;
  color: var(--text-primary) !important;
}

:deep(.el-dropdown-menu__item--divided) {
  border-top-color: var(--border-color) !important;
}

.help-footer {
  display: flex;
  justify-content: space-evenly;
  padding: 16px 20px;
  background-color: var(--bg-card);
  border-top: 1px solid var(--border-color);
  margin-top: 8px;
}

.help-footer a {
  color: var(--text-secondary);
  font-size: 12px;
  text-decoration: none;
}

.help-footer a:hover {
  color: var(--text-primary);
  text-decoration: underline;
}
</style>

<style>
.el-popper.help-dropdown-popper.global-help-popper {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-color) !important;
  border-radius: 8px !important;
  padding: 0 !important;
  box-shadow: 0 8px 16px rgba(0,0,0,0.2) !important;
}

.el-popper.help-dropdown-popper.global-help-popper .el-popper__arrow::before {
  background: var(--bg-card) !important;
  border-color: var(--border-color) !important;
}
</style>
