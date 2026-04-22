<template>
  <ErrorBoundary>
    <Suspense>
      <router-view />
      <template #fallback>
        <div class="app-loading-screen">
          <div class="loader-spinner"></div>
          <p>Đang tải giao diện...</p>
        </div>
      </template>
    </Suspense>
  </ErrorBoundary>
</template>

<script setup>
import { onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import ErrorBoundary from '@/components/ErrorBoundary.vue'
import { updateThemeAttributes } from '@/utils/theme'

const route = useRoute()

// Update theme attributes on route change to handle specialized pages (Login/Register)
watch(() => route.path, (newPath) => {
  updateThemeAttributes(newPath)
})

onMounted(async () => {
  // Initialize theme from localStorage first for immediate feedback
  const savedTheme = localStorage.getItem('theme') || 'light'
  document.documentElement.setAttribute('data-theme', savedTheme)

  // Sync with backend if logged in
  const token = localStorage.getItem('accessToken')
  if (token) {
    try {
      const res = await axiosClient.get('/settings/ThemeSettings')
      const data = res.data?.data
      if (data) {
        // Apply persisted theme tokens from backend
        // This ensures the user's specific color choices are respected globally
        const tokenMap = {
          'bgLayout': '--color-bg',
          'bgCard': '--color-surface',
          'textPrimary': '--color-text-primary',
          'borderColor': '--color-border',
          'accentColor': '--color-accent'
        }
        
        Object.entries(data).forEach(([key, value]) => {
          if (tokenMap[key] && value) {
            document.documentElement.style.setProperty(tokenMap[key], value)
          }
        })
      }
    } catch (e) {
      console.warn('Backend theme sync skipped or failed.')
    }
  }
})
</script>

<style>
/* Global Resets & Base Styles are in style.css */

.app-loading-screen {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
  font-family: 'Inter', sans-serif;
  gap: 16px;
}

.loader-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
