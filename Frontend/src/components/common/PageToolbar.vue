<template>
  <div class="sc-page-toolbar">
    <div class="sc-pt-left">
      <slot name="search"></slot>
      <slot name="filters"></slot>
    </div>
    <div class="sc-pt-right">
      <slot name="actions"></slot>
    </div>
  </div>
</template>

<style scoped>
.sc-page-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
  padding: 8px 0;
  border-bottom: 1px solid var(--color-border, #dfe1e6);
}
.sc-pt-left {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  flex: 1;
}
.sc-pt-right {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}
</style>
