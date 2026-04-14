// Các role được phép truy cập trang Admin (Audit Log, User Management)
const adminAllowedRoles = ['System Admin', 'PM', 'PO']

export default [
  {
    path: '/admin/audit-log',
    name: 'AdminAuditLog',
    component: () => import('../views/admin/AuditLog.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/users',
    name: 'AdminUserManagement',
    component: () => import('../views/admin/UserManagement.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/organization/profile',
    name: 'AdminOrgProfile',
    component: () => import('../views/admin/OrganizationProfile.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/organization/contact',
    name: 'AdminOrgContact',
    component: () => import('../views/admin/OrganizationContact.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/configuration',
    name: 'AdminConfiguration',
    component: () => import('../views/admin/Configuration.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/security/2fa',
    name: 'AdminSecurity2FA',
    component: () => import('../views/admin/security/TwoFactorAuth.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/security/password',
    name: 'AdminSecurityPassword',
    component: () => import('../views/admin/security/ChangePassword.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  },
  {
    path: '/admin/security/ip-whitelist',
    name: 'AdminSecurityIpWhitelist',
    component: () => import('../views/admin/security/IpWhitelist.vue'),
    meta: { requiredRoles: adminAllowedRoles }
  }
]
