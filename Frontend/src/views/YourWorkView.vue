<script setup>
import { ref, onMounted, computed } from 'vue'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useActivityStore } from '@/store/useActivityStore'
import apexchart from 'vue3-apexcharts'
import { ElNotification } from 'element-plus'

const activeTab = ref('Summary')
const tabs = ['Summary', 'Assigned', 'Created', 'Subscribed', 'Starred', 'Activity']

const myTasks = ref([])
const loading = ref(false)
const actStore = useActivityStore()
const selectedProjectId = ref(null)
const projectList = ref([])

const currentUserId = computed(() => {
  const user = localStorage.getItem('user')
  return user ? JSON.parse(user).id : null
})

const fetchProjects = async () => {
    try {
        const [discoveryRes, archivedRes] = await Promise.all([
            axiosClient.get('/projects/discovery'),
            axiosClient.get('/projects/archived')
        ])
        const activeProjects = (discoveryRes.data?.data || []).map(p => ({ ...p, isArchived: false }))
        const archivedProjects = (archivedRes.data?.data || []).map(p => ({ ...p, isArchived: true }))
        projectList.value = [...activeProjects, ...archivedProjects]
    } catch(e) {
        console.error('Error fetching projects', e)
    }
}

const fetchMyTasks = async () => {
  try {
    loading.value = true
    const res = await axiosClient.get('/tasks/search')
    myTasks.value = res.data?.data || []
    actStore.fetchRecentActivities()

    const existingIds = new Set(actStore.activities.map(activity => activity.id))
    let added = false
    myTasks.value.forEach(task => {
      const id = `db-${task.id}`
      if (!existingIds.has(id)) {
        const action = task.reporterId === currentUserId.value ? 'Created' : 'Assigned to'
        actStore.activities.push({
          id,
          icon: 'fa-solid fa-list-check',
          text: `${action} work item`,
          bold: `"${task.title}"`,
          time: new Date(task.createdAt).toLocaleString(),
          _ts: new Date(task.createdAt).getTime()
        })
        added = true
      }
    })

    if (added) {
      actStore.activities.forEach(activity => {
        if (!activity._ts) {
          const ts = Date.parse(activity.time)
          activity._ts = Number.isNaN(ts) ? Date.now() : ts
        }
      })
      actStore.activities.sort((left, right) => right._ts - left._ts)
      actStore.activities = actStore.activities.slice(0, 50)
      localStorage.setItem('nexus_activities', JSON.stringify(actStore.activities))
    }
  } catch (error) {
    console.error('Failed to load tasks:', error)
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await fetchProjects()
  fetchMyTasks()
})

const overview = computed(() => ({
  created: myTasks.value.filter(task => task.reporterId === currentUserId.value).length,
  assigned: myTasks.value.filter(task => task.assignedUserId === currentUserId.value).length,
  subscribed: myTasks.value.filter(task => task.isSubscribed).length
}))

const workload = computed(() => {
  let backlog = 0
  let notStarted = 0
  let workingOn = 0
  let completed = 0
  let canceled = 0

  myTasks.value.forEach(task => {
    const status = (task.statusName || 'BACKLOG').toUpperCase().trim()
    if (status === 'BACKLOG') backlog += 1
    else if (status === 'TODO' || status === 'TO DO') notStarted += 1
    else if (status === 'IN PROGRESS' || status === 'INPROGRESS') workingOn += 1
    else if (status === 'DONE') completed += 1
    else if (status === 'CANCELLED' || status === 'CANCELED') canceled += 1
    else backlog += 1
  })

  return { backlog, notStarted, workingOn, completed, canceled }
})

const prioritySeries = computed(() => [
  myTasks.value.filter(task => task.priority === 1).length,
  myTasks.value.filter(task => task.priority === 2).length,
  myTasks.value.filter(task => task.priority === 3).length,
  myTasks.value.filter(task => task.priority === 4).length
])

