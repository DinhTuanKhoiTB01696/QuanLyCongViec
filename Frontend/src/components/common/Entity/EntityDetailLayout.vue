<template>
  <div class="sprinta-page">
    <slot name="header"></slot>
    <div class="sprinta-detail">
      <div class="sprinta-detail-main">
        <slot name="main"></slot>
      </div>
      <div class="sprinta-right-panel" v-if="$slots.sidebar">
        <slot name="sidebar"></slot>
      </div>
    </div>
  </div>
</template>

<script setup>
// EntityDetailLayout wrapper
</script>
