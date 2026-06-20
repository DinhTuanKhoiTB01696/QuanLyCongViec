import { computed } from 'vue'
import { language, t, setLanguage, getLanguage } from '@/i18n'

export const useI18n = () => {
  const isVietnamese = computed(() => language.value === 'vi')
  const isEnglish = computed(() => language.value === 'en')

  return {
    language,
    isVietnamese,
    isEnglish,
    t,
    setLanguage,
    getLanguage
  }
}

export default useI18n
