<template>
  <div class="sprinta-tabs">
    <button 
      v-for="tab in tabs" 
      :key="tab.value"
      class="sprinta-tab-btn"
      :class="{ 'is-active': modelValue === tab.value }"
      @click="$emit('update:modelValue', tab.value)"
    >
      {{ tab.label }}
    </button>
  </div>
</template>

<script setup>
defineProps({
  tabs: {
    type: Array,
    required: true // Array of { label: 'Tab Name', value: 'tab_id' }
  },
  modelValue: {
    type: String,
    required: true
  }
})

defineEmits(['update:modelValue'])
</script>
