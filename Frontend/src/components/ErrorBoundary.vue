<script setup>
import { ref, onErrorCaptured } from 'vue'

const error = ref(null)

onErrorCaptured((err) => {
  error.value = err
  console.error('Caught by ErrorBoundary:', err)
  return false // prevent the error from propagating further
})

const reload = () => {
  window.location.reload()
}
</script>

<template>
  <div v-if="error" class="error-boundary-container">
    <div class="error-content">
      <h2><i class="fa-solid fa-triangle-exclamation"></i> Có lỗi hiển thị giao diện</h2>
      <p class="error-msg">{{ error.message }}</p>
      <button class="reload-btn" @click="reload">Tải lại trang</button>
    </div>
  </div>
  <slot v-else></slot>
</template>

<style scoped>
.error-boundary-container {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
  width: 100%;
  background-color: var(--bg-layout, var(--color-bg));
  color: var(--text-primary, #e4e4e7);
}
.error-content {
  background: var(--bg-card, var(--color-surface));
  padding: 30px;
  border-radius: 8px;
  border: 1px solid var(--border-color, var(--color-border));
  max-width: 500px;
  text-align: center;
}
.error-msg {
  color: #ef4444;
  margin: 16px 0;
  font-family: monospace;
  background: rgba(239, 68, 68, 0.1);
  padding: 10px;
  border-radius: 4px;
  word-break: break-all;
}
.reload-btn {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
}
.reload-btn:hover {
  background: #2563eb;
}
</style>



