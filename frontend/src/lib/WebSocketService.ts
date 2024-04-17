import { generateUniqueId } from './tools'

type MessageHandler = (data: any) => void

class WebSocketService {
  private socket: WebSocket | null = null
  private messageHandler: MessageHandler | null = null
  private currentTopic: string | null = null
  private currentHistorical: boolean = false
  public sessionId: string = generateUniqueId()

  constructor(private url: string) {}

  connect(topic: string, historical: boolean) {
    if (
      this.currentTopic === topic &&
      this.currentHistorical === historical &&
      this.isConnected()
    ) {
      console.log('WebSocket already connected with same topic and history flag.')
      return
    }

    this.disconnect() // Ensure old connections are closed before opening a new one

    this.currentTopic = topic
    this.currentHistorical = historical
    const topicURL = `${this.url}?topic=${encodeURIComponent(topic)}&sessionId=${encodeURIComponent(this.sessionId)}&historical=${encodeURIComponent(historical)}`

    console.log('Connecting to WebSocket: ', topicURL)
    this.socket = new WebSocket(topicURL)
    this.socket.onopen = () => console.log('WebSocket connection established: ', topicURL)
    this.socket.onmessage = (event) => this.handleMessage(event)
  }

  updateTopic(topic: string, historical: boolean) {
    if (this.currentTopic !== topic || this.currentHistorical !== historical) {
      this.disconnect()
      this.connect(topic, historical)
    }
  }

  setHandler(handler: MessageHandler) {
    console.log('Setting new message handler')
    this.messageHandler = handler
  }

  clearHandler() {
    console.log('Clearing message handler')
    this.messageHandler = null
  }

  private handleMessage(event: MessageEvent) {
    console.log('Received message', event.data)
    const parsedMessage = JSON.parse(event.data)

    if (this.messageHandler && parsedMessage.topic === this.currentTopic) {
      this.messageHandler(parsedMessage)
    }
  }

  disconnect() {
    if (this.socket) {
      console.log('Disconnecting WebSocket')
      this.socket.close()
      this.socket = null
      console.log('WebSocket disconnected')
    }
  }

  isConnected() {
    return this.socket !== null && this.socket.readyState === WebSocket.OPEN
  }
}

export default new WebSocketService(`${import.meta.env.VITE_VUE_APP_API_WEBSOCKET_URL}`)
