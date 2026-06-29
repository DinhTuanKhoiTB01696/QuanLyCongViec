<template>
  <div class="dropdown-filter-wrapper" ref="wrapperRef">
    <button class="filter-chip" :class="{ 'active': isOpen, 'has-value': modelValue }" @click="toggleOpen">
      <slot name="icon"></slot>
      <span class="ms-1">{{ displayLabel }}</span>
      <i class="fa-solid fa-chevron-down ms-2 text-xs"></i>
    </button>
    
    <div v-if="isOpen" class="dropdown-popup">
      <div class="search-box-wrapper" v-if="searchable">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" v-model="searchQuery" :placeholder="searchPlaceholder" class="search-input" @click.stop />
      </div>
      
      <div class="options-list">
        <div class="option-item" :class="{ selected: modelValue === '' }" @click="selectOption('')">
          Tất cả
        </div>
        <div 
          class="option-item" 
          v-for="opt in filteredOptions" 
          :key="opt.value" 
          :class="{ selected: modelValue === opt.value }" 
          @click="selectOption(opt.value)"
        >
          {{ opt.label }}
        </div>
        <div v-if="filteredOptions.length === 0" class="no-results">
          Không tìm thấy kết quả
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'

const props = defineProps({
  label: { type: String, required: true },
  options: { type: Array, default: () => [] }, // Array of { label, value } or primitives
  modelValue: { type: [String, Number, Boolean], default: '' },
  searchable: { type: Boolean, default: true },
  searchPlaceholder: { type: String, default: 'Tìm kiếm...' }
})

const emit = defineEmits(['update:modelValue'])

const isOpen = ref(false)
const searchQuery = ref('')
const wrapperRef = ref(null)

const normalizedOptions = computed(() => {
  return props.options.map(opt => {
    if (typeof opt === 'object' && opt !== null) {
      return { label: String(opt.label || opt.name || opt.title || opt.fullName || opt.value), value: opt.id || opt.value || opt }
    }
    return { label: String(opt), value: opt }
  })
})

const filteredOptions = computed(() => {
  if (!searchQuery.value) return normalizedOptions.value
  const q = searchQuery.value.toLowerCase()
  return normalizedOptions.value.filter(opt => opt.label.toLowerCase().includes(q))
})

const displayLabel = computed(() => {
  if (props.modelValue === '') return props.label
  const selectedOpt = normalizedOptions.value.find(opt => opt.value === props.modelValue)
  return selectedOpt ? `${props.label}: ${selectedOpt.label}` : props.label
})

const toggleOpen = () => {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    searchQuery.value = ''
  }
}

const selectOption = (val) => {
  emit('update:modelValue', val)
  isOpen.value = false
}

const handleClickOutside = (event) => {
  if (wrapperRef.value && !wrapperRef.value.contains(event.target)) {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.dropdown-filter-wrapper {
  position: relative;
  display: inline-block;
}

.filter-chip {
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #42526E;
  display: flex;
  align-items: center;
  cursor: pointer;
  transition: background 0.2s;
  white-space: nowrap;
}

.filter-chip:hover, .filter-chip.active {
  background: #FAFBFC;
  border-color: #4C9AFF;
}

.filter-chip.has-value {
  background: #E6FCFF;
  color: #0052CC;
  border-color: #4C9AFF;
}

.dropdown-popup {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 4px;
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.15);
  min-width: 220px;
  z-index: 1000;
  display: flex;
  flex-direction: column;
}

.search-box-wrapper {
  position: relative;
  padding: 8px;
  border-bottom: 1px solid #DFE1E6;
}

.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
  font-size: 14px;
}

.search-input {
  width: 100%;
  padding: 6px 8px 6px 32px;
  border: 2px solid transparent;
  background: #FAFBFC;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  outline: none;
  transition: all 0.2s;
  box-sizing: border-box;
}

.search-input:focus {
  background: #FFFFFF;
  border-color: #4C9AFF;
}

.options-list {
  max-height: 250px;
  overflow-y: auto;
  padding: 4px 0;
}

.option-item {
  padding: 8px 12px;
  font-size: 14px;
  color: #172B4D;
  cursor: pointer;
  transition: background 0.15s;
}

.option-item:hover {
  background: #FAFBFC;
}

.option-item.selected {
  background: #E6FCFF;
  color: #0052CC;
  font-weight: 500;
}

.no-results {
  padding: 12px;
  font-size: 13px;
  color: #6B778C;
  text-align: center;
}
</style>
