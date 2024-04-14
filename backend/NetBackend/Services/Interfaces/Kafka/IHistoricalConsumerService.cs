using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces.Kafka;

public interface IHistoricalConsumerService
{
    void SubscribeToTopic(string newTopic, SensorType sensorType, string sessionId);
}