import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useActivityStore = defineStore('activityStore', {
  state: () => ({
    activities: [],
    loading: false,
    total: 0
  }),
  actions: {
    async fetchRecentActivities(params = {}) {
      this.loading = true
      try {
        // Default to last 30 days if no timeFilter provided
        if (!params.timeFilter) params.timeFilter = '30d'
        
        const res = await axiosClient.get('/auditlogs', { params })
        if (res.data && res.data.data) {
          this.activities = res.data.data.items || []
          this.total = res.data.data.total || 0
        }
      } catch (err) {
        console.error('Failed to load activities', err)
      } finally {
        this.loading = false
      }
    },
    
    async logActivity(text, bold, icon = 'fa-regular fa-bell') {
      // In a real app, this might be handled by the backend automatically on actions.
      // But we can keep a local-only log or just refresh from server.
      await this.fetchRecentActivities()
    }
  }
})

