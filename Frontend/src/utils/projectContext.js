const SESSION_PROJECT_KEY = 'currentProjectId'
const LOCAL_PROJECT_KEY = 'currentProjectId'
const LAST_PROJECT_KEY = 'lastProjectId'

export const getScopedCurrentProjectId = () => {
  if (typeof window === 'undefined') return ''

  return (
    window.sessionStorage.getItem(SESSION_PROJECT_KEY)
    || window.localStorage.getItem(LOCAL_PROJECT_KEY)
    || window.localStorage.getItem(LAST_PROJECT_KEY)
    || ''
  )
}

export const setScopedCurrentProjectId = (projectId, options = {}) => {
  if (typeof window === 'undefined') return

  const {
    rememberLast = true,
    syncLegacyLocal = false
  } = options

  if (!projectId) {
    window.sessionStorage.removeItem(SESSION_PROJECT_KEY)
    if (syncLegacyLocal) {
      window.localStorage.removeItem(LOCAL_PROJECT_KEY)
    }
    return
  }

  window.sessionStorage.setItem(SESSION_PROJECT_KEY, projectId)

  if (rememberLast) {
    window.localStorage.setItem(LAST_PROJECT_KEY, projectId)
  }

  if (syncLegacyLocal) {
    window.localStorage.setItem(LOCAL_PROJECT_KEY, projectId)
  } else {
    window.localStorage.removeItem(LOCAL_PROJECT_KEY)
  }
}

export const clearScopedCurrentProjectId = () => {
  if (typeof window === 'undefined') return

  window.sessionStorage.removeItem(SESSION_PROJECT_KEY)
  window.localStorage.removeItem(LOCAL_PROJECT_KEY)
  window.localStorage.removeItem(LAST_PROJECT_KEY)
}
