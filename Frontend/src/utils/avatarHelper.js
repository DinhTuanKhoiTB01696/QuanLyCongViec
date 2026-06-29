/**
 * Fallback utility for generating Avatar deterministic values
 * This is used ONLY when the backend hasn't provided the values
 */

const AvatarColors = [
    "#579dff", "#c97cf4", "#00b8d9", "#22a06b", "#f5cd47", "#e2483d"
]

export function getAvatarColor(identifier) {
    if (!identifier) return '#0052cc' // Fallback blue
    
    let hash = 0
    for (let i = 0; i < identifier.length; i++) {
        const char = identifier.charCodeAt(i)
        hash = char + ((hash << 5) - hash)
    }
    
    return AvatarColors[Math.abs(hash) % AvatarColors.length]
}

export function getInitials(fullName, email) {
    const source = fullName?.trim() || email?.trim()
    if (!source) return 'U'

    // If it's an email
    if (source === email) {
        const prefix = email.split('@')[0]
        const match = prefix.match(/\p{L}/u)
        return match ? match[0].toUpperCase() : 'U'
    }

    // If it's a full name
    const words = source.split(/\s+/)
    let initials = ''
    let count = 0

    for (const word of words) {
        const match = word.match(/\p{L}/u)
        if (match) {
            initials += match[0].toUpperCase()
            count++
        }
        if (count >= 2) break
    }

    return initials || 'U'
}
