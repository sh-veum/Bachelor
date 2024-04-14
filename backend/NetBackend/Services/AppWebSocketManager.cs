using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services;

public class AppWebSocketManager : IAppWebSocketManager
{
    private readonly ILogger<AppWebSocketManager> _logger;
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<string, List<(string sessionId, bool historical)>> _subscriptions = new();

    public AppWebSocketManager(ILogger<AppWebSocketManager> logger)
    {
        _logger = logger;
    }

    public async Task SendMessageAsync(string message, string topic, string? currentSessionId = null)
    {
        if (_subscriptions.TryGetValue(topic, out var subscriptions))
        {
            foreach (var (sessionId, historical) in subscriptions)
            {
                if (_sockets.TryGetValue(sessionId, out var socket) && socket?.State == WebSocketState.Open)
                {
                    bool shouldSend = (currentSessionId != null && historical) || (currentSessionId == null && !historical);
                    if (shouldSend)
                    {
                        var buffer = Encoding.UTF8.GetBytes(message);
                        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        _logger.LogInformation($"Message sent to session {sessionId} under topic {topic} with historical setting {historical}.");
                    }
                }
            }
        }
    }

    public async Task HandleWebSocketAsync(WebSocket webSocket, string topic, string sessionId, bool historical)
    {
        _logger.LogInformation($"Handling WebSocket session {sessionId} with topic {topic} and historical setting {historical}");
        _sockets.TryAdd(sessionId, webSocket);

        _subscriptions.AddOrUpdate(topic, new List<(string, bool)> { (sessionId, historical) }, (key, existing) =>
        {
            // Remove existing subscription with the same sessionId but different historical flag
            var oppositeSubscription = existing.FirstOrDefault(sub => sub.sessionId == sessionId && sub.historical != historical);
            if (oppositeSubscription != default)
            {
                existing.Remove(oppositeSubscription);
            }

            // Add new subscription if it does not already exist
            if (!existing.Any(sub => sub.sessionId == sessionId && sub.historical == historical))
            {
                existing.Add((sessionId, historical));
            }
            return existing;
        });

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _sockets.TryRemove(sessionId, out _);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}