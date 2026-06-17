import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

export const useSiteStore = defineStore('site', {
  state: () => ({
    recentSite: null,
    sites: [],
    loading: false,
    error: null
  }),
  actions: {
    async fetchSites() {
      this.loading = true
      this.error = null
      try {
        const response = await axiosClient.get('/workspaces')
        this.sites = response.data?.data || []
        
        // Find recent site based on most recently created or some local storage logic
        if (this.sites.length > 0) {
          const recentId = localStorage.getItem('recent_site_id')
          this.recentSite = this.sites.find(s => s.id === recentId) || this.sites[0]
        }
      } catch (err) {
        this.error = err.message || 'Failed to fetch sites'
      } finally {
        this.loading = false
      }
    },
    async createSite(siteData) {
      this.loading = true
      try {
        const payload = {
          name: siteData.name,
          slug: siteData.slug || siteData.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, ''),
          timezone: 'Asia/Ho_Chi_Minh'
        }
        const response = await axiosClient.post('/workspaces', payload)
        const newSite = response.data?.data
        if (newSite) {
          this.sites.push(newSite)
          this.setRecentSite(newSite)
        }
        return newSite
      } catch (err) {
        throw err
      } finally {
        this.loading = false
      }
    },
    setRecentSite(site) {
      if (!site) return
      this.recentSite = site
      localStorage.setItem('recent_site_id', site.id)
    }
  }
})
