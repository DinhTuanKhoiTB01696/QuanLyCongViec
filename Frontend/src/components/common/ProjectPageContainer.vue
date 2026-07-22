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
  background: transparent;
  min-height: 0;
  overflow: hidden;
}
.project-page-inner {
  width: 100%;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
  box-sizing: border-box;
  min-height: 100%;
  flex: 1;
  overflow: auto;
  overflow-anchor: none;
  overscroll-behavior: contain;
}

@media (max-width: 768px) {
  .project-page-inner {
    gap: 12px;
  }
}
</style>
