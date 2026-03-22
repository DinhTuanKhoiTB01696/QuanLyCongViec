import * as signalR from '@microsoft/signalr'

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
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/kanban-hub', {
        // Use default options, or customize if needed
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build()

    try {
      await this.connection.start()
      console.log('SignalR Connected.')
      await this.connection.invoke('JoinProjectGroup', projectId)
    } catch (err) {
      console.error('SignalR Connection Error: ', err)
      setTimeout(() => this.startConnection(projectId), 5000)
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
      console.log('SignalR Disconnected.')
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
}

export const signalRService = new SignalRService()
