import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useSprintStore = defineStore('sprint', {
  state: () => ({
    sprints: [],
    activeSprint: null,
    loading: false,
    error: null
  }),
  actions: {
    async fetchSprints(projectId) {
      if (!projectId) return;
      this.loading = true;
      try {
        const res = await axiosClient.get(`/projects/${projectId}/sprints`);
        this.sprints = res.data?.data || [];
        this.activeSprint = this.sprints.find(s => s.status === true) || null;
      } catch (err) {
        this.error = err.message;
        console.error('Failed to fetch sprints:', err);
      } finally {
        this.loading = false;
      }
    }
  }
})
