<template>
  <div class="sprinta-card">
    <div v-if="$slots.header" class="sprinta-card-header">
      <slot name="header" />
    </div>
    <div class="sprinta-card-body">
      <slot />
    </div>
    <div v-if="$slots.footer" class="sprinta-card-footer">
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup>
</script>

<style scoped>
.sprinta-card {
  background-color: #FFFFFF;
  border-radius: var(--sp-card-radius, 8px);
  box-shadow: var(--sp-shadow-card, 0 1px 2px rgba(9, 30, 66, 0.25));
  border: 1px solid var(--sp-border-color, #DFE1E6);
  display: flex;
  flex-direction: column;
}

.sprinta-card-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--sp-border-color, #DFE1E6);
  font-weight: 500;
  color: var(--sp-text-primary, #172B4D);
}

.sprinta-card-body {
  padding: 20px;
  flex: 1;
}

.sprinta-card-footer {
  padding: 16px 20px;
  border-top: 1px solid var(--sp-border-color, #DFE1E6);
  background-color: #FAFBFC;
  border-radius: 0 0 var(--sp-card-radius, 8px) var(--sp-card-radius, 8px);
}
</style>
