import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from './useSiteStore'
import { useProjectStore } from './useProjectStore'

const normalizeDateOnly = (value) => {
  if (!value) return null
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) return value
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) return value.slice(0, 10)
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

import { normalizeProjectRole } from '@/utils/permissions'

const normalizeTaskRecord = (task = {}, fallbackProjectId = null) => {
  const parentId = task.parentTaskId || task.parentId || task.ParentTaskId || task.ParentId || null
  const id = task.id || task.Id || null
  const projectId = task.projectId || task.ProjectId || fallbackProjectId || null
  const assignees = Array.isArray(task.assignees)
    ? task.assignees
    : Array.isArray(task.Assignees)
      ? task.Assignees
      : []
  const assigneeIds = Array.from(new Set([
    ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
    ...(Array.isArray(task.AssigneeIds) ? task.AssigneeIds : []),
    ...assignees.map(item => item?.userId || item?.id).filter(Boolean),
    ...(task.assignedUserId || task.AssignedUserId ? [task.assignedUserId || task.AssignedUserId] : [])
  ]))

  return {
    ...task,
    id,
    projectId,
    parentId,
    parentTaskId: parentId,
    assignedUserId: task.assignedUserId || task.AssignedUserId || null,
    assigneeIds,
    assignees: assignees
      .map(item => ({
        ...item,
        userId: item?.userId || item?.UserId || item?.id,
        fullName: item?.fullName || item?.FullName || item?.name,
        email: item?.email || item?.Email,
        progressPercent: item?.progressPercent ?? item?.ProgressPercent ?? 0,
        contributionWeight: item?.contributionWeight ?? item?.ContributionWeight ?? 1,
        estimatedHours: item?.estimatedHours ?? item?.EstimatedHours ?? 0,
        totalActualHours: item?.totalActualHours ?? item?.TotalActualHours ?? 0
      }))
      .filter(item => item.userId)
      .filter((item, index, list) => list.findIndex(candidate => candidate.userId === item.userId) === index),
    statusName: task.statusName || task.StatusName || '',
    sequenceId: task.sequenceId || task.SequenceId || null,
    sortOrder: task.sortOrder ?? task.SortOrder ?? 0,
    sprintId: task.sprintId || task.SprintId || null,
    moduleId: task.moduleId || task.ModuleId || null,
    totalEstimatedHours: task.totalEstimatedHours ?? task.TotalEstimatedHours ?? 0,
    totalActualHours: task.totalActualHours ?? task.TotalActualHours ?? 0,
      visibilityMode: `${task.visibilityMode || task.VisibilityMode || 'project'}`
        .trim()
        .toLowerCase()
        .replace(/\s+/g, '_'),
      visibleToRoles: (Array.isArray(task.visibleToRoles)
        ? task.visibleToRoles
        : Array.isArray(task.VisibleToRoles)
          ? task.VisibleToRoles
          : [])
        .map(role => normalizeProjectRole(role))
        .filter(Boolean),
    storyPoints: task.storyPoints ?? task.StoryPoints ?? 0,
    plannedStartDate: normalizeDateOnly(task.plannedStartDate || task.PlannedStartDate || null),
    plannedEndDate: normalizeDateOnly(task.plannedEndDate || task.PlannedEndDate || null),
    dueDate: normalizeDateOnly(task.dueDate || task.DueDate || null),
    createdAt: task.createdAt || task.CreatedAt || null,
    updatedAt: task.updatedAt || task.UpdatedAt || null
  }
}

