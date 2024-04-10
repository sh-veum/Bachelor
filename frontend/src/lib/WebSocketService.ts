import { generateUniqueId } from './tools'

type MessageHandler = (data: any) => void

class WebSocketService {
  private socket: WebSocket | null = null
  private messageHandlers: Record<string, MessageHandler[]> = {}
  private currentTopic: string | null = null
  public sessionId: string = generateUniqueId()

  constructor(private url: string) {}

  connect(topic: string) {
    this.currentTopic = topic
    const topicURL = `${this.url}?topic=${encodeURIComponent(topic)}&sessionId=${encodeURIComponent(this.sessionId)}`

    console.log('Connecting to WebSocket: ', topicURL)
    if (!this.socket || this.socket.readyState === WebSocket.CLOSED) {
      this.socket = new WebSocket(topicURL)
      this.socket.onopen = () => {
        console.log('WebSocket connection established: ', topicURL)
      }
      this.socket.onmessage = (event) => this.handleMessage(event)
    }
  }

  updateTopic(topic: string) {
    if (this.currentTopic !== topic) {
      this.disconnect()
      this.connect(topic)
    }
  }

  subscribe(topic: string, handler: MessageHandler) {
    console.log('subscribing to', topic)
    if (!this.messageHandlers[topic]) {
      this.messageHandlers[topic] = []
    }
    this.messageHandlers[topic].push(handler)
  }

  unsubscribe(topic: string, handler: MessageHandler) {
    console.log('unsubscribing from', topic)
    if (this.messageHandlers[topic]) {
      this.messageHandlers[topic] = this.messageHandlers[topic].filter((h) => h !== handler)
    }
  }

  private handleMessage(event: MessageEvent) {
    console.log('received message', event.data)
    const parsedMessage = JSON.parse(event.data)

    // Ensure parsedMessage includes 'topic' and 'message'
    const handlers = this.messageHandlers[parsedMessage.topic]
    if (handlers && parsedMessage.topic) {
      handlers.forEach((handler) => handler(parsedMessage))
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

export default new WebSocketService('ws://localhost:8088/ws')
