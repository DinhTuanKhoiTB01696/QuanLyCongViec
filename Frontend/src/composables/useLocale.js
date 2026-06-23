import { language, setLanguage } from '@/i18n'
import { useI18nStore } from '@/store/useI18nStore'

export const useLocale = () => {
  const toggleLocale = () => {
    const next = language.value === 'vi' ? 'en' : 'vi'
    setLanguage(next)
    try {
      const i18nStore = useI18nStore()
      i18nStore.setLocale(next)
    } catch (e) {
      // Pinia might not be active yet
    }
  }

  const t = (en, vi) => {
    return language.value === 'vi' ? vi : en
  }

  const formatDateLocal = (isoString) => {
    if (!isoString) return ''
    const date = new Date(isoString)
    if (Number.isNaN(date.getTime())) return ''
    return date.toLocaleString(language.value === 'vi' ? 'vi-VN' : 'en-US')
  }

  return { locale: language, toggleLocale, t, formatDateLocal }
}
