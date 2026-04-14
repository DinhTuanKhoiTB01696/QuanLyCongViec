<template>
  <div class="plane-list-view">
    <!-- Header Controls omitted here as they are managed by SpaceSummary.vue -->

    <div v-for="(group, key) in groupedTasks" :key="key" class="list-group">
      <!-- Group Header -->
      <div class="group-header" @click="toggleGroup(key)">
        <div class="gh-left">
           <i class="gh-chevron fa-solid" :class="collapsedGroups[key] ? 'fa-chevron-right' : 'fa-chevron-down'"></i>
           <i class="status-icon" :class="group.iconClass" :style="{ color: group.color }"></i>
           <span class="group-name">{{ group.name }}</span>
           <span class="group-count">{{ group.tasks.length }}</span>
        </div>
        <div class="gh-right">
           <i class="fa-solid fa-plus add-icon"></i>
        </div>
      </div>

      <!-- Group Tasks -->
      <div class="group-content" v-show="!collapsedGroups[key]">
        <div class="task-row" v-for="task in group.tasks" :key="task.id" @click="emit('task-click', task)">
          <div class="tr-left">
            <span class="task-id">{{ task.sequenceId || task.id.substring(0,8).toUpperCase() }}</span>
            <span class="task-title" :style="group.name === 'Done' ? { textDecoration: 'line-through', color: '#71717A' } : {}">
               {{ task.title }}
               <span v-if="task.description" style="margin-left: 6px; font-size: 13px;">{{ task.description.includes('đ') ? '🐶' : '📝' }}</span>
            </span>
          </div>
          <div class="tr-right">
            <!-- Properties pills -->
            <div class="pill-group">
              <div class="pill pill-status">
                 <i class="status-icon-sm" :class="group.iconClass" :style="{ color: group.color }"></i>
                 {{ group.name }}
              </div>
              <div class="pill pill-priority">
                 <i class="fa-solid fa-signal" v-if="task.priority === 3" style="color: #F59E0B"></i>
                 <i class="fa-solid fa-arrow-down" v-else style="color: #71717A"></i>
              </div>
              <div class="pill pill-user">
                 <div class="avatar-xxs">
                    <i class="fa-regular fa-user" v-if="!task.assigneeName"></i>
                    <span v-else>{{ task.assigneeName.substring(0,1).toUpperCase() }}</span>
                 </div>
              </div>
            </div>
            <div class="row-action">
              <i class="fa-solid fa-ellipsis"></i>
            </div>
          </div>
        </div>

        <div class="add-row-placeholder">
          <i class="fa-solid fa-plus"></i> New work item
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'

const props = defineProps({
  tasks: { type: Array, default: () => [] }
})

const emit = defineEmits(['task-click'])

const collapsedGroups = ref({})

const toggleGroup = (key) => {
  collapsedGroups.value[key] = !collapsedGroups.value[key]
}

const groupedTasks = computed(() => {
  const groups = {
    backlog: { name: 'Backlog', iconClass: 'fa-regular fa-circle-dashed', color: '#71717A', tasks: [] },
    todo: { name: 'Todo', iconClass: 'fa-regular fa-circle', color: '#A1A1AA', tasks: [] },
    inprogress: { name: 'In Progress', iconClass: 'fa-solid fa-circle-half-stroke', color: '#F59E0B', tasks: [] },
    done: { name: 'Done', iconClass: 'fa-solid fa-circle-check', color: '#10B981', tasks: [] }
  }

  props.tasks.forEach(task => {
    const s = (task.statusName || '').toUpperCase()
    if (s === 'IN PROGRESS') groups.inprogress.tasks.push(task)
    else if (s === 'DONE') groups.done.tasks.push(task)
    else if (s === 'BACKLOG') groups.backlog.tasks.push(task)
    else groups.todo.tasks.push(task)
  })

  // Filter out empty groups if you want, but often they are kept. Let's keep them.
  return groups
})
</script>

<style scoped>
.plane-list-view {
  display: flex;
  flex-direction: column;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
}

.list-group {
  margin-bottom: 24px;
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  cursor: pointer;
  border-bottom: 1px solid #1E2025;
  margin-bottom: 8px;
}

.group-header:hover .add-icon { opacity: 1; }

.gh-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.gh-chevron {
  font-size: 10px;
  color: #71717A;
  width: 14px;
  text-align: center;
}

.status-icon {
  font-size: 14px;
}

.group-name {
  font-size: 14px;
  font-weight: 600;
  color: #E4E4E7;
}

.group-count {
  font-size: 12px;
  font-weight: 500;
  color: #71717A;
  margin-left: 4px;
}

.gh-right {
  display: flex;
  align-items: center;
}

.add-icon {
  color: #71717A;
  font-size: 14px;
  opacity: 0;
  transition: opacity 0.2s;
  padding: 4px;
}

.group-content {
  display: flex;
  flex-direction: column;
}

.task-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0 10px 24px;
  border-bottom: 1px solid #1E2025;
  cursor: pointer;
}
.task-row:hover {
  background-color: #16181D;
}

.tr-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.task-id {
  font-size: 12px;
  font-weight: 500;
  color: #71717A;
  width: 50px;
}

.task-title {
  font-size: 14px;
  font-weight: 500;
  color: #D4D4D8;
}

.tr-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.pill-group {
  display: flex;
  align-items: center;
  gap: 8px;
  opacity: 0;
  transition: opacity 0.2s;
}
.task-row:hover .pill-group { opacity: 1; }

.pill {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 8px;
  border: 1px solid #27272A;
  border-radius: 4px;
  font-size: 12px;
  color: #A1A1AA;
}
.pill i { font-size: 12px; }

.status-icon-sm { font-size: 12px; }

.avatar-xxs {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 1px dashed #3F3F46;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 9px;
  font-weight: 600;
}

.row-action {
  color: #71717A;
  padding: 4px 8px;
  opacity: 0;
  transition: opacity 0.2s;
}
.row-action:hover { color: #E4E4E7; }
.task-row:hover .row-action { opacity: 1; }

.add-row-placeholder {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 0 12px 24px;
  font-size: 13px;
  font-weight: 500;
  color: #71717A;
  cursor: pointer;
  border-bottom: 1px solid transparent;
}
.add-row-placeholder:hover {
  color: #E4E4E7;
}

</style>
