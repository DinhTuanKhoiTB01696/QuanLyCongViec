<template>
  <span class="sprinta-status-badge" :class="statusClass">
    <span v-if="dot" class="status-dot"></span>
    {{ status }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, required: true },
  dot: { type: Boolean, default: false }
})

const statusClass = computed(() => {
  const s = (props.status || '').toLowerCase()
  if (s.includes('đúng tiến độ') || s.includes('on track')) return 'status-success'
  if (s.includes('rủi ro') || s.includes('at risk')) return 'status-warning'
  if (s.includes('chậm') || s.includes('trễ') || s.includes('off track')) return 'status-danger'
  if (s.includes('hoàn tất') || s.includes('done')) return 'status-done'
  if (s.includes('lưu trữ') || s.includes('archived')) return 'status-archived'
  return 'status-pending'
})
</script>

<style scoped>
.sprinta-status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 2px 8px;
  border-radius: 3px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Success */
.status-success { background-color: #E3FCEF; color: #006644; }
.status-success .status-dot { background-color: #36B37E; }

/* Warning */
.status-warning { background-color: #FFF0B3; color: #FF8B00; }
.status-warning .status-dot { background-color: #FFAB00; }

/* Danger */
.status-danger { background-color: #FFEBE6; color: #BF2600; }
.status-danger .status-dot { background-color: #FF5630; }

/* Done */
.status-done { background-color: #EAE6FF; color: #403294; }
.status-done .status-dot { background-color: #6554C0; }

/* Archived */
.status-archived { background-color: #F4F5F7; color: #42526E; }
.status-archived .status-dot { background-color: #A5ADBA; }

/* Pending/Default */
.status-pending { background-color: #DFE1E6; color: #42526E; }
.status-pending .status-dot { background-color: #7A869A; }
</style>
