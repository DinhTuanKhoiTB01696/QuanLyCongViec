/**
 * Shared utility: Vietnamese relative time formatter
 * Handles null, undefined, 0001-01-01 (C# default), and future dates gracefully.
 *
 * @param {string|null|undefined} dateStr  - Primary date string (e.g. updatedAt)
 * @param {string|null|undefined} fallback - Fallback date string (e.g. createdAt)
 * @returns {string} Vietnamese relative time
 *
 * Examples:
 *   timeAgo(task.updatedAt, task.createdAt)
 *   timeAgo(task.createdAt)
 */
export const timeAgo = (dateStr, fallback = null) => {
  // Resolve a usable date string
  const resolve = (s) => {
    if (!s) return null
    let str = String(s).trim()
    // C# default DateTime / min value
    if (str.startsWith('0001-01-01') || str.startsWith('0001-01-01T')) return null

    // Check if it is an ISO string lacking a timezone designator
    if ((str.includes('T') || str.includes(' ')) && !str.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(str)) {
      str = str.replace(' ', 'T')
      str = `${str}Z`
    }

    const d = new Date(str)
    if (isNaN(d.getTime())) return null
    // Clearly bogus dates (year 1 to 1971 = likely default/epoch)
    if (d.getFullYear() <= 1970) return null
    return d
  }

  const date = resolve(dateStr) ?? resolve(fallback)

  if (!date) return 'Vừa xong'

  const now = new Date()
  const diffMs = now - date

  // Future date → show "Vừa xong" (clock skew tolerance)
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
