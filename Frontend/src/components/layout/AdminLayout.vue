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
</style>




