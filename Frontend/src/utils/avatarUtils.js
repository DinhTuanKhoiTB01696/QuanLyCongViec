export const getInitials = (name) => {
  if (!name) return '?'
  
  const parts = name.trim().split(/\s+/)
  
  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase()
  }
  
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
}

export const getAvatarColor = (name) => {
  const colors = [
    '#579dff', // Blue
    '#c97cf4', // Purple
    '#00b8d9', // Teal
    '#22a06b', // Green
    '#f5cd47', // Yellow
    '#e2483d', // Red
    '#ff8f73', // Orange
    '#8f90a6'  // Gray
  ]
  
  if (!name) return colors[0]
  
  // Calculate a consistent hash from the string
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  
  // Get positive index
  const index = Math.abs(hash) % colors.length
  return colors[index]
}
