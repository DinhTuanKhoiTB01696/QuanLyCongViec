import { ref, watch } from 'vue'

const EXCLUDED_ROUTES = ['/AcceptInvite']

const getInitialTheme = () => {
  const savedTheme = localStorage.getItem('theme')
  if (savedTheme) return savedTheme
  return 'light' // Force light theme as default for Atlassian Jira style
}

export const currentTheme = ref(getInitialTheme())

export const toggleTheme = (theme) => {
  if (theme) {
    currentTheme.value = theme
  } else {
    currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
  }
}

// Function to update document attributes based on theme and route
export const updateThemeAttributes = (path) => {
  const isExcluded = EXCLUDED_ROUTES.some(route => path === route || path.startsWith(route + '?'))
  const themeToApply = isExcluded ? 'light' : currentTheme.value
  
  document.documentElement.setAttribute('data-theme', themeToApply)
  if (themeToApply === 'dark') {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
}

// Initialize watcher
watch(currentTheme, () => {
  const path = window.location.pathname
  updateThemeAttributes(path)
  localStorage.setItem('theme', currentTheme.value)
}, { immediate: true })
