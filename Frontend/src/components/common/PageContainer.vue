<template>
  <div class="sc-page-container">
    <slot></slot>
  </div>
</template>

<style scoped>
.sc-page-container {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  box-sizing: border-box;
}
@media (max-width: 768px) {
  .sc-page-container {
    padding: 16px;
    gap: 16px;
  }
}
</style>
