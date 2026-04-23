export const PROJECT_ADMIN_WINDOW_NAME = 'SprintAProjectAdministrationWindow'

export const getProjectSettingsWindowName = (projectId) =>
  `SprintAProjectSettingsWindow_${projectId || 'default'}`

export const openNamedAppWindow = (href, name) => {
  const targetWindow = window.open(href, name)
  targetWindow?.focus?.()
  return targetWindow
}
