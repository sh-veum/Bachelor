namespace NetBackend.Services.Interfaces.MessageHandler;

public interface IMessageHandler
{
    Task HandleMessage(string message, string topic, long offset);
}