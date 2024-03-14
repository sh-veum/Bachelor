using System.Collections.Concurrent;
using MockSensors.Constants;
using MockSensors.Dto;

namespace MockSensors.Sensors;

public class WaterQualitySensorManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WaterQualitySensor> _logger;
    private readonly ConcurrentDictionary<string, WaterQualitySensor> _sensors = new();
    private readonly HashSet<string> _allSensorIds = new();

    public WaterQualitySensorManager(IConfiguration configuration, ILogger<WaterQualitySensor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool TryStartSensor(string id)
    {
        var topic = $"{TopicConstants.WaterQualityUpdates}-{id}";
        var sensor = new WaterQualitySensor(_configuration, topic, _logger);
        var added = _sensors.TryAdd(id, sensor);
        if (added)
        {
            _allSensorIds.Add(id); // Track sensor ID upon successful start
            sensor.Start();
        }
        return added;
    }

    public SensorStopResult TryStopSensor(string id)
    {
        if (_sensors.TryRemove(id, out var sensor))
        {
            sensor.Stop();
            return SensorStopResult.Stopped;
        }
        else if (_allSensorIds.Contains(id))
        {
            return SensorStopResult.AlreadyStopped;
        }
        else
        {
            return SensorStopResult.NotFound;
        }
    }


    public IEnumerable<SensorDto> GetActiveSensors()
    {
        return _sensors.Select(kv => new SensorDto(kv.Key, true)).ToList();
    }

    public IEnumerable<SensorDto> GetAllSensorsWithStatus()
    {
        return _allSensorIds.Select(id => new SensorDto(id, _sensors.ContainsKey(id))).ToList();
    }

    public void StopAllSensors()
    {
        foreach (var sensorId in _sensors.Keys.ToList())
        {
            if (_sensors.TryRemove(sensorId, out var sensor))
            {
                sensor.Stop();
            }
        }
    }
}