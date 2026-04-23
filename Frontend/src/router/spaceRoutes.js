export default [
  {
    path: '/spaces',
    name: 'ManageSpaces',
    component: () => import('../views/ManageSpaces.vue')
  },
  {
    path: '/space/:id',
    name: 'SpaceSummary',
    component: () => import('../views/SpaceSummary.vue')
  },
  {
    path: '/space/:id/cycles',
    name: 'CyclesView',
    component: () => import('../views/CyclesView.vue')
  },
  {
    path: '/space/:id/cycles/:cycleId',
    name: 'CycleDetailView',
    component: () => import('../views/SpaceSummary.vue')
  },
  {
    path: '/space/:id/intakes',
    name: 'IntakesView',
    component: () => import('../views/IntakesView.vue')
  },
  {
    path: '/space/:id/modules',
    name: 'ModulesView',
    component: () => import('../views/ModulesView.vue')
  },
  {
    path: '/space/:id/views',
    name: 'ViewsView',
    component: () => import('../views/ViewsView.vue')
  },
  {
    path: '/space/:id/pages',
    name: 'PagesView',
    component: () => import('../views/PagesView.vue')
  },
  {
    path: '/space/:id/settings',
    name: 'ProjectSettings',
    component: () => import('../views/ProjectSettingsView.vue')
  }
]
