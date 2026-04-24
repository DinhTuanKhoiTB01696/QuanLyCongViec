<template>
  <div class="admin-layout">
    <AdminSidebar />
    <main class="admin-content">
      <div class="content-wrapper">
        <slot></slot>
      </div>
    </main>
  </div>
</template>

<script setup>
import AdminSidebar from './AdminSidebar.vue'
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--color-bg);
  color: var(--color-text-primary);
  font-family: 'Inter', system-ui, sans-serif;
}

.admin-content {
  flex: 1;
  background-color: var(--color-bg);
  padding: 32px 48px;
  overflow-y: auto;
  height: 100vh;
}

.content-wrapper {
  max-width: 1200px;
  margin: 0 auto;
}
</style><style>
.admin-layout .primary-btn,
.admin-layout .secondary-btn,
.admin-layout .btn-ghost,
.admin-layout .neutral-btn,
.admin-layout .filter-button,
.admin-layout .el-button {
  border-radius: 8px !important;
}

.admin-layout .el-input__wrapper,
.admin-layout .el-textarea__inner,
.admin-layout .el-select__wrapper,
.admin-layout input[type="text"],
.admin-layout input[type="color"] {
  border-radius: 8px !important;
}

.admin-layout .status-row {
  border-radius: 8px !important;
}

.admin-layout .settings-card {
  border-radius: 12px !important;
}
</style>
