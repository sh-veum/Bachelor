type MessageHandler = (data: any) => void

class WebSocketService {
  private socket: WebSocket | null = null
  private messageHandlers: Record<string, MessageHandler[]> = {}

  constructor(private url: string) {}

  connect() {
    console.log('connecting to websocket: ', this.url)
    if (!this.socket || this.socket.readyState === WebSocket.CLOSED) {
      this.socket = new WebSocket(this.url)
      this.socket.onmessage = (event) => this.handleMessage(event)
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
    console.log('disconnecting websocket')
    if (this.socket) {
      this.socket.close()
      console.log('websocket disconnected')
    }
  }

  isConnected() {
    return this.socket !== null && this.socket.readyState === WebSocket.OPEN
  }
}

export default new WebSocketService('ws://localhost:8088/ws')