const priorityChartOptions = computed(() => ({
  chart: { type: 'pie', toolbar: { show: false }, background: 'transparent' },
  theme: { mode: 'dark' },
  labels: ['Urgent', 'High', 'Medium', 'Low'],
  dataLabels: {
    enabled: true,
    style: { fontSize: '11px', fontWeight: 600 }
  },
  legend: {
    position: 'bottom',
    labels: { colors: '#A1A1AA' }
  },
  colors: ['#ef4444', '#f97316', '#3b82f6', '#94a3b8'],
  stroke: { colors: ['#111315'] },
  tooltip: { theme: 'dark' }
}))

const stateSeries = computed(() => [{
  name: 'Work items',
  data: [
    workload.value.backlog,
    workload.value.notStarted,
    workload.value.workingOn,
    workload.value.completed
  ]
}])

const stateChartOptions = computed(() => ({
  chart: { type: 'bar', toolbar: { show: false }, background: 'transparent' },
  theme: { mode: 'dark' },
  plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '48%', distributed: true } },
  dataLabels: { enabled: false },
  legend: { show: false },
  colors: ['#71717A', '#3B82F6', '#F59E0B', '#10B981'],
  grid: { borderColor: '#27272A', strokeDashArray: 3 },
  xaxis: {
    categories: ['Backlog', 'Not Started', 'In Progress', 'Completed'],
    labels: { style: { colors: '#A1A1AA' } }
  },
  yaxis: {
    labels: { style: { colors: '#E4E4E7' } }
  },
  tooltip: { theme: 'dark' }
}))

const recentActivity = computed(() => {
  return actStore.activities.map(activity => ({
    id: activity.id,
    text: `${activity.text} ${activity.bold || ''}`.trim(),
    time: activity.time
  }))
})

const listData = computed(() => {
  let list = myTasks.value
  if (activeTab.value === 'Assigned') {
    list = myTasks.value.filter(task => task.assignedUserId === currentUserId.value)
  } else if (activeTab.value === 'Created') {
    list = myTasks.value.filter(task => task.reporterId === currentUserId.value)
  } else if (activeTab.value === 'Subscribed') {
    list = myTasks.value.filter(task => task.isSubscribed)
  } else if (activeTab.value === 'Starred') {
    const starredLocal = JSON.parse(localStorage.getItem('starred_tasks') || '[]')
    list = starredLocal.map(v => myTasks.value.find(t => t.id === v.id)).filter(Boolean)
  }

  return list.map(task => ({
    id: task.sequenceId || task.id.substring(0, 8).toUpperCase(),
    rawId: task.id,
    title: task.title,
    state: task.statusName || 'To Do',
    priority: task.priority || 3,
    assigneeName: task.assigneeName,
    modules: '0 Modules',
    cycle: 'No Cycle',
    task
  }))
})

const updateTaskProperty = async (task, field, value) => {
  try {
    const index = myTasks.value.findIndex(item => item.id === task.id)
    if (index !== -1) {
      myTasks.value[index][field] = value

      const updatePayload = { [field]: value }
      if (task.projectId) {
        await axiosClient.patch(`/projects/${task.projectId}/WorkTasks/${task.id}`, updatePayload)
      }

      let activityText = `Updated ${field} to ${value}`
      if (field === 'statusName') activityText = `Changed status to ${value}`
      if (field === 'priority') {
        activityText = `Changed priority to ${value === 1 ? 'Urgent' : value === 2 ? 'High' : value === 3 ? 'Normal' : 'Low'}`
      }
      actStore.logActivity(activityText, `on ${task.sequenceId || task.id}`, 'fa-solid fa-pen-to-square')
    }
  } catch (error) {
    console.error('Failed to update task:', error)
  }
}

const pageActivities = computed(() => actStore.activities)

