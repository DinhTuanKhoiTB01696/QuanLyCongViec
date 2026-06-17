export default [
  {
    path: '/spaces',
    name: 'ManageSpaces',
    component: () => import('../views/ManageSpaces.vue')
  },
  {
    path: '/spaces/trash',
    name: 'GlobalTrashView',
    component: () => import('../views/GlobalTrashView.vue')
  },
  {
    path: '/spaces/categories',
    name: 'SpaceCategories',
    component: () => import('../views/SpaceCategories.vue')
  },
  {
    path: '/spaces/archive',
    name: 'GlobalArchivesView',
    component: () => import('../views/GlobalArchivesView.vue')
  },
  {
    path: '/archives',
    redirect: '/spaces/archive'
  },
  {
    path: '/space/:id',
    name: 'SpaceSummaryRedirect',
    redirect: to => {
      return `/space/${to.params.id}/dashboard`
    }
  },
  {
    path: '/space/:id/work-items',
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
    path: '/space/:id/reports',
    name: 'ReportsView',
    component: () => import('../views/ReportsView.vue')
  },
  {
    path: '/space/:id/dashboard',
    name: 'SpaceDashboard',
    component: () => import('../views/SpaceDashboard.vue')
  },
  {
    path: '/space/:id/settings',
    name: 'ProjectSettings',
    component: () => import('../views/ProjectSettings.vue'),
    meta: { requiresProjectSettingsAccess: true }
  }
]
