import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from '@/store/useSiteStore'
import { ensureWorkspaceIdFromState, resolveWorkspaceIdFromState } from '@/utils/contextIds'

export const useStarredStore = defineStore('starredStore', {
  state: () => ({
    starredItems: [],
    loading: false,
    error: null
  }),
  actions: {
    getWorkspaceId() {
      const siteStore = useSiteStore()
      return resolveWorkspaceIdFromState({ siteStore })
    },
    async ensureWorkspaceId() {
      const siteStore = useSiteStore()
      return ensureWorkspaceIdFromState({ siteStore })
    },
    async fetchStarredItems() {
      this.loading = true
      this.error = null
      try {
        const workspaceId = await this.ensureWorkspaceId()
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
        const workspaceId = await this.ensureWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
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
