using System.Net.WebSockets;

namespace NetBackend.Services.Interfaces;

public interface IAppWebSocketManager
{
    Task SendMessageAsync(string message, IEnumerable<string> sessionIds);
    Task HandleWebSocketAsync(WebSocket webSocket, string topic, string sessionId);
    List<string> GetTopicSubscribers(string topic);
}