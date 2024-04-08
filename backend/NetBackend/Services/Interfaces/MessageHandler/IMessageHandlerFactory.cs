using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces.MessageHandler;

public interface IMessageHandlerFactory
{
    IMessageHandler GetHandler(SensorType sensorType);
}
