<script setup>
import { ref } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'

// Mock tasks based on image 1
const mockTasks = [
  { id: 'CUN-8', title: 'Test Task', state: 'Backlog', priority: 'None', labels: 'Select labels', modules: 'Select modules', cycle: 'Select cycle' },
  { id: 'CUN-7', title: '6. Customize your settings', state: 'Backlog', priority: 'None', labels: 'Select labels', modules: '2 Modules', cycle: 'Cycle 2: Collaboration & Cu...' },
  { id: 'CUN-6', title: '5. Use Cycles to time box tasks', state: 'Backlog', priority: 'Low', labels: 'concepts', modules: '2 Modules', cycle: 'Cycle 2: Collaboration & Cu...' },
  { id: 'CUN-5', title: '4. Visualize your work', state: 'In Progress', priority: 'None', labels: 'Select labels', modules: 'Onboarding Flow (Feature)', cycle: 'Cycle 2: Collaboration & Cu...' },
  { id: 'CUN-4', title: '3. Create and assign Work items', state: 'In Progress', priority: 'High', labels: 'concepts', modules: '2 Modules', cycle: 'Cycle 1: Getting Started wit...' },
  { id: 'CUN-3', title: '2. Invite your team', state: 'Backlog', priority: 'High', labels: 'Select labels', modules: '2 Modules', cycle: 'Cycle 1: Getting Started wit...' },
  { id: 'CUN-2', title: '1. Create Projects', state: 'Todo', priority: 'High', labels: 'concepts', modules: 'Core Workflow (System)', cycle: 'Cycle 1: Getting Started wit...' },
  { id: 'CUN-1', title: 'Welcome to Plane', state: 'Done', priority: 'Urgent', labels: 'Select labels', modules: 'Core Workflow (System)', cycle: 'Cycle 1: Getting Started wit...' }
]

const getStatusIcon = (st) => {
  if (st === 'Done') return { class: 'fa-solid fa-circle-check text-green', color: '#10B981' }
  if (st === 'In Progress') return { class: 'fa-solid fa-circle-half-stroke text-orange', color: '#F59E0B' }
  if (st === 'Todo') return { class: 'fa-regular fa-circle text-muted', color: '#71717A' }
  return { class: 'fa-solid fa-circle-dashed text-muted', color: '#71717A' } // Backlog
}

const getPrioIcon = (pr) => {
  if (pr === 'Urgent') return { class: 'fa-solid fa-angles-up text-red' }
  if (pr === 'High') return { class: 'fa-solid fa-chevron-up text-orange' }
  if (pr === 'Low') return { class: 'fa-solid fa-chevron-down text-blue' }
  return { class: 'fa-solid fa-ban text-muted' }
}
</script>

<template>
  <NexusLayout>
    <div class="views-wrapper">
      <!-- Header -->
      <header class="vh-header">
        <div class="vh-left">
           <span class="breadcrumb"><i class="fa-solid fa-layer-group"></i> Views <i class="fa-solid fa-chevron-right separator"></i> All work items <i class="fa-solid fa-chevron-down ms-2" style="font-size: 10px;"></i></span>
        </div>
        <div class="vh-right">
           <button class="plane-toolbar-btn"><i class="fa-solid fa-filter"></i></button>
           <button class="plane-toolbar-btn">Display</button>
           <button class="plane-primary-btn">Add view <i class="fa-solid fa-chevron-down"></i></button>
        </div>
      </header>
      
      <!-- Table content -->
      <div class="spreadsheet-container">
        <table class="plane-table">
          <thead>
            <tr>
              <th style="width: 25%;">Work items</th>
              <th><i class="fa-regular fa-circle-dot"></i> State <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-signal"></i> Priority <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-user-group"></i> Assignees <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-tag"></i> Labels <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-table-cells-large"></i> Modules <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-arrows-spin"></i> Cycle <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-regular fa-calendar"></i> Start date <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-calendar-day"></i> Due date <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-triangle-exclamation"></i> Estimate <i class="fa-solid fa-chevron-down f-10"></i></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in mockTasks" :key="t.id">
              <td>
                <div class="wi-cell">
                  <span class="wi-id">{{ t.id }}</span>
                  <span class="wi-title">{{ t.title }}</span>
                </div>
              </td>
              <td>
                <div class="state-cell">
                  <i :class="getStatusIcon(t.state).class"></i>
                  <span>{{ t.state }}</span>
                </div>
              </td>
              <td>
                <div class="prio-cell">
                  <i :class="getPrioIcon(t.priority).class"></i>
                  <span>{{ t.priority }}</span>
                </div>
              </td>
              <td>
                <div class="assignee-cell">
                  <i class="fa-regular fa-user"></i>
                  <span class="text-muted">Assignees</span>
                </div>
              </td>
              <td>
                <div class="label-cell text-muted">
                  <i class="fa-solid fa-tag" v-if="t.labels.includes('Select')"></i>
                  <span class="d-dot" v-else></span>
                  {{ t.labels }}
                </div>
              </td>
              <td>
                <div class="module-cell text-muted">
                  <i class="fa-solid fa-table-cells-large"></i> {{ t.modules }}
                </div>
              </td>
              <td>
                <div class="cycle-cell text-muted">
                  <i class="fa-solid fa-arrows-spin"></i> {{ t.cycle }}
                </div>
              </td>
              <td class="text-muted"><i class="fa-regular fa-calendar"></i> Start date</td>
              <td class="text-muted"><i class="fa-solid fa-calendar-day"></i> Due date</td>
              <td class="text-muted"><i class="fa-solid fa-caret-up"></i> Estimate</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.views-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #0D0F11;
  color: #E4E4E7;
}
.vh-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
}
.separator { font-size: 10px; color: #71717A; border-right: 1px solid #27272A; padding-right: 8px; margin-right: 8px; }
.ms-2 { margin-left: 8px; }

.vh-right {
  display: flex;
  gap: 12px;
}
.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: #D4D4D8;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-toolbar-btn:hover { background: #1E2025; }

.plane-primary-btn {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background 0.2s;
}
.plane-primary-btn:hover { background: #0284C7; }

/* Spreadsheet styles */
.spreadsheet-container {
  flex: 1;
  overflow: auto;
}
.plane-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12px;
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
.plane-table th i.f-10 { font-size: 9px; float: right; margin-top: 4px; opacity: 0.5; }
.plane-table th i:not(.f-10) { margin-right: 6px; }

.plane-table td {
  padding: 8px 16px;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  white-space: nowrap;
}
.plane-table tr:hover { background: #16181D; }

.wi-cell { display: flex; align-items: center; gap: 16px; }
.wi-id { color: #A1A1AA; min-width: 45px; }
.wi-title { color: #E4E4E7; font-weight: 500; }

.state-cell, .prio-cell { display: flex; align-items: center; gap: 8px; color: #E4E4E7; }
.text-green { color: #10B981; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }
.text-blue { color: #3B82F6; }
.text-muted { color: #71717A; }

.assignee-cell, .label-cell, .module-cell, .cycle-cell { display: flex; align-items: center; gap: 6px; }
.d-dot { width: 6px; height: 6px; border-radius: 50%; background: #9333EA; display: inline-block; }
td i { width: 14px; text-align: center; }
</style>
