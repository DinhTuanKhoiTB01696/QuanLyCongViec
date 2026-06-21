<template>
  <div class="entity-link-picker-wrapper" style="position: relative;" ref="pickerWrapper">
    <button 
      class="sprinta-btn-secondary" 
      style="width: 100%; justify-content: flex-start; background: transparent; border: 2px solid transparent; padding: 4px 8px; font-weight: 400; height: auto; min-height: 32px;"
      @click="togglePopover"
      :class="{ 'is-active': isOpen }"
    >
      <span v-if="loading" style="color: #6B778C;">Đang tải...</span>
      <span v-else-if="!selectedIds || selectedIds.length === 0" style="color: #6B778C;">{{ placeholder || 'Chọn mục...' }}</span>
      <span v-else style="color: #172B4D;">Đã chọn {{ selectedIds.length }} mục</span>
    </button>

    <div v-if="isOpen" class="sprinta-popover" style="top: 100%; left: 0; width: 300px; margin-top: 4px;">
      <div style="padding: 12px; border-bottom: 1px solid #DFE1E6;">
        <input 
          type="text" 
          v-model="searchQuery" 
          :placeholder="searchPlaceholder || 'Tìm kiếm...'" 
          style="width: 100%; padding: 6px 12px; border: 2px solid #DFE1E6; border-radius: 3px; outline: none; font-size: 14px; color: #172B4D; box-sizing: border-box;"
        />
      </div>
      
      <div style="max-height: 250px; overflow-y: auto; padding: 8px 0;">
        <div v-if="filteredItems.length === 0" style="padding: 16px; text-align: center; color: #6B778C; font-size: 13px;">
          Không tìm thấy kết quả
        </div>
        <div 
          v-else
          v-for="item in filteredItems" 
          :key="item.id"
          @click="selectItem(item)"
          style="padding: 8px 16px; cursor: pointer; display: flex; align-items: center; justify-content: space-between; font-size: 14px; color: #172B4D;"
          onmouseover="this.style.backgroundColor='#FAFBFC'"
          onmouseout="this.style.backgroundColor='transparent'"
        >
          <div style="display: flex; align-items: center; gap: 8px;">
            <div 
              style="width: 24px; height: 24px; border-radius: 3px; background-color: #0052CC; color: white; display: flex; align-items: center; justify-content: center; font-size: 10px; font-weight: bold;"
            >
              {{ item.avatar || item.icon || (item.name ? item.name.charAt(0) : 'X') }}
            </div>
            <span>{{ item.name || item.title || item.fullName }}</span>
          </div>
          <Check v-if="selectedIds && selectedIds.includes(item.id)" class="w-4 h-4" style="color: #0052CC;" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { Check } from 'lucide-vue-next'

const props = defineProps({
  entityType: {
    type: String,
    required: true // 'goal' | 'project' | 'team' | 'user'
  },
  items: {
    type: Array,
    default: () => []
  },
  selectedIds: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  placeholder: String,
  searchPlaceholder: String
})

const emit = defineEmits(['select'])

const isOpen = ref(false)
const searchQuery = ref('')
const pickerWrapper = ref(null)

const togglePopover = () => {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    searchQuery.value = ''
  }
}

const selectItem = (item) => {
  if (props.selectedIds.includes(item.id)) return // Prevent duplicate selection logic here or let parent handle, but UI shows feedback
  emit('select', item)
  isOpen.value = false
}

const filteredItems = computed(() => {
  if (!searchQuery.value) return props.items
  const query = searchQuery.value.toLowerCase()
  return props.items.filter(item => {
    const text = item.name || item.title || item.fullName || ''
    return text.toLowerCase().includes(query)
  })
})

const handleClickOutside = (event) => {
  if (pickerWrapper.value && !pickerWrapper.value.contains(event.target)) {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})
</script>

<style scoped>
.entity-link-picker-wrapper .sprinta-btn-secondary:hover {
  background-color: #FAFBFC !important;
}
</style>
