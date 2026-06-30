import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useActivityStore = defineStore('activityStore', {
  state: () => ({
    activities: [],
    loading: false,
    total: 0
  }),
  actions: {
    normalizeActivity(item) {
      const timestamp = item.timestamp || item.createdAt || item.time || new Date().toISOString()
      const user = item.user || item.userName || item.actorName || item.email || 'System'
      const action = item.action || item.eventType || 'updated'
      const resource = item.resource || item.entityName || item.targetType || ''
      const summary = item.summary || item.description || item.message || `${user} ${action} ${resource}`.trim()

      return {
        id: item.id || `${action}-${resource}-${timestamp}`,
        icon: item.icon || 'fa-solid fa-clock-rotate-left',
        text: summary || `${user} ${action}`.trim(),
        bold: item.bold || item.targetId || item.entityId || '',
        time: new Date(timestamp).toLocaleString(),
        _ts: Date.parse(timestamp) || Date.now(),
        raw: item
      }
    },

    async fetchRecentActivities(params = {}) {
      this.loading = true
      try {
        // Default to last 30 days if no timeFilter provided
        if (!params.timeFilter) params.timeFilter = '30d'
        
        const res = await axiosClient.get('/site-auditlogs', { params })
        if (res.data && res.data.data) {
          this.activities = (res.data.data.items || []).map(item => this.normalizeActivity(item))
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

