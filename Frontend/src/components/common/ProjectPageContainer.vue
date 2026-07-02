<template>
  <div class="project-page-container">
    <div class="project-page-inner">
      <slot></slot>
    </div>
  </div>
</template>

<style scoped>
.project-page-container {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--bg-primary, var(--color-bg));
}
.project-page-inner {
  width: 100%;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  box-sizing: border-box;
}

@media (max-width: 768px) {
  .project-page-inner {
    padding: 24px 16px;
    gap: 24px;
  }
}
</style>
