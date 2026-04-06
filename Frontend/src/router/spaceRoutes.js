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
  }
]
