using NetBackend.Constants;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces.Kafka;

namespace NetBackend.Services.Kafka;

public class SensorService : ISensorService
{
    private readonly ILogger<SensorService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKafkaConsumerService _sensorConsumerService;

    public SensorService(ILogger<SensorService> logger, IHttpClientFactory httpClientFactory, IKafkaConsumerService sensorConsumerService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _sensorConsumerService = sensorConsumerService;
    }

    public async Task<(bool success, string message)> StartSensorAsync(string sensorId, SensorType sensorType, bool sendHistoricalData = false, string? currentSessionId = null)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");
        var response = await client.PostAsync($"sensors/{sensorType}/startSensor/{sensorId}", null);

        var responseMessage = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Sensor {sensorId} started successfully.");

            string? newTopic;

            if (sensorType == SensorType.waterQuality)
            {
                newTopic = $"{KafkaConstants.WaterQualityLogTopic}-{sensorId}";

                _sensorConsumerService.SubscribeToTopic(newTopic, SensorType.waterQuality);
            }
            else if (sensorType == SensorType.boat)
            {
                newTopic = $"{KafkaConstants.BoatLogTopic}-{sensorId}";

                _sensorConsumerService.SubscribeToTopic(newTopic, SensorType.boat);
            }

            return (true, responseMessage);
        }
        else
        {
            _logger.LogError($"Failed to start sensor {sensorId}. Response: {response.StatusCode}, Message: {responseMessage}");
            return (false, responseMessage);
        }
    }

    public async Task<(bool success, string message)> StopSensorAsync(string sensorId, SensorType sensorType)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");
        var response = await client.PostAsync($"sensors/{sensorType}/stopSensor/{sensorId}", null);
        var responseMessage = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"{sensorType} sensor {sensorId} stopped successfully.");
            return (true, responseMessage);
        }
        else
        {
            _logger.LogError($"Failed to stop {sensorType} sensor {sensorId}. Response: {response.StatusCode}, Message: {responseMessage}");
            return (false, responseMessage);
        }
    }

    public async Task<bool> StopAllSensorsAsync(SensorType sensorType)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");

        var response = await client.PostAsync($"sensors/{sensorType}/stopAll", null!);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Sensors stopped successfully.");
            return true;
        }
        else
        {
            _logger.LogError($"Failed to stop sensors. Response: {response.StatusCode}");
            return false;
        }
    }

    public async Task<(bool success, string message)> GetActiveSensors(SensorType sensorType)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");
        var response = await client.GetAsync($"sensors/{sensorType}/activeSensors");
        var responseMessage = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Sensors retrieved successfully.");
            return (true, responseMessage);
        }
        else
        {
            _logger.LogError($"Failed to retrieve sensors. Response: {response.StatusCode}");
            return (false, responseMessage);
        }
    }
}