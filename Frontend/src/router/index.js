import { createRouter, createWebHistory } from 'vue-router'
import homeRoutes from './homeRoutes'
import authRoutes from './authRoutes'
import dashboardRoutes from './dashboardRoutes'
import spaceRoutes from './spaceRoutes'
import aiRoutes from './aiRoutes'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    ...homeRoutes,
    ...authRoutes,
    ...dashboardRoutes,
    ...spaceRoutes,
    ...aiRoutes
  ]
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('accessToken')
  // Cho phép các trang công khai không cần token
  const publicPages = ['/login', '/register', '/', '/auth/github/callback']
  const authRequired = !publicPages.includes(to.path)

  if (authRequired && !token) {
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }

  next()
})

export default router
