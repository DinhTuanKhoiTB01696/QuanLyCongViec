import { ref } from 'vue'
import vi from './locales/vi'
import en from './locales/en'

const STORAGE_KEY = 'app_language'
const LEGACY_STORAGE_KEY = 'admin_locale'
const SUPPORTED_LANGUAGES = ['vi', 'en']
const dictionaries = { vi, en }

const normalizeLanguage = (language) => {
  return SUPPORTED_LANGUAGES.includes(language) ? language : 'vi'
}

const readInitialLanguage = () => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) return normalizeLanguage(stored)

    const legacy = localStorage.getItem(LEGACY_STORAGE_KEY)
    return normalizeLanguage(legacy || 'vi')
  } catch {
    return 'vi'
  }
}

export const language = ref(readInitialLanguage())

const persistLanguage = (nextLanguage) => {
  try {
    localStorage.setItem(STORAGE_KEY, nextLanguage)
  } catch {
    // Ignore storage failures so rendering never crashes.
  }
}

persistLanguage(language.value)

const resolvePath = (source, key) => {
  return key.split('.').reduce((current, part) => {
    if (current && Object.prototype.hasOwnProperty.call(current, part)) {
      return current[part]
    }
    return undefined
  }, source)
}

const interpolate = (value, params = {}) => {
  if (typeof value !== 'string') return value
  return value.replace(/\{(\w+)\}/g, (_, token) => {
    return Object.prototype.hasOwnProperty.call(params, token) ? `${params[token]}` : `{${token}}`
  })
}

export const getLanguage = () => language.value

export const setLanguage = (nextLanguage) => {
  const normalized = normalizeLanguage(nextLanguage)
  language.value = normalized
  persistLanguage(normalized)
  return normalized
}

export const t = (key, params = {}) => {
  const currentValue = resolvePath(dictionaries[language.value] || dictionaries.vi, key)
  const fallbackValue = resolvePath(dictionaries.vi, key) ?? resolvePath(dictionaries.en, key)
  const value = currentValue ?? fallbackValue
  return interpolate(value ?? key, params)
}

export const isSupportedLanguage = (nextLanguage) => SUPPORTED_LANGUAGES.includes(nextLanguage)

export default {
  language,
  t,
  setLanguage,
  getLanguage,
  isSupportedLanguage
}
