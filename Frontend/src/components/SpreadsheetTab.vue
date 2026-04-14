<script setup>
import { computed } from 'vue'

const props = defineProps({
  tasks: {
    type: Array,
    default: () => []
  }
})

const formatDate = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

const getPrioIcon = (prio) => {
  if (prio === 1) return { class: 'fa-solid fa-angles-up text-red', label: 'Urgent' }
  if (prio === 2) return { class: 'fa-solid fa-chevron-up text-orange', label: 'High' }
  if (prio === 3) return { class: 'fa-solid fa-minus text-blue', label: 'Low' }
  return { class: 'fa-solid fa-ban text-muted', label: 'None' }
}

const getStatusDisplay = (statusName) => {
  const s = statusName?.toUpperCase() || ''
  if (s === 'DONE') return { class: 'fa-solid fa-circle-check text-green', label: 'Done' }
  if (s === 'IN PROGRESS') return { class: 'fa-solid fa-circle-half-stroke text-orange', label: 'In Progress' }
  if (s === 'TO DO' || s === 'TODO') return { class: 'fa-regular fa-circle text-muted', label: 'Todo' }
  return { class: 'fa-solid fa-circle-dashed text-muted', label: 'Backlog' }
}
</script>

<template>
  <div class="spreadsheet-container">
    <table class="plane-table">
      <thead>
        <tr>
          <th style="width: 35%;">Work items</th>
          <th><i class="fa-regular fa-circle-dot"></i> State</th>
          <th><i class="fa-solid fa-signal"></i> Priority</th>
          <th><i class="fa-solid fa-user-group"></i> Assignees</th>
          <th>Created on</th>
          <th>Updated on</th>
          <th><i class="fa-solid fa-link"></i> Link</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(task, index) in tasks" :key="task.id || index">
          <td>
            <div class="wi-cell">
              <span class="wi-id">{{ task.sequenceId || `CUN-${index + 1}` }}</span>
              <span class="wi-title">{{ task.title }}</span>
            </div>
          </td>
          <td>
            <div class="state-cell">
              <i :class="getStatusDisplay(task.statusName).class"></i>
              <span>{{ getStatusDisplay(task.statusName).label }}</span>
            </div>
          </td>
          <td>
            <div class="prio-cell">
              <i :class="getPrioIcon(task.priority).class"></i>
              <span>{{ getPrioIcon(task.priority).label }}</span>
            </div>
          </td>
          <td>
            <!-- Assignee Mock -->
            <div class="assignee-cell">
              <div class="avatar-group">
                <i class="fa-regular fa-user"></i>
                <span>{{ task.assigneeName ? 'Assignees' : 'Assignees' }}</span>
              </div>
            </div>
          </td>
          <td>
            <span class="date-text">{{ formatDate(task.createdDate || '2026-04-12T00:00:00Z') }}</span>
          </td>
          <td>
            <span class="date-text">{{ formatDate(task.updatedDate || '2026-04-14T00:00:00Z') }}</span>
          </td>
          <td>
            <span class="link-text">0 links</span>
          </td>
        </tr>
        <!-- Empty State -->
        <tr v-if="tasks.length === 0">
           <td colspan="7" class="empty-cell">No work items found.</td>
        </tr>
      </tbody>
    </table>
    
    <div class="table-footer">
       <button class="add-btn"><i class="fa-solid fa-plus"></i> Add work item</button>
    </div>
  </div>
</template>

<style scoped>
.spreadsheet-container {
  flex: 1;
  background: #0D0F11;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
  overflow: auto;
  border-top: 1px solid #1E2025;
}

.plane-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 13px;
}

.plane-table th {
  padding: 12px 16px;
  font-weight: 500;
  color: #A1A1AA;
  border-bottom: 2px solid #1E2025;
  border-right: 1px solid #1E2025;
  background: #0D0F11;
  position: sticky;
  top: 0;
  z-index: 10;
  white-space: nowrap;
}
.plane-table th i { margin-right: 6px; font-size: 12px; }

.plane-table td {
  padding: 10px 16px;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  white-space: nowrap;
}
.plane-table tr:hover { background: #16181D; }

.wi-cell { display: flex; align-items: center; gap: 16px; }
.wi-id { color: #A1A1AA; font-size: 12px; min-width: 50px; }
.wi-title { color: #E4E4E7; font-weight: 500; }

.state-cell, .prio-cell { display: flex; align-items: center; gap: 8px; color: #E4E4E7; }
.text-green { color: #10B981; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }
.text-blue { color: #3B82F6; }
.text-muted { color: #71717A; }

.assignee-cell .avatar-group { display: flex; align-items: center; gap: 6px; color: #E4E4E7; }
.date-text { color: #A1A1AA; font-size: 12px; }
.link-text { color: #A1A1AA; font-size: 12px; }

.empty-cell { text-align: center; color: #71717A; padding: 40px; }

.table-footer {
  padding: 12px 16px;
}
.add-btn {
  background: transparent;
  color: #A1A1AA;
  border: none;
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
}
.add-btn:hover { color: #E4E4E7; }
</style>
