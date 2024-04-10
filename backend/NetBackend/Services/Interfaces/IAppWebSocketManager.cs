using System.Net.WebSockets;

namespace NetBackend.Services.Interfaces;

public interface IAppWebSocketManager
{
    Task SendMessageAsync(string message, string topic, string? currentSessionId = null);
    Task HandleWebSocketAsync(WebSocket webSocket, string topic, string sessionId);
}