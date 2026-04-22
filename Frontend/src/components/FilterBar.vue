<script setup>
import { computed, nextTick, onBeforeUnmount, ref } from 'vue'

const props = defineProps({
  filters: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['remove', 'clear', 'add', 'add-filter', 'apply', 'update:filters'])

const showBuilder = ref(false)
const addButtonRef = ref(null)
const builderStyle = ref({ top: '0px', left: '0px', width: '260px' })
const draft = ref({ field: 'status', operator: 'is', value: '' })

const filterFields = [
  { key: 'status', label: 'Status', icon: 'fa-regular fa-circle-dot', values: ['BACKLOG', 'TO DO', 'IN PROGRESS', 'IN REVIEW', 'DONE'] },
  { key: 'assignee', label: 'Assignee', icon: 'fa-regular fa-user', values: ['Unassigned'] },
  { key: 'creator', label: 'Creator', icon: 'fa-regular fa-user', values: ['Me'] },
  { key: 'priority', label: 'Priority', icon: 'fa-solid fa-signal', values: ['Urgent', 'High', 'Normal', 'Low', 'None'] },
  { key: 'label', label: 'Label', icon: 'fa-solid fa-tag', values: ['No label'] },
  { key: 'startDate', label: 'Start date', icon: 'fa-regular fa-calendar-plus', values: ['Today', 'This week', 'Empty'] },
  { key: 'dueDate', label: 'Due date', icon: 'fa-regular fa-calendar', values: ['Today', 'This week', 'Overdue', 'Empty'] },
  { key: 'cycle', label: 'Cycle', icon: 'fa-solid fa-arrows-spin', values: ['No cycle'] },
  { key: 'module', label: 'Module', icon: 'fa-solid fa-table-cells-large', values: ['No module'] },
  { key: 'createdAt', label: 'Created at', icon: 'fa-regular fa-calendar', values: ['Today', 'This week'] },
  { key: 'updatedAt', label: 'Updated at', icon: 'fa-regular fa-calendar', values: ['Today', 'This week'] }
]

const operatorsByField = {
  status: ['is', 'is not', 'in', 'not in'],
  assignee: ['is', 'is not', 'empty', 'not empty'],
  creator: ['is', 'is not'],
  priority: ['is', 'is not', 'in'],
  label: ['includes', 'not includes', 'empty'],
  startDate: ['before', 'after', 'between', 'empty'],
  dueDate: ['before', 'after', 'between', 'empty', 'overdue'],
  cycle: ['is', 'is not', 'empty'],
  module: ['is', 'is not', 'empty'],
  createdAt: ['before', 'after', 'between'],
  updatedAt: ['before', 'after', 'between']
}

const selectedField = computed(() => filterFields.find(field => field.key === draft.value.field) || filterFields[0])
const availableOperators = computed(() => operatorsByField[draft.value.field] || ['is'])
const valueRequired = computed(() => !['empty', 'not empty', 'overdue'].includes(draft.value.operator))

const syncBuilderPosition = () => {
  if (!addButtonRef.value) return
  const rect = addButtonRef.value.getBoundingClientRect()
  builderStyle.value = {
    top: `${rect.bottom + 8}px`,
    left: `${Math.max(rect.left, 12)}px`,
    width: '260px'
  }
}

const closeBuilder = () => {
  showBuilder.value = false
}

const handleWindowChange = () => {
  if (showBuilder.value) {
    syncBuilderPosition()
  }
}

const handleDocumentClick = (event) => {
  if (!showBuilder.value) return
  if (event.target.closest('.filter-builder') || event.target.closest('.add-filter-icon-btn')) return
  closeBuilder()
}

const removeFilter = (id) => {
  const next = props.filters.filter(filter => filter.id !== id)
  emit('update:filters', next)
  emit('remove', id)
  emit('apply', next)
}

const clearAll = () => {
  emit('update:filters', [])
  emit('clear')
  emit('apply', [])
  closeBuilder()
}

const addFilter = async () => {
  emit('add')
  showBuilder.value = !showBuilder.value
  if (showBuilder.value) {
    await nextTick()
    syncBuilderPosition()
  }
}

const applyFilter = () => {
  if (valueRequired.value && !draft.value.value) return

  const filter = {
    id: `${draft.value.field}-${Date.now()}`,
    field: draft.value.field,
    operator: draft.value.operator,
    value: valueRequired.value ? draft.value.value : '',
    label: selectedField.value.label,
    condition: draft.value.operator,
    displayValue: valueRequired.value ? draft.value.value : draft.value.operator,
    icon: selectedField.value.icon
  }

  const next = [...props.filters, filter]
  emit('update:filters', next)
  emit('add', filter)
  emit('add-filter', filter)
  emit('apply', next)
  draft.value = { field: 'status', operator: 'is', value: '' }
  closeBuilder()
}

window.addEventListener('resize', handleWindowChange)
window.addEventListener('scroll', handleWindowChange, true)
document.addEventListener('click', handleDocumentClick)

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleWindowChange)
  window.removeEventListener('scroll', handleWindowChange, true)
  document.removeEventListener('click', handleDocumentClick)
})
</script>

