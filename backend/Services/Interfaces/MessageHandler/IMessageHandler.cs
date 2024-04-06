namespace NetBackend.Services.Interfaces.MessageHandler;

public interface IMessageHandler
{
    Task HandleMessageAsync(string message, string topic, long offset);
}