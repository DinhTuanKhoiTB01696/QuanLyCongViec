const CHANNEL_NAME = 'SprintAAdminRealtime'
const STORAGE_KEY = 'SprintAAdminRealtimeEvent'
const WINDOW_EVENT_NAME = 'sprinta-admin-realtime'
const SOURCE_ID = `${Date.now()}_${Math.random().toString(36).slice(2)}`

let broadcastChannel = null

const getBroadcastChannel = () => {
  if (typeof window === 'undefined' || typeof BroadcastChannel === 'undefined') {
    return null
  }

  if (!broadcastChannel) {
    broadcastChannel = new BroadcastChannel(CHANNEL_NAME)
  }

  return broadcastChannel
}

const normalizeMessage = (message, options = {}) => {
  const { allowOwnSource = false } = options
  if (!message || (!allowOwnSource && message.sourceId === SOURCE_ID) || !message.type) {
    return null
  }

  return message
}

const getMessageKey = (message) => {
  if (!message) return ''
  return `${message.sourceId || 'unknown'}:${message.timestamp || 0}:${message.type || 'unknown'}`
}

export const broadcastAdminRealtime = (type, payload = {}) => {
  const message = {
    type,
    payload,
    sourceId: SOURCE_ID,
    timestamp: Date.now()
  }

  const channel = getBroadcastChannel()
  channel?.postMessage(message)

  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent(WINDOW_EVENT_NAME, { detail: message }))
    // Keep the latest event in storage. Removing it immediately can cause
    // sibling tabs to only receive the remove event and miss the update.
    localStorage.setItem(STORAGE_KEY, JSON.stringify(message))
  }
}

export const subscribeAdminRealtime = (handler) => {
  if (typeof window === 'undefined' || typeof handler !== 'function') {
    return () => {}
  }

  let lastMessageKey = ''

  const handleMessage = (rawMessage, options = {}) => {
    const message = normalizeMessage(rawMessage, options)
    const messageKey = getMessageKey(message)
    if (!message || !messageKey || messageKey === lastMessageKey) {
      return
    }

    lastMessageKey = messageKey
    handler(message)
  }

  const onBroadcastMessage = (event) => {
    handleMessage(event?.data)
  }

  const onWindowMessage = (event) => {
    handleMessage(event?.detail, { allowOwnSource: true })
  }

  const onStorage = (event) => {
    if (event.key !== STORAGE_KEY || !event.newValue) {
      return
    }

    try {
      handleMessage(JSON.parse(event.newValue))
    } catch {
      // Ignore malformed sync events.
    }
  }

  const channel = getBroadcastChannel()
  channel?.addEventListener('message', onBroadcastMessage)
  window.addEventListener(WINDOW_EVENT_NAME, onWindowMessage)
  window.addEventListener('storage', onStorage)

  const intervalId = window.setInterval(() => {
    try {
      const rawValue = localStorage.getItem(STORAGE_KEY)
      if (!rawValue) return
      handleMessage(JSON.parse(rawValue))
    } catch {
      // Ignore malformed sync events.
    }
  }, 1000)

  return () => {
    channel?.removeEventListener('message', onBroadcastMessage)
    window.removeEventListener(WINDOW_EVENT_NAME, onWindowMessage)
    window.removeEventListener('storage', onStorage)
    window.clearInterval(intervalId)
  }
}
