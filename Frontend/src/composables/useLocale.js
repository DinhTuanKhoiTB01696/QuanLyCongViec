import { language, setLanguage } from '@/i18n'

const persistLocale = (locale) => {
  localStorage.setItem('app_language', locale)
  localStorage.setItem('admin_locale', locale)
  localStorage.setItem('sprinta_locale', locale)
}

export const useLocale = () => {
  const toggleLocale = () => {
    const nextLocale = language.value === 'vi' ? 'en' : 'vi'
    setLanguage(nextLocale)
    persistLocale(nextLocale)
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
