<template>
  <div class="sprinta-search-bar relative flex items-center">
    <Search class="absolute left-3 w-4 h-4 text-muted pointer-events-none search-icon" />
    <input
      type="text"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
      :placeholder="placeholder"
      class="w-full bg-surface border-2 border-border text-primary rounded-md pl-9 pr-8 h-9 text-sm focus:outline-none focus:border-accent transition-colors sprinta-search-input"
    />
    <button 
      v-if="modelValue" 
      @click="$emit('update:modelValue', '')"
      class="absolute right-2 text-muted hover:text-primary p-1 rounded-sm flex items-center justify-center transition-colors clear-btn"
    >
      <X class="w-3.5 h-3.5" />
    </button>
  </div>
</template>

<script setup>
import { Search, X } from 'lucide-vue-next'

defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Search...'
  }
})

defineEmits(['update:modelValue'])
</script>

<style scoped>
.sprinta-search-input {
  background-color: var(--color-surface);
  border-color: var(--color-border);
  color: var(--color-text-primary);
  border-radius: var(--radius-sm);
}

.sprinta-search-input::placeholder {
  color: var(--color-text-muted);
}

.sprinta-search-input:focus {
  border-color: var(--color-accent);
}

.sprinta-search-input:focus + .clear-btn {
  color: var(--color-text-secondary);
}
</style>
