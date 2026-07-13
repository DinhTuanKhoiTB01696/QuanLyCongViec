import { defineStore } from 'pinia'

const readPosition = () => {
  try {
    const saved = JSON.parse(localStorage.getItem('sprinta-ai-pet-position') || 'null')
    return saved && Number.isFinite(saved.x) && Number.isFinite(saved.y) ? saved : { x: 24, y: 180 }
  } catch {
    return { x: 24, y: 180 }
  }
}

export const useAiPetStore = defineStore('aiPet', {
  state: () => ({
    isPanelOpen: false,
    isPinned: localStorage.getItem('sprinta-ai-pet-pinned') !== 'false',
    position: readPosition()
  }),
  actions: {
    setPanelOpen(value) { this.isPanelOpen = Boolean(value) },
    togglePanel() { this.isPanelOpen = !this.isPanelOpen },
    setPinned(value) {
      this.isPinned = Boolean(value)
      localStorage.setItem('sprinta-ai-pet-pinned', String(this.isPinned))
    },
    setPosition(value) {
      this.position = { ...this.position, ...value }
      localStorage.setItem('sprinta-ai-pet-position', JSON.stringify(this.position))
    }
  }
})