<template>
  <div class="filter-bar-container">
    <div class="filters-scroll-area">
      <div v-for="filter in filters" :key="filter.id" class="filter-chip">
        <div class="chip-segment label-sec">
          <i v-if="filter.icon" :class="filter.icon" class="mr-2"></i>
          <span>{{ filter.label }}</span>
        </div>
        <div class="chip-segment condition-sec">{{ filter.condition }}</div>
        <div class="chip-segment value-sec">{{ filter.displayValue || filter.value || '--' }}</div>
        <button class="chip-segment remove-sec" type="button" @click="removeFilter(filter.id)" aria-label="Remove filter">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <span v-if="filters.length === 0" class="empty-filter-copy">No filters applied</span>

      <div class="add-filter-wrapper">
        <button
          ref="addButtonRef"
          class="add-filter-icon-btn"
          type="button"
          :class="{ active: showBuilder }"
          :aria-expanded="showBuilder"
          @click.stop="addFilter"
        >
          <i class="fa-solid fa-filter-circle-plus"></i>
        </button>
      </div>
    </div>

    <div class="filter-bar-actions" v-if="filters.length > 0">
      <div class="v-divider"></div>
      <button class="clear-all-btn" type="button" @click="clearAll">Clear all</button>
    </div>
  </div>

  <Teleport to="body">
    <div v-if="showBuilder" class="filter-builder" :style="builderStyle" @click.stop>
      <label>
        <span>Field</span>
        <select v-model="draft.field" @change="draft.operator = availableOperators[0]; draft.value = ''">
          <option v-for="field in filterFields" :key="field.key" :value="field.key">{{ field.label }}</option>
        </select>
      </label>

      <label>
        <span>Operator</span>
        <select v-model="draft.operator" @change="draft.value = ''">
          <option v-for="operator in availableOperators" :key="operator" :value="operator">{{ operator }}</option>
        </select>
      </label>

      <label v-if="valueRequired">
        <span>Value</span>
        <select v-model="draft.value">
          <option value="" disabled>Select value</option>
          <option v-for="value in selectedField.values" :key="value" :value="value">{{ value }}</option>
        </select>
      </label>

      <button class="apply-filter-btn" type="button" :disabled="valueRequired && !draft.value" @click="applyFilter">
        Apply filter
      </button>
    </div>
  </Teleport>
</template>

<style scoped>
.filter-bar-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 8px 12px;
  width: 100%;
  min-height: 48px;
  position: relative;
  z-index: 1000;
}

.filters-scroll-area {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px;
  flex: 1;
}

.empty-filter-copy {
  color: var(--color-text-muted);
  font-size: 13px;
}

.filter-chip {
  display: flex;
  align-items: stretch;
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  overflow: hidden;
  height: 32px;
}

.chip-segment {
  display: flex;
  align-items: center;
  padding: 0 10px;
  font-size: 13px;
  border-right: 1px solid var(--color-border);
  white-space: nowrap;
}

.label-sec {
  color: var(--color-text-secondary);
}

.condition-sec {
  color: var(--color-text-muted);
  background: var(--color-surface-hover);
}

.value-sec {
  color: var(--color-text-primary);
  font-weight: 600;
}

.remove-sec {
  border: 0;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 0 8px;
}

.remove-sec:hover {
  background: rgba(255, 60, 60, 0.1);
  color: #ef4444;
}

.add-filter-wrapper {
  position: relative;
}

.add-filter-icon-btn,
.clear-all-btn,
.apply-filter-btn {
  border-radius: 6px;
}

.add-filter-icon-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  color: var(--color-text-muted);
  cursor: pointer;
}

.add-filter-icon-btn.active {
  border-color: var(--color-accent);
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

.filter-builder {
  position: fixed;
  z-index: 9999;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  box-shadow: var(--shadow-md);
}

.filter-builder label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: var(--color-text-secondary);
  font-size: 12px;
}

.filter-builder select {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  color: var(--color-text-primary);
  padding: 8px;
  outline: none;
}

.apply-filter-btn {
  background: var(--color-accent);
  border: none;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  padding: 8px 10px;
}

.apply-filter-btn:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.filter-bar-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-left: 12px;
}

.v-divider {
  width: 1px;
  height: 20px;
  background: var(--color-border);
}

.clear-all-btn {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  padding: 6px 14px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
}

.clear-all-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}
</style>




