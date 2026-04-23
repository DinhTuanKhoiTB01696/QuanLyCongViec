import { createRouter, createWebHistory } from 'vue-router'
import homeRoutes from './homeRoutes'
import authRoutes from './authRoutes'
import dashboardRoutes from './dashboardRoutes'
import spaceRoutes from './spaceRoutes'
import aiRoutes from './aiRoutes'
import logRoutes from './logRoutes'
import adminRoutes from './adminRoutes'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    ...homeRoutes,
    ...authRoutes,
    ...dashboardRoutes,
    ...spaceRoutes,
    ...aiRoutes,
    ...logRoutes,
    ...adminRoutes
  ]
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('accessToken')
  // Cho phép các trang công khai không cần token
  const publicPages = ['/login', '/register', '/', '/auth/github/callback', '/accept-invite']
  const authRequired = !publicPages.includes(to.path)

  if (authRequired && !token) {
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }

  // Kiểm tra phân quyền theo role cho các route admin
  if (to.meta.requiredRoles && to.meta.requiredRoles.length > 0) {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}')
      const userRoles = (user.systemRoles || []).map(r => r.toLowerCase())
      const requiredRoles = (to.meta.requiredRoles || []).map(r => r.toLowerCase())
      const hasPermission = requiredRoles.some(role => userRoles.includes(role))

      if (!hasPermission) {
        return next({
          path: '/dashboard',
          query: { denied: to.fullPath }
        })
      }
    } catch (e) {
      return next({ path: '/dashboard' })
    }
  }

  next()
})

export default router
