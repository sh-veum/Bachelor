using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services;

public class AppWebSocketManager : IAppWebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<string, List<string>> _subscriptions = new();
    private readonly ILogger<AppWebSocketManager> _logger;

    public AppWebSocketManager(ILogger<AppWebSocketManager> logger)
    {
        _logger = logger;
    }

    public async Task SendMessageAsync(string message, IEnumerable<string> sessionIds)
    {
        _logger.LogInformation($"Sending message to all sockets");
        foreach (var sessionId in sessionIds)
        {
            _logger.LogInformation($"Sending message to socket {sessionId}");
            if (_sockets.TryGetValue(sessionId, out WebSocket? socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }

    public void SubscribeSessionToTopic(string topic, string sessionId)
    {
        _logger.LogInformation($"Subscribing session {sessionId} to topic {topic}");
        _subscriptions.AddOrUpdate(topic, [sessionId], (key, existingSessionIds) =>
        {
            if (!existingSessionIds.Contains(sessionId))
            {
                existingSessionIds.Add(sessionId);
            }
            _logger.LogInformation($"Subscriptions for topic {topic}: {existingSessionIds.Count}");
            return existingSessionIds;
        });
    }

    public void UnsubscribeSessionFromTopic(string topic, string sessionId)
    {
        if (_subscriptions.TryGetValue(topic, out var sessionIds))
        {
            sessionIds.Remove(sessionId);
            if (!sessionIds.Any())
            {
                _subscriptions.TryRemove(topic, out _);
            }
        }
    }

    public async Task HandleWebSocketAsync(WebSocket webSocket, string topic, string sessionId)
    {
        _logger.LogInformation($"Handling WebSocket session {sessionId} with topic {topic}");
        _sockets.TryAdd(sessionId, webSocket);
        SubscribeSessionToTopic(topic, sessionId);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _sockets.TryRemove(sessionId, out _);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public List<string> GetTopicSubscribers(string topic)
    {
        return _subscriptions.TryGetValue(topic, out var sessionIds) ? sessionIds : [];
    }
}