const downloadWordActivity = () => {
  let htmlContent = `
  <html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>
  <head><meta charset='utf-8'><title>Activity Log</title></head><body>
  <h2>Activity History</h2>
  <ul>
  `

  actStore.activities.forEach(activity => {
    htmlContent += `<li><strong>${activity.time}</strong> - ${activity.text} <em>${activity.bold || ''}</em></li>`
  })

  htmlContent += '</ul></body></html>'

  const blob = new Blob(['\ufeff', htmlContent], { type: 'application/msword' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `Activity_Log_${new Date().toISOString().slice(0, 10)}.doc`
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)

  ElNotification({ title: 'Success', message: 'Activity history exported.', type: 'success' })
}
</script>

<template>
  <NexusLayout>
    <div class="yw-container">
      <div class="yw-main">
        <header class="yw-header flex-between">
          <span class="yw-title"><i class="fa-regular fa-user"></i> Your work</span>
        </header>


        <div class="yw-tabs">
          <button
            v-for="tab in tabs"
            :key="tab"
            class="tab-btn"
            :class="{ active: activeTab === tab }"
            @click="activeTab = tab"
          >
            {{ tab }}
          </button>
        </div>

        <div class="yw-scrollable" v-if="activeTab === 'Summary'">
          <h3 class="section-title mt-4">Overview</h3>
          <div class="yw-cards-row">
            <div class="yw-card">
              <div class="card-icon"><i class="fa-solid fa-plus"></i></div>
              <div class="card-info">
                <div class="card-lbl">Work items created</div>
                <div class="card-val">{{ overview.created }}</div>
              </div>
            </div>
            <div class="yw-card">
              <div class="card-icon"><i class="fa-regular fa-circle-user"></i></div>
              <div class="card-info">
                <div class="card-lbl">Work items assigned</div>
                <div class="card-val">{{ overview.assigned }}</div>
              </div>
            </div>
            <div class="yw-card">
              <div class="card-icon"><i class="fa-solid fa-inbox"></i></div>
              <div class="card-info">
                <div class="card-lbl">Work items subscribed</div>
                <div class="card-val">{{ overview.subscribed }}</div>
              </div>
            </div>
          </div>

          <h3 class="section-title mt-4">Workload</h3>
          <div class="yw-workload-row">
            <div class="wl-card">
              <div class="wl-lbl"><span class="dbox bg-gray"></span> Backlog</div>
              <div class="wl-val">{{ workload.backlog }}</div>
            </div>
            <div class="wl-card">
              <div class="wl-lbl"><span class="dbox bg-blue"></span> Not started</div>
              <div class="wl-val">{{ workload.notStarted }}</div>
            </div>
            <div class="wl-card">
              <div class="wl-lbl"><span class="dbox bg-orange"></span> Working on</div>
              <div class="wl-val">{{ workload.workingOn }}</div>
            </div>
            <div class="wl-card">
              <div class="wl-lbl"><span class="dbox bg-green"></span> Completed</div>
              <div class="wl-val">{{ workload.completed }}</div>
            </div>
            <div class="wl-card">
              <div class="wl-lbl"><span class="dbox bg-red"></span> Canceled</div>
              <div class="wl-val">{{ workload.canceled }}</div>
            </div>
          </div>

          <div class="yw-two-cols mt-4">
            <div class="chart-col">
              <h3 class="section-title">Work items by Priority</h3>
              <div class="empty-chart" v-if="myTasks.length === 0">
                <i class="fa-solid fa-chart-simple chart-icon"></i>
                <span>No work item assigned yet</span>
              </div>
              <apexchart
                v-else
                type="pie"
                height="220"
                :options="priorityChartOptions"
                :series="prioritySeries"
              />
            </div>
            <div class="chart-col">
              <h3 class="section-title">Work items by state</h3>
              <div class="empty-chart" v-if="myTasks.length === 0">
                <i class="fa-solid fa-chart-column chart-icon"></i>
                <span>No work item assigned yet</span>
              </div>
              <apexchart
                v-else
                type="bar"
                height="170"
                :options="stateChartOptions"
                :series="stateSeries"
              />
            </div>
          </div>

          <h3 class="section-title mt-4">Recent activity</h3>
          <div class="list-body">
            <div class="list-row" style="cursor: default;" v-for="activity in recentActivity" :key="activity.id">
              <div class="lr-left">
                <span class="lr-id" style="min-width: 30px;"><i class="fa-solid fa-clock-rotate-left" style="color: #A1A1AA"></i></span>
                <span class="lr-title">{{ activity.text }}</span>
              </div>
              <div class="lr-right">
                <div class="lr-badge cursor-not-allowed">
                  <i class="fa-regular fa-clock"></i> {{ activity.time }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="yw-scrollable" v-else-if="['Assigned', 'Created', 'Subscribed', 'Starred'].includes(activeTab)">
          <div class="list-header mt-4">
            <i class="fa-solid fa-circle-dashed f-icon"></i>
            <span class="lh-title">All work items</span>
            <span class="lh-count">{{ listData.length }}</span>
          </div>

          <div class="list-body mt-4">
            <div class="list-row" v-for="item in listData" :key="item.id">
              <div class="lr-left">
                <span class="lr-id">{{ item.id }}</span>
                <span class="lr-title">{{ item.title }}</span>
              </div>
              <div class="lr-right">
                <el-dropdown trigger="click" @command="value => updateTaskProperty(item.task, 'statusName', value)">
                  <div class="lr-badge cursor-pointer hover:bg-[var(--color-bg-secondary)]">
                    <i class="fa-solid fa-circle-check" v-if="item.state.toUpperCase() === 'DONE'"></i>
                    <i class="fa-solid fa-circle-half-stroke" v-else-if="item.state.toUpperCase() === 'IN PROGRESS'"></i>
                    <i class="fa-regular fa-circle" v-else></i>
                    {{ item.state }}
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu class="plane-dropdown">
                      <el-dropdown-item command="BACKLOG">Backlog</el-dropdown-item>
                      <el-dropdown-item command="TO DO">To Do</el-dropdown-item>
                      <el-dropdown-item command="IN PROGRESS">In Progress</el-dropdown-item>
                      <el-dropdown-item command="IN REVIEW">In Review</el-dropdown-item>
                      <el-dropdown-item command="DONE">Done</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

                <el-dropdown trigger="click" @command="value => updateTaskProperty(item.task, 'priority', value)">
                  <div class="lr-badge cursor-pointer hover:bg-[var(--color-bg-secondary)]">
                    <i class="fa-solid fa-angles-up text-red-500" v-if="item.priority === 1"></i>
                    <i class="fa-solid fa-chevron-up text-orange-500" v-else-if="item.priority === 2"></i>
                    <i class="fa-solid fa-minus text-blue-500" v-else-if="item.priority === 3"></i>
                    <i class="fa-solid fa-chevron-down text-gray-400" v-else></i>
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu class="plane-dropdown">
                      <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up text-red-500"></i> Urgent</el-dropdown-item>
                      <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up text-orange-500"></i> High</el-dropdown-item>
                      <el-dropdown-item :command="3"><i class="fa-solid fa-minus text-blue-500"></i> Normal</el-dropdown-item>
                      <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down text-gray-400"></i> Low</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

                <div class="lr-badge cursor-not-allowed">
                  <i class="fa-regular fa-user" v-if="!item.assigneeName"></i>
                  <span v-else>{{ item.assigneeName.substring(0, 1).toUpperCase() }}</span>
                </div>
                <div class="lr-badge cursor-not-allowed"><i class="fa-regular fa-calendar"></i></div>
                <div class="lr-badge cursor-not-allowed"><i class="fa-solid fa-table-cells-large"></i> {{ item.modules }}</div>
                <div class="lr-badge cursor-not-allowed"><i class="fa-solid fa-arrows-spin"></i> {{ item.cycle }}</div>
              </div>
            </div>
          </div>
        </div>

        <div class="yw-scrollable" v-else-if="activeTab === 'Activity'">
          <div class="activity-page-header mt-4 flex-between">
            <h3 class="section-title" style="margin: 0;">Recent activity</h3>
            <button class="plane-primary-btn" @click="downloadWordActivity">Download today's activity</button>
          </div>

          <div class="list-body mt-4">
            <div class="list-row" style="cursor: default;" v-for="(activity, index) in pageActivities" :key="index">
              <div class="lr-left">
                <span class="lr-id" style="min-width: 30px;"><i :class="activity.icon || 'fa-solid fa-bell'"></i></span>
                <span class="lr-title">{{ activity.text }} <span class="p-ac-bold text-white">{{ activity.bold }}</span></span>
              </div>
              <div class="lr-right">
                <div class="lr-badge cursor-not-allowed">
                  <i class="fa-regular fa-clock"></i> {{ activity.time }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="yw-sidebar">
        <div class="cover-image">
          <button class="edit-cover"><i class="fa-solid fa-pencil"></i></button>
        </div>
        <div class="profile-info">
          <div class="avatar-lg bg-blue">A</div>
          <div class="user-details">
            <h2 class="user-name">Alo</h2>
            <p class="user-handle">(cuongdqtb01697)</p>
          </div>

          <div class="info-row mt-4">
            <span class="info-lbl">Joined on</span>
            <span class="info-val">Apr 12, 2026</span>
          </div>
          <div class="info-row">
            <span class="info-lbl">Timezone</span>
            <span class="info-val">03:07 UTC</span>
          </div>

          <div class="workspace-row mt-4">
            <i class="fa-solid fa-briefcase ws-icon"></i>
            <span>Cun</span>
            <i class="fa-solid fa-chevron-down ms-auto" style="font-size: 10px; color: #71717A;"></i>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.yw-container {
  display: flex;
  height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
  font-family: 'Inter', sans-serif;
  overflow: hidden;
}

.yw-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0 32px;
  overflow-y: auto;
}

.yw-header {
  padding: 24px 0 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.yw-title {
  font-size: 16px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
}

.yw-tabs {
  display: flex;
  gap: 24px;
  border-bottom: 1px solid var(--color-border);
}

.tab-btn {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  font-size: 13px;
  font-weight: 500;
  padding: 8px 0;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
}
.tab-btn:hover { color: var(--color-text-primary); }
.tab-btn.active { color: var(--color-accent); border-bottom: 2px solid var(--color-accent); }

.yw-scrollable {
  padding-bottom: 40px;
}

.mt-4 { margin-top: 24px; }
.section-title { font-size: 14px; font-weight: 600; margin-bottom: 16px; color: var(--color-text-primary); }

.yw-cards-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.yw-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
}
.card-icon { font-size: 18px; color: var(--color-text-muted); width: 24px; text-align: center; }
.card-lbl { font-size: 11px; color: var(--color-text-muted); margin-bottom: 4px; }
.card-val { font-size: 18px; font-weight: 600; color: var(--color-text-primary); }

.yw-workload-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
}

.wl-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 60px;
}
.wl-lbl { font-size: 12px; font-weight: 500; color: var(--color-text-muted); display: flex; align-items: center; gap: 6px; }
.dbox { width: 8px; height: 8px; border-radius: 2px; }
.bg-gray { background: var(--color-text-muted); }
.bg-blue { background: var(--color-accent); }
.bg-orange { background: var(--color-warning); }
.bg-green { background: var(--color-success); }
.bg-red { background: var(--color-danger); }
.wl-val { font-size: 18px; font-weight: 600; color: var(--color-text-primary); margin-top: auto;}

