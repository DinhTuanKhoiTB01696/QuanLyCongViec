<template>
  <div class="project-loading-state">
    <i class="fa-solid fa-spinner fa-spin loading-icon"></i>
    <p class="loading-text">{{ text }}</p>
  </div>
</template>

<script setup>
defineProps({
  text: { type: String, default: 'Loading...' }
})
</script>

<style scoped>
.project-loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 64px 0;
  width: 100%;
}
.loading-icon {
  font-size: 32px;
  color: var(--color-accent, #0c66e4);
  margin-bottom: 16px;
}
.loading-text {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-secondary, #42526e);
  margin: 0;
}
</style>
