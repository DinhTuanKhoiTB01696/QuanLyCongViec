export default [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue')
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/Register.vue')
  },
  {
    path: '/auth/github/callback',
    name: 'GitHubCallback',
    component: () => import('../views/GitHubCallback.vue')
  },
  {
    path: '/accept-invite',
    name: 'AcceptInvite',
    component: () => import('../views/AcceptInvite.vue')
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('../views/ForgotPassword.vue')
  }
]
