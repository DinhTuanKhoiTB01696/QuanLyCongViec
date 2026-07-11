export const EMPTY_GUID = '00000000-0000-0000-0000-000000000000'

export const isValidEntityId = (value) => {
  const id = `${value || ''}`.trim()
  return id.length >= 36 && id !== EMPTY_GUID && id !== 'default' && id !== '1'
}

export const resolveWorkspaceIdFromState = ({ explicitId = null, siteStore = null, project = null } = {}) => {
  if (isValidEntityId(explicitId)) return explicitId

  const projectWorkspaceId = project?.workspaceId || project?.WorkspaceId || project?.originalRow?.workspaceId || project?.originalRow?.WorkspaceId
  if (isValidEntityId(projectWorkspaceId)) return projectWorkspaceId

  const activeSiteId = siteStore?.activeSite?.id || siteStore?.activeSite?.Id
  if (isValidEntityId(activeSiteId)) return activeSiteId

  const recentSiteId = siteStore?.recentSite?.id || siteStore?.recentSite?.Id
  if (isValidEntityId(recentSiteId)) return recentSiteId

  const firstSiteId = siteStore?.sites?.[0]?.id || siteStore?.sites?.[0]?.Id
  if (isValidEntityId(firstSiteId)) return firstSiteId

  return null
}

export const ensureWorkspaceIdFromState = async ({ explicitId = null, siteStore = null, project = null } = {}) => {
  const resolvedId = resolveWorkspaceIdFromState({ explicitId, siteStore, project })
  if (resolvedId || !siteStore?.fetchSites) return resolvedId

  await siteStore.fetchSites()

  return resolveWorkspaceIdFromState({ explicitId, siteStore, project })
}
