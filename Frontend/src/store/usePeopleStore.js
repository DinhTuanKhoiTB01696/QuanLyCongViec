import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const usePeopleStore = defineStore('people', {
  state: () => ({
    users: [],
    currentUser: null,
    linkedGoals: [],
    linkedProjects: [],
    kudos: [],
    history: [],
    isLoading: false,
    error: null,
    isEmpty: false,
    isSuccess: false,
    totalCount: 0,
    page: 1,
    pageSize: 20
  }),
  actions: {
    async fetchPeople(search = '', page = 1, pageSize = 20) {
      this.isLoading = true
      this.error = null
      this.isEmpty = false
      this.isSuccess = false
      try {
        const response = await axiosClient.get('/users', {
          params: { search, page, pageSize }
        })
        const data = response.data?.data || response.data || []
        
        this.users = data.map(u => ({
          id: u.id,
          fullName: u.fullName || u.email,
          email: u.email,
          location: u.location || '',
          avatar: u.avatarUrl || (u.fullName ? u.fullName.substring(0, 2).toUpperCase() : u.email.substring(0, 2).toUpperCase()),
          department: u.departmentName || 'N/A',
          position: u.jobTitle || 'Member',
          team: u.projects && u.projects.length > 0 ? u.projects[0] : 'N/A',
          timezone: u.timezone || '',
          status: u.status,
          bio: u.bio || ''
        }))
        this.totalCount = response.data?.total || 0
        this.page = response.data?.page || 1
        this.pageSize = response.data?.pageSize || 20
        this.isEmpty = this.users.length === 0
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch people'
        this.users = []
      } finally {
        this.isLoading = false
      }
    },
    async fetchProfileDetail(id) {
      this.isLoading = true
      this.error = null
      try {
        const response = await axiosClient.get(`/users/${id}`)
        const data = response.data?.data || response.data
        
        if (!data) {
          throw new Error('User not found')
        }

        this.currentUser = {
          id: data.id,
          fullName: data.fullName,
          email: data.email,
          avatar: data.avatarUrl || (data.fullName ? data.fullName.substring(0, 2).toUpperCase() : data.email.substring(0, 2).toUpperCase()),
          coverUrl: data.coverUrl,
          department: data.departmentName || 'N/A',
          position: data.jobTitle,
          team: data.team,
          status: data.status,
          bio: data.bio,
          location: data.location,
          timezone: data.timezone
        }
        
        this.linkedGoals = data.linkedGoals || []
        this.linkedProjects = data.linkedProjects || []
        this.kudos = data.kudos || []
        this.history = data.history || []
        
        this.isSuccess = true
      } catch (err) {
        this.error = err.message || 'Failed to fetch profile detail'
      } finally {
        this.isLoading = false
      }
    },
    async updateProfile(payload) {
      try {
        const response = await axiosClient.put('/users/profile', payload)
        return response.data
      } catch (err) {
        throw err
      }
    }
  }
})
