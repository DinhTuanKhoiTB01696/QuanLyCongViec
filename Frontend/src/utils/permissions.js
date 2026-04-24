import { getStoredUserSession } from '@/utils/authSession'

const normalizeRole = (role) => (role || '').trim().toLowerCase()

export const SYSTEM_ADMIN_ROLES = [
  'superadmin',
  'admin',
  'system admin',
  'organization admin',
  'accessadmin',
  'access admin'
]

export const ADMIN_USER_DIRECTORY_ROLES = [
  ...SYSTEM_ADMIN_ROLES
]

export const PROJECT_SETTINGS_ROLES = [
  'project_manager',
  'pm',
  'po',
  'admin',
  'scrum_master',
  'project_lead',
  'sm'
]

export const getStoredUser = () => {
  return getStoredUserSession()
}

export const getNormalizedSystemRoles = (user = getStoredUser()) => {
  const roles = Array.isArray(user?.systemRoles) ? user.systemRoles : []
  return roles.map(normalizeRole).filter(Boolean)
}

export const hasSystemAdminAccess = (user = getStoredUser()) => {
  const roles = getNormalizedSystemRoles(user)
  return roles.some(role => SYSTEM_ADMIN_ROLES.includes(role))
}

export const canAccessAdminUserDirectory = (user = getStoredUser()) => {
  const roles = getNormalizedSystemRoles(user)
  return roles.some(role => ADMIN_USER_DIRECTORY_ROLES.includes(role))
}

export const normalizeProjectRole = (role) => normalizeRole(role).replace(/[\s-]+/g, '_')

export const canAccessProjectSettings = (project, user = getStoredUser()) => {
  if (hasSystemAdminAccess(user)) {
    return true
  }

  const projectRole = normalizeProjectRole(project?.myRole || project?.MyRole || project?.projectRole || project?.ProjectRole)
  if (!projectRole) {
    return false
  }

  return PROJECT_SETTINGS_ROLES.includes(projectRole)
}

export const getProjectSettingsDeniedMessage = () => 'You do not have permission to manage this project.'
