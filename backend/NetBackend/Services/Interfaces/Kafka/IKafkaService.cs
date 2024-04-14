using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces.Kafka;

public interface IKafkaService
{
    Task<(bool success, string message)> StartHistoricalConsumer(string sensorId, SensorType sensorType, string currentSessionId);
}