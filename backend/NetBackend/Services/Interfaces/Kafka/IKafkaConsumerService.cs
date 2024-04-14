using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces.Kafka;

public interface IKafkaConsumerService
{
    void SubscribeToTopic(string newTopic, SensorType sensorType);
}