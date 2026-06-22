import { getStoredUser } from '@/utils/permissions'

const AVATAR_COLORS = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d']

export const getAvatarColor = (email, fullName) => {
  const str = fullName || (email ? email.split('@')[0] : 'User')
  const index = str.length % AVATAR_COLORS.length
  return AVATAR_COLORS[index]
}

export const getAvatarInitials = (email, fullName) => {
  if (email && email.includes('@')) {
    return email.split('@')[0].charAt(0).toUpperCase()
  }
  if (fullName) {
    return String(fullName).charAt(0).toUpperCase()
  }
  return 'U'
}

export const getCurrentUserAvatarInfo = () => {
  const currentUser = getStoredUser()
  const email = currentUser?.email || ''
  const fullName = currentUser?.fullName || currentUser?.name || currentUser?.publicName || ''
  return {
    name: fullName || email.split('@')[0] || 'User',
    email: email || 'user@example.com',
    initials: getAvatarInitials(email, fullName),
    color: getAvatarColor(email, fullName),
    avatarUrl: currentUser?.avatarUrl || ''
  }
}
