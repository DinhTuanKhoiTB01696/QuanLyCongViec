import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useProjectStore = defineStore('project', {
  state: () => ({
    currentProject: null,
    loading: false,
    error: null,
    members: [],
    tags: []
  }),
  actions: {
    async fetchProjectDetails(projectId) {
      if (!projectId) return;
      this.loading = true;
      try {
        const [projRes, membersRes, tagsRes] = await Promise.all([
          axiosClient.get(`/projects/${projectId}`),
          axiosClient.get(`/projects/${projectId}/members`),
          axiosClient.get(`/projects/${projectId}/labels`)
        ]);
        this.currentProject = projRes.data?.data || null;
        this.members = membersRes.data?.data || [];
        this.tags = tagsRes.data?.data || [];
      } catch (err) {
        this.error = err.message;
        console.error('Failed to fetch project details:', err);
      } finally {
        this.loading = false;
      }
    }
  }
})
