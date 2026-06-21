import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'
import { getStoredUser } from '@/utils/permissions'

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
    isSuccess: false
  }),
  actions: {
    async fetchPeople() {
      this.isLoading = true
      this.error = null
      this.isEmpty = false
      this.isSuccess = false
      try {
        let data = []
        try {
          const response = await axiosClient.get('/users/directory')
          data = response.data?.data || response.data || []
        } catch (apiErr) {
          console.warn("Failed to fetch /users/directory, falling back to mock data:", apiErr)
        }
        
        const currentUserData = getStoredUser()
        const currentEmail = currentUserData?.email || 'user@example.com'
        const currentName = currentUserData?.fullName || currentUserData?.name || currentUserData?.publicName || currentUserData?.username || currentEmail.split('@')[0]

        let fetchedUsers = Array.isArray(data) ? data : []
        
        // Ensure current user is at the top
        const currentIdx = fetchedUsers.findIndex(u => u.id === currentUserData?.id || u.email === currentEmail)
        if (currentIdx > 0) {
          const [curr] = fetchedUsers.splice(currentIdx, 1)
          fetchedUsers.unshift(curr)
        } else if (currentIdx === -1 && currentUserData) {
          fetchedUsers.unshift({
             id: currentUserData.id || 'u_curr',
             fullName: currentName,
             email: currentEmail,
             location: 'Việt Nam',
             roles: ['Lập trình viên']
          })
        }

        this.users = fetchedUsers.map(u => ({
          id: u.id || Math.random().toString(36).substr(2, 9),
          fullName: u.fullName || u.name || u.email || 'User',
          email: u.email || '',
          location: u.location || '',
          avatar: u.avatar || String(u.fullName || u.name || u.email || 'U').substring(0, 2).toUpperCase(),
          department: u.departments && u.departments.length > 0 ? u.departments[0] : (u.department || 'N/A'),
          position: u.roles && u.roles.length > 0 ? u.roles[0] : (u.position || 'Member'),
          team: u.projects && u.projects.length > 0 ? u.projects[0] : (u.team || 'N/A'),
          status: u.status,
          bio: u.bio || 'Passionate team member.'
        }))
        this.isEmpty = this.users.length === 0
        this.isSuccess = true
      } catch (err) {
        console.error("fetchPeople Error:", err)
        this.error = err.message || 'Failed to fetch people'
        // Fallback to empty only on critical fatal errors outside the API call
        this.users = []
      } finally {
        this.isLoading = false
      }
    },
    async fetchProfileDetail(id) {
      this.isLoading = true
      this.error = null
      try {
        const response = await axiosClient.get(`/users/directory/${id}`)
        const data = response.data?.data || response.data
        
        if (!data) {
          throw new Error('User not found')
        }

        this.currentUser = {
          id: data.id,
          fullName: data.fullName,
          email: data.email,
          avatar: data.avatar || (data.fullName ? data.fullName.substring(0, 2).toUpperCase() : data.email.substring(0, 2).toUpperCase()),
          department: data.department,
          position: data.position,
          team: data.team,
          status: data.status,
          bio: data.bio
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
    async inviteUsers(workspaceId, emails) {
      if (!workspaceId) throw new Error('Workspace ID is required to invite users')
      if (!emails || emails.length === 0) return

      try {
        const promises = emails.map(email => 
          axiosClient.post(`/workspaces/${workspaceId}/members/invite`, { email })
        )
        await Promise.all(promises)
      } catch (err) {
        if (err.response && err.response.status === 403) {
          throw new Error('Bạn không có quyền mời thành viên vào site này.')
        }
        throw err
      }
    }
  }
})
