import { language, setLanguage } from '@/i18n'

export const useLocale = () => {
  const toggleLocale = () => {
    setLanguage(language.value === 'vi' ? 'en' : 'vi')
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
