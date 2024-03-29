namespace NetBackend.Services.Interfaces;

public interface IAppWebSocketManager
{
    Task SendMessageAsync(string message);
    Task HandleWebSocketAsync(System.Net.WebSockets.WebSocket webSocket);
}