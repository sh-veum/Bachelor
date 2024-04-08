using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces;

public interface ISensorConsumerService
{
    Task SubscribeToTopicAsync(string newTopic, SensorType sensorType, bool sendHistoricalData = false);
}