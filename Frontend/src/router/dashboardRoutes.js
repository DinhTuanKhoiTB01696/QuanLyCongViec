export default [
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue')
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('../views/Profile.vue')
  },
  {
    path: '/user-management',
    name: 'UserManagement',
    component: () => import('../views/UserManagement.vue')
  }
]
