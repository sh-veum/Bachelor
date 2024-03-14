using NetBackend.Constants;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services;

public class SensorService : ISensorService
{
    private readonly ILogger<SensorService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWaterQualityConsumerService _waterQualityConsumerService;

    public SensorService(ILogger<SensorService> logger, IHttpClientFactory httpClientFactory, IWaterQualityConsumerService waterQualityConsumerService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _waterQualityConsumerService = waterQualityConsumerService;
    }

    public async Task<(bool success, string message)> StartWaterQualitySensorAsync(string sensorId)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");
        var response = await client.PostAsync($"sensors/waterQuality/startSensor/{sensorId}", null);

        var responseMessage = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Sensor {sensorId} started successfully.");

            var newTopic = $"{KafkaConstants.WaterQualityLogTopic}-{sensorId}";

            _logger.LogInformation($"Subscribing to topic: {newTopic}");
            _waterQualityConsumerService.SubscribeToTopic(newTopic);
            return (true, responseMessage);
        }
        else
        {
            _logger.LogError($"Failed to start sensor {sensorId}. Response: {response.StatusCode}, Message: {responseMessage}");
            return (false, responseMessage);
        }
    }

    public async Task<(bool success, string message)> StopWaterQualitySensorAsync(string sensorId)
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");
        var response = await client.PostAsync($"sensors/waterQuality/stopSensor/{sensorId}", null);
        var responseMessage = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Sensor {sensorId} stopped successfully.");
            return (true, responseMessage);
        }
        else
        {
            _logger.LogError($"Failed to stop sensor {sensorId}. Response: {response.StatusCode}, Message: {responseMessage}");
            return (false, responseMessage);
        }
    }

    public async Task<bool> StopAllSensorsAsync()
    {
        var client = _httpClientFactory.CreateClient("MockSensorClient");

        var response = await client.PostAsync($"sensors/waterQuality/stopAll", null!);
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
}