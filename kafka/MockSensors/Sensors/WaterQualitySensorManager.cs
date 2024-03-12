using System.Collections.Concurrent;

namespace MockSensors.Sensors;

public class WaterQualitySensorManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WaterQualitySensor> _logger;
    private readonly ConcurrentDictionary<string, WaterQualitySensor> _sensors = new();

    public WaterQualitySensorManager(IConfiguration configuration, ILogger<WaterQualitySensor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool TryStartSensor(string id)
    {
        var topic = $"water-quality-updates-{id}";
        var sensor = new WaterQualitySensor(_configuration, topic, _logger);
        if (_sensors.TryAdd(id, sensor))
        {
            sensor.Start();
            return true;
        }

        return false;
    }

    public bool TryStopSensor(string id)
    {
        if (_sensors.TryRemove(id, out var sensor))
        {
            sensor.Stop();
            return true;
        }

        return false;
    }
}