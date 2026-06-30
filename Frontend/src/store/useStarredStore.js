import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from '@/store/useSiteStore'

export const useStarredStore = defineStore('starredStore', {
  state: () => ({
    starredItems: [],
    loading: false,
    error: null
  }),
  actions: {
    getWorkspaceId() {
      const siteStore = useSiteStore()
      let id = siteStore.recentSite?.id || localStorage.getItem('recent_site_id')
      if (!id || id === '1' || id.length < 36) {
        id = '00000000-0000-0000-0000-000000000000'
      }
      return id
    },
    async fetchStarredItems() {
      this.loading = true
      this.error = null
      try {
        const workspaceId = this.getWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
        
        const response = await axiosClient.get(`/workspaces/${workspaceId}/starreditems`)
        this.starredItems = response.data?.data || response.data || []
      } catch (err) {
        this.error = err.message || 'Failed to fetch starred items'
        console.error(this.error)
      } finally {
        this.loading = false
      }
    },
    async toggleStar(itemType, itemId) {
      try {
        const workspaceId = this.getWorkspaceId()
        await axiosClient.post(`/workspaces/${workspaceId}/starreditems/toggle`, null, {
          params: { itemType, itemId }
        })
        await this.fetchStarredItems()
      } catch (err) {
        console.error('Failed to toggle star', err)
      }
    }
  }
})
