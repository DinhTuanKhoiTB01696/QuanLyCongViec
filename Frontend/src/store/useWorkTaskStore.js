import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useWorkTaskStore = defineStore('workTask', {
  state: () => ({
    tasks: [],
    loading: false,
    error: null,
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
    async fetchTasks(projectId) {
      if (!projectId) return;
      this.loading = true;
      this.error = null;
      try {
        const res = await axiosClient.get(`/projects/${projectId}/WorkTasks`);
        this.tasks = res.data?.data || [];
      } catch (err) {
        this.error = err.message;
        console.error('Failed to fetch tasks:', err);
      } finally {
        this.loading = false;
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
        await this.fetchTasks(projectId);
        return res.data?.data;
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
    }
  }
})