export const useWorkTaskStore = defineStore('workTask', {
  state: () => ({
    tasks: [],
    starredTasks: [],
    recentlyViewedTasks: (() => {
      try {
        return JSON.parse(localStorage.getItem('recently_viewed_tasks') || '[]')
      } catch {
        return []
      }
    })(),
    loading: false,
    error: null,
    errorStatus: null,
    currentProjectId: null,
    fetchAbortController: null,
    fetchRequestId: 0
  }),
  getters: {
    backlogTasks: (state) => (state.tasks || []).filter(t => {
       const s = (t.statusName || '').toUpperCase().trim();
       return s === 'BACKLOG' || s === '';
    }).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)),
    
    todoTasks: (state) => (state.tasks || []).filter(t => {
       const s = (t.statusName || '').toUpperCase().trim();
       return s === 'TODO' || s === 'TO DO';
    }).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)),
    
    inProgressTasks: (state) => (state.tasks || []).filter(t => {
       const s = (t.statusName || '').toUpperCase().trim();
       return s === 'IN PROGRESS' || s === 'INPROGRESS';
    }).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)),
    
    reviewTasks: (state) => (state.tasks || []).filter(t => {
       const s = (t.statusName || '').toUpperCase().trim();
       return s === 'IN REVIEW' || s === 'REVIEW';
    }).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)),

    doneTasks: (state) => (state.tasks || []).filter(t => {
       const s = (t.statusName || '').toUpperCase().trim();
       return s === 'DONE';
    }).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)),
  },
  actions: {
    normalizeTaskRecord(task = {}, fallbackProjectId = null) {
      return normalizeTaskRecord(task, fallbackProjectId)
    },
    clearTasks(projectId = null) {
      this.tasks = []
      this.error = null
      this.errorStatus = null
      this.currentProjectId = projectId
    },
    async fetchTasks(projectId, options = {}) {
      if (!projectId) return;

      const { reset = true } = options
      const previousProjectId = this.currentProjectId
      const requestId = this.fetchRequestId + 1
      this.fetchRequestId = requestId
      this.fetchAbortController?.abort()
      const controller = new AbortController()
      this.fetchAbortController = controller

      this.loading = true;
      this.error = null;
      this.errorStatus = null
      this.currentProjectId = projectId

      const shouldReset = reset && (previousProjectId !== projectId || !this.tasks.length)
      if (shouldReset) {
        this.tasks = []
      }

      try {
        const res = await axiosClient.get(`/projects/${projectId}/WorkTasks`, {
          signal: controller.signal
        });

        if (requestId !== this.fetchRequestId || this.currentProjectId !== projectId) {
          return this.currentProjectId === projectId ? this.tasks : []
        }

        this.tasks = (res.data?.data || [])
          .map(task => normalizeTaskRecord(task, projectId))
        this.fetchStarredTasks().catch(() => {})
        return this.tasks
      } catch (err) {
        if (err?.name === 'CanceledError' || err?.code === 'ERR_CANCELED') {
          return this.currentProjectId === projectId ? this.tasks : []
        }

        this.error = err.message;
        this.errorStatus = Number(err?.response?.status || 0) || null
        console.error('Failed to fetch tasks:', err);
        return this.currentProjectId === projectId ? this.tasks : []
      } finally {
        if (requestId === this.fetchRequestId) {
          this.loading = false;
          this.fetchAbortController = null
        }
      }
    },
    async createTask(projectId, payload) {
      try {
        const res = await axiosClient.post(`/projects/${projectId}/WorkTasks`, payload);
        await this.fetchTasks(projectId);
        return res.data?.data;
      } catch (err) {
        console.error('Error creating task:', err);
        throw err;
      }
    },
    async updateTask(projectId, taskId, payload, options = {}) {
      const index = this.tasks.findIndex(t => t.id === taskId);
      const previousTask = index >= 0 ? { ...this.tasks[index] } : null;
      const method = options.method === 'put' ? 'put' : 'patch';

      if (index >= 0) {
        this.tasks[index] = { ...this.tasks[index], ...payload };
      }

      try {
        const res = method === 'put'
          ? await axiosClient.put(`/projects/${projectId}/WorkTasks/${taskId}`, payload)
          : await axiosClient.patch(`/projects/${projectId}/WorkTasks/${taskId}`, payload);
        const updatedTask = normalizeTaskRecord(res.data?.data || {}, projectId);
        if (index >= 0 && updatedTask?.id) {
          this.tasks[index] = {
            ...this.tasks[index],
            ...updatedTask
          };
        }
        return updatedTask;
      } catch (err) {
        if (index >= 0 && previousTask) {
          this.tasks[index] = previousTask;
        }
        this.error = err.response?.data?.message || err.message;
        throw err;
      }
    },
    async updateTaskStatus(projectId, taskId, statusName) {
      const task = this.tasks.find(t => t.id === taskId);
      const previousStatus = task?.statusName;
      if (task) task.statusName = statusName;
      try {
        await axiosClient.put(`/projects/${projectId}/WorkTasks/${taskId}/status`, { statusName });
        await this.fetchTasks(projectId);
      } catch (err) {
        if (task) task.statusName = previousStatus;
        this.error = err.response?.data?.message || err.message;
        throw err;
      }
    },
    async reorderTask(projectId, taskId, sortOrder, newStatusName) {
      const task = this.tasks.find(t => t.id === taskId);
      const previousTask = task ? { ...task } : null;
      if (task) {
        task.sortOrder = sortOrder;
        if (newStatusName) task.statusName = newStatusName;
      }

      try {
        await axiosClient.put(`/projects/${projectId}/WorkTasks/${taskId}/reorder`, { sortOrder, newStatusName });
        await this.fetchTasks(projectId);
      } catch (err) {
        if (task && previousTask) Object.assign(task, previousTask);
        this.error = err.response?.data?.message || err.message;
        throw err;
      }
    },
    resolveWorkspaceId(projectId = null, taskProjectId = null) {
      const siteStore = useSiteStore()
      const projectStore = useProjectStore()
      
      const searchId = projectId || this.currentProjectId
      
      // 1. Try projectStore.currentProject
      let p = projectStore.currentProject
      if (p && p.id === searchId) {
        const wId = p.workspaceId || p.WorkspaceId
        if (wId && wId.length >= 36) return wId
      }
      
      // 2. Try projectDetailsById
      if (searchId) {
        p = projectStore.projectDetailsById[searchId]
        if (p) {
          const wId = p.workspaceId || p.WorkspaceId
          if (wId && wId.length >= 36) return wId
        }
      }
      
      // 3. Try allProjects
      if (searchId) {
        p = projectStore.allProjects.find(item => item.id === searchId)
        if (p) {
          const wId = p.workspaceId || p.WorkspaceId || p.originalRow?.workspaceId || p.originalRow?.WorkspaceId
          if (wId && wId.length >= 36) return wId
        }
      }
      
      // 4. Try task's projectId
      const tPid = taskProjectId || projectId
      if (tPid) {
        p = projectStore.allProjects.find(item => item.id === tPid)
        if (p) {
          const wId = p.workspaceId || p.WorkspaceId || p.originalRow?.workspaceId || p.originalRow?.WorkspaceId
          if (wId && wId.length >= 36) return wId
        }
      }
      
      // 5. Try local storage
      const localId = localStorage.getItem('sprinta_recent_site_id') || localStorage.getItem('recent_site_id')
      if (localId && localId.length >= 36 && localId !== '00000000-0000-0000-0000-000000000000') {
        return localId
      }
      
      // 6. Try siteStore
      const siteId = siteStore.recentSite?.id || siteStore.recentSite?.Id
      if (siteId && siteId.length >= 36) {
        return siteId
      }
      
      return null
    },
    async fetchStarredTasks() {
      let workspaceId = this.resolveWorkspaceId()
      if (!workspaceId) {
        workspaceId = '00000000-0000-0000-0000-000000000000'
      }
      try {
        const response = await axiosClient.get(`/workspaces/${workspaceId}/StarredItems`)
        const data = response.data
        if (Array.isArray(data)) {
          this.starredTasks = data
        } else if (data && Array.isArray(data.data)) {
          this.starredTasks = data.data
        } else {
          this.starredTasks = []
        }
      } catch (error) {
        console.error('Failed to fetch starred tasks:', error)
      }
    },
    async toggleTaskStar(taskOrId) {
      if (!taskOrId) return
      const taskId = typeof taskOrId === 'object' ? taskOrId.id : taskOrId
      const fullTask = typeof taskOrId === 'object' ? taskOrId : this.tasks.find(t => t.id === taskId)
      
      let workspaceId = this.resolveWorkspaceId(null, fullTask?.projectId)
      if (!workspaceId || workspaceId.length < 36) {
        workspaceId = '00000000-0000-0000-0000-000000000000'
      }
      if (workspaceId === '00000000-0000-0000-0000-000000000000') {
        console.error('Cannot toggle star: workspace ID is empty')
        return
      }
      
      const index = this.starredTasks.findIndex(t => t.itemId === taskId)
      const isStarred = index >= 0

      // Optimistic UI update
      if (isStarred) {
        this.starredTasks.splice(index, 1)
      } else {
        this.starredTasks.push({
          id: Math.random().toString(), // Temp ID
          itemId: taskId,
          itemType: 'Task',
          title: fullTask?.title || 'Task',
          subtitle: fullTask?.projectName || '',
          url: `/space/${fullTask?.projectId}/work-items?task=${taskId}`
        })
      }

      try {
        await axiosClient.post(`/workspaces/${workspaceId}/StarredItems/toggle?itemType=Task&itemId=${taskId}`)
        // Fetch again to ensure sync with DB
        await this.fetchStarredTasks()
      } catch (error) {
        console.error('Failed to toggle task star:', error)
        // Revert optimistic update
        await this.fetchStarredTasks()
      }
    },
    isTaskStarred(taskId) {
      if (!taskId) return false
      const id = typeof taskId === 'object' ? taskId.id : taskId
      return this.starredTasks.some(t => t.itemId === id)
    },
    logViewedTask(task, spaces = []) {
      if (!task || !task.id) return
      // Remove duplicate nếu đã có
      this.recentlyViewedTasks = this.recentlyViewedTasks.filter(item => item.id !== task.id)
      const proj = spaces.find(s => s.id === task.projectId)
      this.recentlyViewedTasks.unshift({
        id: task.id,
        title: task.title,
        sequenceId: task.sequenceId,
        projectId: task.projectId,
        projectName: task.projectName || proj?.name || 'Project',
        projectColor: task.projectColor || proj?.cover || '#3b82f6',
        createdAt: task.createdAt || new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        statusName: task.statusName || 'TO DO',
        priority: task.priority || 3
      })
      // Giữ tối đa 15 items
      if (this.recentlyViewedTasks.length > 15) {
        this.recentlyViewedTasks = this.recentlyViewedTasks.slice(0, 15)
      }
      localStorage.setItem('recently_viewed_tasks', JSON.stringify(this.recentlyViewedTasks))
    }
  }
})