.yw-two-cols {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.chart-col {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 20px;
  transition: all 0.3s ease;
}

.empty-chart {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  height: 150px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: var(--color-text-muted);
  font-size: 12px;
}

.chart-icon { font-size: 32px; opacity: 0.3; }

.list-header { display: flex; align-items: center; gap: 8px; font-size: 14px; font-weight: 600; color: #E4E4E7; }
.f-icon { color: #A1A1AA; font-size: 12px; }
.lh-count { font-size: 12px; font-weight: 400; color: #71717A; }
.list-header { display: flex; align-items: center; gap: 8px; font-size: 14px; font-weight: 600; color: var(--color-text-primary); }
.f-icon { color: var(--color-text-muted); font-size: 12px; }
.lh-count { font-size: 12px; font-weight: 400; color: var(--color-text-muted); }

.list-body { border-top: 1px solid var(--color-border); }
.list-row { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid var(--color-border); transition: background 0.2s; cursor: pointer; }
.list-row:hover { background: var(--color-surface); }
.lr-left { display: flex; align-items: center; gap: 16px; }
.lr-id { font-size: 12px; color: var(--color-text-muted); min-width: 45px; }
.lr-title { font-size: 13px; font-weight: 500; color: var(--color-text-primary); }
.lr-right { display: flex; align-items: center; gap: 6px; }

.lr-badge { border: 1px solid var(--color-border); border-radius: 2px; padding: 4px 8px; font-size: 12px; color: var(--color-text-muted); display: flex; align-items: center; gap: 6px; }
.lr-badge.green { border-color: #064E3B; background: rgba(16, 185, 129, 0.1); color: #10B981; }
.lr-badge i { font-size: 11px; }
.text-orange { color: #F59E0B; }
.avatar-badge { width: 24px; height: 24px; border-radius: 50%; background: var(--color-accent); color: var(--color-text-primary); display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 600; padding: 0; border: none; }

.flex-between { display: flex; justify-content: space-between; align-items: center; }
.plane-primary-btn { background: var(--color-accent); color: var(--color-text-primary); border: none; border-radius: 2px; padding: 6px 12px; font-size: 12px; font-weight: 500; cursor: pointer; transition: background 0.2s; }
.plane-primary-btn:hover { filter: brightness(1.1); }

.p-act-row { display: flex; align-items: flex-start; gap: 16px; padding: 16px 0; border-bottom: 1px solid var(--color-border); }
.p-act-icon { width: 20px; font-size: 12px; color: var(--color-text-muted); text-align: center; margin-top: 2px; }
.p-act-content { display: flex; align-items: center; flex-wrap: wrap; gap: 6px; font-size: 13px; }
.p-ac-text { color: var(--color-text-muted); }
.p-ac-bold { color: var(--color-text-primary); font-weight: 500; }
.p-ac-time { color: var(--color-text-muted); font-size: 11px; }

.yw-sidebar {
  width: 320px;
  background: var(--color-bg);
  border-left: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
}

.cover-image {
  height: 120px;
  background: var(--color-border);
  background-image: linear-gradient(45deg, var(--color-surface), var(--color-border));
  position: relative;
}

.edit-cover {
  position: absolute;
  top: 16px;
  right: 16px;
  background: rgba(0, 0, 0, 0.5);
  border: none;
  color: #E4E4E7;
  border-radius: 2px;
  color: var(--color-text-primary);
  width: 24px;
  height: 24px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile-info {
  padding: 0 24px 24px;
  position: relative;
}

.avatar-lg {
  position: absolute;
  top: -24px;
  width: 48px;
  height: 48px;
  background: var(--color-accent);
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 600;
  border-radius: 2px;
  border: 4px solid var(--color-bg);
}

.user-details { margin-top: 40px; }
.user-name { font-size: 16px; font-weight: 600; margin: 0; color: var(--color-text-primary); }
.user-handle { font-size: 12px; color: var(--color-text-muted); margin: 4px 0 0 0; }

.info-row { display: flex; justify-content: space-between; font-size: 12px; margin-bottom: 8px; }
.info-lbl { color: var(--color-text-muted); }
.info-val { color: var(--color-text-primary); font-weight: 500; }

.workspace-row { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; padding-top: 16px; border-top: 1px solid var(--color-border); cursor: pointer; }
.ws-icon { color: #F59E0B; }
.ms-auto { margin-left: auto; }
</style>




