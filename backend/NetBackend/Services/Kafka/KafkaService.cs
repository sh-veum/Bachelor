using NetBackend.Constants;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;

namespace NetBackend.Services;

public class KafkaService : IKafkaService
{
    private readonly ILogger<KafkaService> _logger;
    private readonly IHistoricalConsumerService _historicalConsumerService;

    public KafkaService(ILogger<KafkaService> logger, IHistoricalConsumerService historicalConsumerService)
    {
        _logger = logger;
        _historicalConsumerService = historicalConsumerService;
    }

    public Task<(bool success, string message)> StartHistoricalConsumer(string sensorId, SensorType sensorType, string currentSessionId)
    {
        if (sensorType == SensorType.none)
        {
            string newTopic = $"{KafkaConstants.BoatLogTopic}-{sensorId}";

            _historicalConsumerService.SubscribeToTopic(newTopic, SensorType.none, currentSessionId);
        }
        else if (sensorType == SensorType.waterQuality)
        {
            string newTopic = $"{KafkaConstants.WaterQualityLogTopic}-{sensorId}";

            _historicalConsumerService.SubscribeToTopic(newTopic, SensorType.waterQuality, currentSessionId);
        }
        else if (sensorType == SensorType.boat)
        {
            string newTopic = $"{KafkaConstants.BoatLogTopic}-{sensorId}";

            _historicalConsumerService.SubscribeToTopic(newTopic, SensorType.boat, currentSessionId);
        }
        else
        {
            _logger.LogError($"Failed to start historical consumer for sensor {sensorId}.");
            return Task.FromResult((false, "Failed to start historical consumer. Sensor type not recognized."));
        }

        return Task.FromResult((true, "Historical consumer started successfully."));
    }
}