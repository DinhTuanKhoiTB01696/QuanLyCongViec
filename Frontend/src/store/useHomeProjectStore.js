import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useHomeProjectStore = defineStore('homeProject', {
  state: () => ({
    projects: [],
    currentProject: null,
    linkedGoals: [],
    linkedTasks: [],
    relatedProjects: [],
    history: [],
    updates: [],
    isLoading: false,
    error: null,
    isEmpty: false,
    isSuccess: false
  }),
  actions: {
    async fetchProjects() {
      this.isLoading = true
      this.error = null
      this.isEmpty = false
      this.isSuccess = false
      try {
        const response = await axiosClient.get('/projects')
        this.projects = response.data?.data || response.data || []
        this.isEmpty = this.projects.length === 0
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch projects'
        this.projects = []
      } finally {
        this.isLoading = false
      }
    },
    async fetchProjectDetail(id) {
      this.isLoading = true
      this.error = null
      try {
        const response = await axiosClient.get(`/projects/${id}`)
        const project = response.data?.data || response.data
        this.currentProject = project
        
        // Use backend fields if available, otherwise fallback to empty arrays
        this.linkedGoals = project.linkedGoals || []
        this.linkedTasks = project.linkedTasks || []
        this.relatedProjects = project.relatedProjects || []
        this.history = project.history || []
        this.updates = project.updates || []
        
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch project detail'
      } finally {
        this.isLoading = false
      }
    },
    async toggleArchive() {
      if (!this.currentProject) return
      try {
        await axiosClient.put(`/projects/${this.currentProject.id}/archive`)
        this.currentProject.isArchived = true
      } catch (err) {
        console.error('Failed to archive project', err)
      }
    },
    async toggleStar() {
      if (!this.currentProject) return
      try {
        await axiosClient.put(`/projects/${this.currentProject.id}/favorite`, { favorite: !this.currentProject.isFavorite })
        this.currentProject.isFavorite = !this.currentProject.isFavorite
        this.currentProject.isStarred = this.currentProject.isFavorite // Sync alias
      } catch (err) {
        console.error('Failed to toggle star', err)
      }
    },
    async createProject(projectData) {
      this.isLoading = true
      try {
        const response = await axiosClient.post('/projects', projectData)
        const newProject = response.data?.data || response.data
        if (this.projects) {
          this.projects.unshift(newProject)
        }
        return newProject
      } catch (err) {
        console.error('Failed to create project', err)
        throw err
      } finally {
        this.isLoading = false
      }
    }
  }
})
