/**
 * Shared utility: Vietnamese relative time formatter.
 * Treat ISO-like strings without timezone as local time to avoid a 7-hour
 * offset when the backend stores local DateTime values.
 */
export const timeAgo = (dateStr, fallback = null) => {
  const resolve = (value) => {
    if (!value) return null

    const raw = String(value).trim()
    if (!raw || raw.startsWith('0001-01-01')) return null

    const normalized = raw.replace(' ', 'T')
    const hasTimezone = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(normalized)
    const localIso = normalized.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{1,7}))?)?/)

    if (!hasTimezone && localIso) {
      const [, year, month, day, hour, minute, second = '0', fraction = '0'] = localIso
      const ms = Number(fraction.padEnd(3, '0').slice(0, 3))
      const localDate = new Date(
        Number(year),
        Number(month) - 1,
        Number(day),
        Number(hour),
        Number(minute),
        Number(second),
        ms
      )
      if (Number.isNaN(localDate.getTime()) || localDate.getFullYear() <= 1970) return null
      return localDate
    }

    const parsed = new Date(normalized)
    if (Number.isNaN(parsed.getTime()) || parsed.getFullYear() <= 1970) return null
    return parsed
  }

  const date = resolve(dateStr) ?? resolve(fallback)
  if (!date) return 'Vừa xong'

  const diffMs = Date.now() - date.getTime()
  if (diffMs < 0) return 'Vừa xong'

  const seconds = Math.floor(diffMs / 1000)
  if (seconds < 60) return 'Vừa xong'

  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes} phút trước`

  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} giờ trước`

  const days = Math.floor(hours / 24)
  if (days < 30) return `${days} ngày trước`

  const months = Math.floor(days / 30)
  if (months < 12) return `${months} tháng trước`

  const years = Math.floor(days / 365)
  return `${years} năm trước`
}
