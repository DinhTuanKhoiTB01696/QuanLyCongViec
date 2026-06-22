<template>
  <div class="mention-list">
    <template v-if="items.length">
      <button
        class="mention-item"
        :class="{ 'is-selected': index === selectedIndex }"
        v-for="(item, index) in items"
        :key="index"
        @click="selectItem(index)"
      >
        <div class="mention-avatar" :style="{ backgroundColor: getAvatarColor(item.email, item.fullName) }">{{ getAvatarInitials(item.email, item.fullName) }}</div>
        <div class="mention-info">
           <div class="mention-name">{{ item.fullName || item.email }}</div>
           <div class="mention-email" v-if="item.email && item.fullName">{{ item.email }}</div>
        </div>
      </button>
    </template>
    <div class="mention-item empty" v-else>
      Không tìm thấy kết quả
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { getAvatarInitials, getAvatarColor } from '@/utils/avatar'

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
    props.command({ id: item.id || item.email, label: item.fullName || item.email })
  }
}

defineExpose({
  onKeyDown,
})
</script>

<style scoped>
.mention-list {
  background: white;
  border-radius: 3px;
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.25);
  border: 1px solid #DFE1E6;
  overflow: hidden;
  padding: 4px 0;
  min-width: 200px;
}

.mention-item {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  text-align: left;
  background: transparent;
  border: none;
  padding: 8px 12px;
  cursor: pointer;
  transition: background 0.1s;
}

.mention-item:hover,
.mention-item.is-selected {
  background: #FAFBFC;
}

.mention-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 10px;
  font-weight: 600;
  flex-shrink: 0;
}

.mention-info {
  display: flex;
  flex-direction: column;
}

.mention-name {
  font-size: 14px;
  color: #172B4D;
  font-weight: 500;
}

.mention-email {
  font-size: 12px;
  color: #5E6C84;
}

.mention-item.empty {
  color: #5E6C84;
  font-style: italic;
  font-size: 14px;
}
</style>
