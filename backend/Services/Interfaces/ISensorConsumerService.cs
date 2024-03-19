using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces;

public interface ISensorConsumerService
{
    void SubscribeToTopic(string newTopic, SensorType sensorType, bool sendHistoricalData = false);
}