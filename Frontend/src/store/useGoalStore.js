import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { useSiteStore } from '@/store/useSiteStore'

export const useGoalStore = defineStore('goal', {
  state: () => ({
    goals: [],
    currentGoal: null,
    updates: [],
    comments: [],
    spaceProjects: [],
    siteProjects: [],
    lessons: [],
    risks: [],
    decisions: [],
    history: [],
    statuses: [],
    isLoading: false,
    error: null,
    isEmpty: false,
    isSuccess: false
  }),
  actions: {
    isValidWorkspaceId(id) {
      const emptyGuid = '00000000-0000-0000-0000-000000000000'
      return typeof id === 'string' &&
        /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id) &&
        id !== emptyGuid
    },
    getWorkspaceId() {
      const siteStore = useSiteStore()
      const candidates = [
        siteStore.recentSite?.id,
        localStorage.getItem('recent_site_id'),
        localStorage.getItem('sprinta_recent_site_id'),
        siteStore.sites?.[0]?.id
      ]

      const id = candidates.find(candidate => this.isValidWorkspaceId(candidate))
      if (id && localStorage.getItem('recent_site_id') !== id) {
        localStorage.setItem('recent_site_id', id)
      }
      return id || null
    },
    async ensureWorkspaceId() {
      let workspaceId = this.getWorkspaceId()
      if (workspaceId) return workspaceId

      const siteStore = useSiteStore()
      if (!siteStore.sites?.length) {
        await siteStore.fetchSites()
      }

      workspaceId = this.getWorkspaceId()
      if (!workspaceId) throw new Error('No workspace selected')
      return workspaceId
    },
    async fetchGoals() {
      this.isLoading = true
      this.error = null
      this.isEmpty = false
      this.isSuccess = false
      try {
        const workspaceId = await this.ensureWorkspaceId()

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
    async fetchStatuses() {
      try {
        const workspaceId = await this.ensureWorkspaceId()

        const response = await axiosClient.get(`/workspaces/${workspaceId}/goals/statuses`)
        this.statuses = response.data?.data || response.data || []
        return this.statuses
      } catch (err) {
        console.warn('Failed to fetch goal statuses', err)
        return this.statuses || []
      }
    },
    async createGoal(goalData) {
      this.isLoading = true
      try {
        const workspaceId = await this.ensureWorkspaceId()

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
        const workspaceId = await this.ensureWorkspaceId()

        const response = await axiosClient.get(`/workspaces/${workspaceId}/goals/${id}`)
        const goal = response.data?.data || response.data
        this.currentGoal = goal

        this.updates = goal.updates || []
        this.comments = goal.comments || []
        this.lessons = goal.lessons || []
        this.risks = goal.risks || []
        this.decisions = goal.decisions || []
        const allProjects = goal.linkedProjects || []
        this.spaceProjects = allProjects.filter(p => p.linkType === 'spaceProject')
        this.siteProjects = allProjects.filter(p => p.linkType === 'siteProject')

        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch goal detail'
      } finally {
        this.isLoading = false
      }
    },

    // --- Updates ---
    async addUpdate(goalId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      const response = await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/updates`, data)
      await this.fetchGoalDetail(goalId)
      await this.fetchGoals()
      return response.data?.data || response.data
    },
    async updateUpdate(goalId, updateId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      const response = await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}`, data)
      await this.fetchGoalDetail(goalId)
      return response.data?.data || response.data
    },
    async deleteUpdate(goalId, updateId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Goal Comments ---
    async addGoalComment(goalId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/comments`, data)
      await this.fetchGoalDetail(goalId)
    },
    async updateGoalComment(goalId, commentId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/comments/${commentId}`, data)
      await this.fetchGoalDetail(goalId)
    },
    async deleteGoalComment(goalId, commentId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/comments/${commentId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Update Comments ---
    async addUpdateComment(goalId, updateId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/comments`, data)
      await this.fetchGoalDetail(goalId)
    },
    async updateUpdateComment(goalId, updateId, commentId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/comments/${commentId}`, data)
      await this.fetchGoalDetail(goalId)
    },
    async deleteUpdateComment(goalId, updateId, commentId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/comments/${commentId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Reactions ---
    async toggleReaction(goalId, updateId, reactionType) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/reactions`, { reactionType })
      await this.fetchGoalDetail(goalId)
    },

    // --- Attachments ---
    async addUpdateAttachment(goalId, updateId, formData) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      await this.fetchGoalDetail(goalId)
    },
    async deleteUpdateAttachment(goalId, updateId, attachmentId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/updates/${updateId}/attachments/${attachmentId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Project Links ---
    async addProjectLink(goalId, projectId, linkType) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/projects`, { projectId, linkType })
      await this.fetchGoalDetail(goalId)
    },
    async deleteProjectLink(goalId, linkId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/projects/${linkId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Lessons / Risks / Decisions ---
    async addLesson(goalId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/lessons`, data)
      await this.fetchGoalDetail(goalId)
    },
    async updateLesson(goalId, lessonId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/lessons/${lessonId}`, data)
      await this.fetchGoalDetail(goalId)
    },
    async deleteLesson(goalId, lessonId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/lessons/${lessonId}`)
      await this.fetchGoalDetail(goalId)
    },

    async addRisk(goalId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/risks`, data)
      await this.fetchGoalDetail(goalId)
    },
    async updateRisk(goalId, riskId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/risks/${riskId}`, data)
      await this.fetchGoalDetail(goalId)
    },
    async deleteRisk(goalId, riskId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/risks/${riskId}`)
      await this.fetchGoalDetail(goalId)
    },

    async addDecision(goalId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.post(`/workspaces/${workspaceId}/goals/${goalId}/decisions`, data)
      await this.fetchGoalDetail(goalId)
    },
    async updateDecision(goalId, decisionId, data) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.put(`/workspaces/${workspaceId}/goals/${goalId}/decisions/${decisionId}`, data)
      await this.fetchGoalDetail(goalId)
    },
    async deleteDecision(goalId, decisionId) {
      const workspaceId = await this.ensureWorkspaceId()
      await axiosClient.delete(`/workspaces/${workspaceId}/goals/${goalId}/decisions/${decisionId}`)
      await this.fetchGoalDetail(goalId)
    },

    // --- Standard Toggles ---
    async toggleArchive() {
      if (!this.currentGoal) return
      try {
        const workspaceId = await this.ensureWorkspaceId()
        await axiosClient.post(`/workspaces/${workspaceId}/goals/${this.currentGoal.id}/archive`)
        this.currentGoal.isArchived = !this.currentGoal.isArchived
      } catch (err) {
        console.error('Failed to archive goal', err)
      }
    },
    async toggleFollow(goalId) {
      const target = this.goals.find(g => g.id === goalId)
      if (target) target.isFollowing = !target.isFollowing
      if (this.currentGoal && this.currentGoal.id === goalId) {
        this.currentGoal.isFollowing = !this.currentGoal.isFollowing
      }
    },
    async toggleStar() {
      if (!this.currentGoal) return
      try {
        const workspaceId = await this.ensureWorkspaceId()
        await axiosClient.post(`/workspaces/${workspaceId}/starreditems/toggle`, {
          itemId: this.currentGoal.id,
          itemType: 'Goal'
        })
        this.currentGoal.isStarred = !this.currentGoal.isStarred
      } catch (err) {
        console.error('Failed to toggle star', err)
      }
    },
    async updateGoal(id, data) {
      this.isLoading = true
      try {
        const workspaceId = await this.ensureWorkspaceId()

        const response = await axiosClient.put(`/workspaces/${workspaceId}/goals/${id}`, data)
        const updatedGoal = response.data?.data || response.data

        const idx = this.goals.findIndex(g => g.id === id)
        if (idx !== -1) {
          this.goals[idx] = { ...this.goals[idx], ...updatedGoal }
        }

        if (this.currentGoal && this.currentGoal.id === id) {
          this.currentGoal = { ...this.currentGoal, ...updatedGoal }
        }
        await this.fetchGoals()
        return updatedGoal
      } catch (err) {
        this.error = err.message || 'Failed to update goal'
        throw err
      } finally {
        this.isLoading = false
      }
    }
  }
})
