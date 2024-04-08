using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces.MessageHandler;

namespace NetBackend.Services.MessageHandlers;

public class MessageHandlerFactory : IMessageHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MessageHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMessageHandler GetHandler(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.boat => _serviceProvider.GetRequiredService<BoatLocationMessageHandler>(),
            SensorType.waterQuality => _serviceProvider.GetRequiredService<WaterQualityMessageHandler>(),
            _ => throw new NotImplementedException()
        };
    }
}