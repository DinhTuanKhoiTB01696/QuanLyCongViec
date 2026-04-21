import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

const SPRINT_CACHE_TTL_MS = 30000

export const useSprintStore = defineStore('sprint', {
  state: () => ({
    sprints: [],
    activeSprint: null,
    loading: false,
    error: null,
    lastFetchedByProject: {}
  }),
  actions: {
    async fetchSprints(projectId, options = {}) {
      if (!projectId) return

      const { force = false } = options
      const lastFetched = this.lastFetchedByProject[projectId]
      const isWarm = lastFetched && (Date.now() - lastFetched) < SPRINT_CACHE_TTL_MS
      if (!force && isWarm && this.sprints.length) {
        return
      }

      this.loading = true
      this.error = null

      try {
        const response = await axiosClient.get(`/projects/${projectId}/sprints`)
        this.sprints = response.data?.data || []
        this.activeSprint = this.sprints.find(item => (item.state || '').toLowerCase() === 'active') || null
        this.lastFetchedByProject = {
          ...this.lastFetchedByProject,
          [projectId]: Date.now()
        }
      } catch (error) {
        this.error = error.message
        console.error('Failed to fetch sprints:', error)
      } finally {
        this.loading = false
      }
    },

    async toggleFavorite(projectId, sprintId) {
      if (!projectId || !sprintId) return

      const index = this.sprints.findIndex(item => item.id === sprintId)
      const previous = index !== -1 ? this.sprints[index].isFavorite : null

      if (index !== -1) {
        this.sprints[index].isFavorite = !this.sprints[index].isFavorite
      }

      try {
        const response = await axiosClient.patch(`/projects/${projectId}/sprints/${sprintId}/favorite`)
        if (index !== -1) {
          this.sprints[index].isFavorite = response.data?.data?.isFavorite
        }
      } catch (error) {
        if (index !== -1) {
          this.sprints[index].isFavorite = previous
        }
        console.error('Failed to toggle sprint favorite:', error)
      }
    }
  }
})
