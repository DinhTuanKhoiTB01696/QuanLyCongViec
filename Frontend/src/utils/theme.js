import { ref, watch } from 'vue'

const getInitialTheme = () => {
  const savedTheme = localStorage.getItem('theme')
  if (savedTheme) return savedTheme
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export const currentTheme = ref(getInitialTheme())

export const toggleTheme = (theme) => {
  if (theme) {
    currentTheme.value = theme
  } else {
    currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
  }
}

watch(currentTheme, (newTheme) => {
  if (newTheme === 'dark') {
    document.documentElement.classList.add('dark')
    document.documentElement.setAttribute('data-theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    document.documentElement.setAttribute('data-theme', 'light')
  }
  localStorage.setItem('theme', newTheme)
}, { immediate: true })
