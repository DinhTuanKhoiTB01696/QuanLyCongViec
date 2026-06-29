<template>
  <div class="mention-list" v-if="items.length">
    <div
      v-for="(item, index) in items"
      :key="index"
      class="mention-item"
      :class="{ 'is-selected': index === selectedIndex }"
      @click="selectItem(index)"
      @mouseenter="selectedIndex = index"
    >
      <UserAvatar :user="item" :size="24" :fontSize="10" />
      <span class="mention-name">{{ item.fullName || item.name || item.email }}</span>
    </div>
  </div>
  <div class="mention-list mention-empty" v-else>
    Không tìm thấy người dùng
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import UserAvatar from '@/components/common/UserAvatar.vue'

const props = defineProps({
  items: {
    type: Array,
    required: true,
  },
  command: {
    type: Function,
    required: true,
  },
})

const selectedIndex = ref(0)

watch(() => props.items, () => {
  selectedIndex.value = 0
})

const onKeyDown = ({ event }) => {
  if (event.key === 'ArrowUp') {
    upHandler()
    return true
  }

  if (event.key === 'ArrowDown') {
    downHandler()
    return true
  }

  if (event.key === 'Enter') {
    enterHandler()
    return true
  }

  return false
}

const upHandler = () => {
  selectedIndex.value = ((selectedIndex.value + props.items.length) - 1) % props.items.length
}

const downHandler = () => {
  selectedIndex.value = (selectedIndex.value + 1) % props.items.length
}

const enterHandler = () => {
  selectItem(selectedIndex.value)
}

const selectItem = (index) => {
  const item = props.items[index]
  if (item) {
    props.command({ id: item.id, label: item.fullName || item.name || item.email, color: '#0052CC' })
  }
}

// Expose onKeyDown to parent (tippy plugin)
defineExpose({
  onKeyDown
})
</script>

<style scoped>
.mention-list {
  background: white;
  border-radius: 4px;
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.25);
  border: 1px solid #DFE1E6;
  padding: 4px 0;
  max-height: 250px;
  overflow-y: auto;
  min-width: 200px;
}

.mention-empty {
  padding: 8px 16px;
  color: #5E6C84;
  font-size: 14px;
}

.mention-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  cursor: pointer;
  transition: background-color 0.1s ease;
}

.mention-item.is-selected,
.mention-item:hover {
  background-color: #FAFBFC;
}

.mention-name {
  font-size: 14px;
  color: #172B4D;
  font-weight: 500;
}
</style>
