import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { reportExpectedError } from '@/utils/errorTelemetry'

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
      if (!projectId || projectId === 'default' || projectId.length < 30) return

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
        reportExpectedError('Failed to fetch sprints', error)
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
        reportExpectedError('Failed to toggle sprint favorite', error)
      }
    },

    async startSprint(projectId, sprintId) {
      if (!projectId || !sprintId) return

      try {
        const response = await axiosClient.post(`/projects/${projectId}/sprints/${sprintId}/start`)
        await this.fetchSprints(projectId, { force: true })
        return response.data?.data
      } catch (error) {
        reportExpectedError('Failed to start sprint', error)
        throw error
      }
    }
  }
})
