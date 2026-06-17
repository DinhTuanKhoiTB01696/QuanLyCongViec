import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from '@/store/useSiteStore'

export const useGoalStore = defineStore('goal', {
  state: () => ({
    goals: [],
    currentGoal: null,
    updates: [],
    linkedProjects: [],
    lessons: [],
    risks: [],
    decisions: [],
    history: [],
    isLoading: false,
    error: null,
    isEmpty: false,
    isSuccess: false
  }),
  actions: {
    getWorkspaceId() {
      const siteStore = useSiteStore()
      let id = siteStore.recentSite?.id || localStorage.getItem('sprinta_recent_site_id')
      if (!id || id === '1' || id.length < 36) {
        id = '00000000-0000-0000-0000-000000000000'
      }
      return id
    },
    async fetchGoals() {
      this.isLoading = true
      this.error = null
      this.isEmpty = false
      this.isSuccess = false
      try {
        const workspaceId = this.getWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
        
        const response = await axiosClient.get(`/workspaces/${workspaceId}/goals`)
        this.goals = response.data?.data || response.data || []
        this.isEmpty = this.goals.length === 0
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch goals'
        this.goals = []
      } finally {
        this.isLoading = false
      }
    },
    async createGoal(goalData) {
      this.isLoading = true
      try {
        const workspaceId = this.getWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
        
        const response = await axiosClient.post(`/workspaces/${workspaceId}/goals`, goalData)
        const newGoal = response.data?.data || response.data
        this.goals.push(newGoal)
        return newGoal
      } catch (err) {
        this.error = err.message || 'Failed to create goal'
        throw err
      } finally {
        this.isLoading = false
      }
    },
    async fetchGoalDetail(id) {
      this.isLoading = true
      this.error = null
      try {
        const workspaceId = this.getWorkspaceId()
        if (!workspaceId) throw new Error('No workspace selected')
        
        const response = await axiosClient.get(`/workspaces/${workspaceId}/goals/${id}`)
        const goal = response.data?.data || response.data
        this.currentGoal = goal
        
        // Map sub-entities from goal object (assuming EF Core Include)
        this.updates = goal.updates || []
        this.lessons = goal.lessons || []
        this.risks = goal.risks || []
        this.decisions = goal.decisions || []
        this.linkedProjects = goal.linkedProjects || []
        
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch goal detail'
      } finally {
        this.isLoading = false
      }
    },
    async addUpdate(goalId, data) {
      const workspaceId = this.getWorkspaceId()
      const response = await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/updates`, data)
      this.updates.push(response.data?.data || response.data)
    },
    async addLesson(goalId, data) {
      const workspaceId = this.getWorkspaceId()
      const response = await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/lessons`, data)
      this.lessons.push(response.data?.data || response.data)
    },
    async addRisk(goalId, data) {
      const workspaceId = this.getWorkspaceId()
      const response = await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/risks`, data)
      this.risks.push(response.data?.data || response.data)
    },
    async addDecision(goalId, data) {
      const workspaceId = this.getWorkspaceId()
      const response = await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/decisions`, data)
      this.decisions.push(response.data?.data || response.data)
    },
    async toggleArchive() {
      if (!this.currentGoal) return
      try {
        const workspaceId = this.getWorkspaceId()
        await axiosClient.post(`/workspaces/${workspaceId}/goals/${this.currentGoal.id}/archive`)
        this.currentGoal.isArchived = !this.currentGoal.isArchived
      } catch (err) {
        console.error('Failed to archive goal', err)
      }
    },
    async toggleFollow(goalId) {
      // Placeholder for future API if following API is implemented
      const target = this.goals.find(g => g.id === goalId)
      if (target) target.isFollowing = !target.isFollowing
      if (this.currentGoal && this.currentGoal.id === goalId) {
        this.currentGoal.isFollowing = !this.currentGoal.isFollowing
      }
    },
    async toggleStar() {
      if (!this.currentGoal) return
      try {
        const workspaceId = this.getWorkspaceId()
        // Assuming StarredItem API integration
        await axiosClient.post(`/workspaces/${workspaceId}/starreditems/toggle`, { 
          itemId: this.currentGoal.id, 
          itemType: 'Goal' 
        })
        this.currentGoal.isStarred = !this.currentGoal.isStarred
      } catch (err) {
        console.error('Failed to toggle star', err)
      }
    }
  }
})
