import * as signalR from '@microsoft/signalr'
import { isExpectedNetworkError } from '@/utils/errorTelemetry'
import { getStoredAccessToken } from '@/utils/authSession'

class SignalRService {
  constructor() {
    this.connection = null
    this.projectId = null
  }

  async startConnection(projectId) {
    if (this.connection && this.projectId === projectId) return
    
    if (this.connection) {
      await this.stopConnection()
    }

    this.projectId = projectId
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5136/api'
    const hubBaseUrl = apiBaseUrl.replace(/\/api\/?$/, '')
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubBaseUrl}/kanban-hub`, {
        accessTokenFactory: () => getStoredAccessToken() || ''
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(signalR.LogLevel.None)
      .build()

    this.connection.onreconnected(async () => {
      if (this.connection && this.projectId) {
        try {
          await this.connection.invoke('JoinProjectGroup', this.projectId)
        } catch (err) {
          if (!isExpectedNetworkError(err)) {
            console.error('SignalR rejoin group error:', err)
          }
        }
      }
    })

    try {
      await this.connection.start()
      await this.connection.invoke('JoinProjectGroup', projectId)
    } catch (err) {
      if (!isExpectedNetworkError(err) && !(err.message && err.message.includes('405'))) {
        console.error('SignalR Connection Error:', err)
      }
      // Don't retry if hub is not available
      this.connection = null
      this.projectId = null
    }
  }

  async stopConnection() {
    if (this.connection) {
      if (this.projectId) {
        await this.connection.invoke('LeaveProjectGroup', this.projectId)
      }
      await this.connection.stop()
      this.connection = null
      this.projectId = null
    }
  }

  on(eventName, callback) {
    if (this.connection) {
      this.connection.on(eventName, callback)
    }
  }

  off(eventName, callback) {
    if (this.connection) {
      this.connection.off(eventName, callback)
    }
  }

  async sendProjectEvent(projectId, type, payload = {}) {
    if (!projectId || !type) return

    if (!this.connection || this.projectId !== projectId) {
      await this.startConnection(projectId)
    }

    if (!this.connection) return

    try {
      await this.connection.invoke('BroadcastProjectEvent', projectId, type, JSON.stringify(payload || {}))
    } catch (err) {
      if (!isExpectedNetworkError(err)) {
        console.error('SignalR project event error:', err)
      }
    }
  }
}

export const signalRService = new SignalRService()
