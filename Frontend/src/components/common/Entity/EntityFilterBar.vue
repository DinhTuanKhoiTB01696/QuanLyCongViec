<template>
  <div class="sprinta-filter-bar">
    <button 
      v-for="filter in filters" 
      :key="filter.id"
      class="sprinta-filter-chip"
      :class="{ 'is-active': activeFilters.includes(filter.id) }"
      @click="toggleFilter(filter.id)"
    >
      <component v-if="filter.icon" :is="filter.icon" class="w-4 h-4"></component>
      {{ filter.label }}
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  filters: {
    type: Array,
    required: true // [{ id: 'status', label: 'Trạng thái', icon: Object }]
  },
  modelValue: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const activeFilters = ref([...props.modelValue])

const toggleFilter = (id) => {
  const index = activeFilters.value.indexOf(id)
  if (index === -1) {
    activeFilters.value.push(id)
  } else {
    activeFilters.value.splice(index, 1)
  }
  emit('update:modelValue', activeFilters.value)
  emit('change', activeFilters.value)
}
</script>

<style scoped>
.sprinta-filter-chip.is-active {
  background: #E6FCFF;
  color: #0052CC;
  border-color: #0052CC;
}
</style>
