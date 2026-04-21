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
    path: '/your-work',
    name: 'YourWork',
    component: () => import('../views/YourWorkView.vue')
  },
  {
    path: '/stickies',
    name: 'Stickies',
    component: () => import('../views/StickiesView.vue')
  },
  {
    path: '/rewards',
    name: 'Rewards',
    redirect: '/dashboard'
  },
  {
    path: '/drafts',
    name: 'Drafts',
    component: () => import('../views/DraftsView.vue')
  },
  {
    path: '/views',
    name: 'Views',
    component: () => import('../views/GlobalViewsView.vue')
  },
  {
    path: '/analytics',
    name: 'Analytics',
    component: () => import('../views/GlobalAnalyticsView.vue')
  },
  {
    path: '/archives',
    name: 'Archives',
    component: () => import('../views/GlobalArchivesView.vue')
  }
  // Recent route ĐÃ XÓA - Module chỉ chứa dữ liệu Mock, không có giá trị nghiệp vụ
]
