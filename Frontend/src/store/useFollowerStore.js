import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from '@/store/useSiteStore'

export const useFollowerStore = defineStore('followerStore', {
  state: () => ({
    followedItems: [],
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
    async fetchFollowedItems() {
      this.loading = true
      this.error = null
      try {
        const workspaceId = this.getWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
        
        const response = await axiosClient.get(`/workspaces/${workspaceId}/followers`)
        this.followedItems = response.data?.data || response.data || []
      } catch (err) {
        this.error = err.message || 'Failed to fetch followed items'
        console.error(this.error)
      } finally {
        this.loading = false
      }
    },
    async toggleFollow(entityType, entityId) {
      try {
        const workspaceId = this.getWorkspaceId()
        await axiosClient.post(`/workspaces/${workspaceId}/followers/toggle`, null, {
          params: { entityType, entityId }
        })
        await this.fetchFollowedItems()
      } catch (err) {
        console.error('Failed to toggle follow', err)
      }
    }
  }
})
