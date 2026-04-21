import { defineStore } from 'pinia'
import axiosClient from '@/api/axiosClient'

const defaultProjectNodes = (projectId) => ([
  {
    id: `${projectId}-work-items`,
    key: 'work-items',
    label: 'Work items',
    route: `/space/${projectId}`
  },
  {
    id: `${projectId}-cycles`,
    key: 'cycles',
    label: 'Cycles',
    route: `/space/${projectId}/cycles`
  },
  {
    id: `${projectId}-modules`,
    key: 'modules',
    label: 'Modules',
    route: `/space/${projectId}/modules`
  },
  {
    id: `${projectId}-views`,
    key: 'views',
    label: 'Views',
    route: `/space/${projectId}/views`
  },
  {
    id: `${projectId}-pages`,
    key: 'pages',
    label: 'Pages',
    route: `/space/${projectId}/pages`
  }
])

const mapProjectRow = (project) => ({
  id: project.id,
  name: project.name,
  key: project.key || project.identifier || project.name?.substring(0, 4)?.toUpperCase() || 'PRJ',
  description: project.description || '',
  icon: project.icon || null,
  cover: project.cover || null,
  networkType: project.networkType || 'Public',
  leadName: project.leadName || project.creatorName || 'Project',
  isMember: typeof project.isMember === 'boolean' ? project.isMember : true,
  isFavorite: Boolean(project.isFavorite),
  myRole: project.myRole || null,
  createdAt: project.createdAt || null,
  updatedAt: project.updatedAt || null,
  children: defaultProjectNodes(project.id),
  originalRow: project
})

export const useProjectStore = defineStore('project', {
  state: () => ({
    currentProject: null,
    allProjects: [],
    expandedProjectIds: [],
    loading: false,
    error: null,
    members: [],
    tags: []
  }),
  getters: {
    sidebarProjects: (state) => state.allProjects.filter(project => project.isMember !== false),
    favoriteProjects: (state) => state.allProjects.filter(project => project.isFavorite),
    projectTree: (state) => state.allProjects.map(project => ({
      ...project,
      expanded: state.expandedProjectIds.includes(project.id)
    }))
  },
  actions: {
    async fetchAllProjects(force = false) {
      if (!force && this.allProjects.length > 0) return this.allProjects;

      this.loading = true;
      this.error = null;
      try {
        const response = await axiosClient.get('/projects/discovery');
        const rows = response.data?.data || response.data || [];
        this.allProjects = rows.map(mapProjectRow);
        return this.allProjects;
      } catch (err) {
        this.error = err.message || 'Failed to fetch projects';
        console.error('Failed to fetch all projects:', err);
        return [];
      } finally {
        this.loading = false;
      }
    },
    toggleProject(projectId) {
      if (!projectId) return;
      if (this.expandedProjectIds.includes(projectId)) {
        this.expandedProjectIds = this.expandedProjectIds.filter(id => id !== projectId);
        return;
      }
      this.expandedProjectIds = [...this.expandedProjectIds, projectId];
    },
    expandProject(projectId) {
      if (!projectId || this.expandedProjectIds.includes(projectId)) return;
      this.expandedProjectIds = [...this.expandedProjectIds, projectId];
    },
    collapseProject(projectId) {
      this.expandedProjectIds = this.expandedProjectIds.filter(id => id !== projectId);
    },
    async updateFavorite(projectId, favorite) {
      await axiosClient.put(`/projects/${projectId}/favorite`, { favorite });
      const index = this.allProjects.findIndex(project => project.id === projectId);
      if (index >= 0) {
        const current = this.allProjects[index];
        this.allProjects.splice(index, 1, { ...current, isFavorite: favorite });
      }
      if (this.currentProject?.id === projectId) {
        this.currentProject = { ...this.currentProject, isFavorite: favorite };
      }
    },
    async fetchProjectDetails(projectId) {
      if (!projectId) return;
      this.loading = true;
      this.error = null;
      try {
        const [projRes, membersRes, tagsRes] = await Promise.all([
          axiosClient.get(`/projects/${projectId}`),
          axiosClient.get(`/projects/${projectId}/members`),
          axiosClient.get(`/projects/${projectId}/labels`)
        ]);
        this.currentProject = projRes.data?.data || null;
        this.members = membersRes.data?.data || [];
        this.tags = tagsRes.data?.data || [];

        const mappedProject = mapProjectRow(this.currentProject);
        const existingIndex = this.allProjects.findIndex(project => project.id === mappedProject.id);
        if (existingIndex >= 0) {
          this.allProjects.splice(existingIndex, 1, {
            ...this.allProjects[existingIndex],
            ...mappedProject
          });
        } else {
          this.allProjects = [...this.allProjects, mappedProject];
        }
      } catch (err) {
        this.error = err.message || 'Failed to fetch project details';
        console.error('Failed to fetch project details:', err);
      } finally {
        this.loading = false;
      }
    }
  }
})
