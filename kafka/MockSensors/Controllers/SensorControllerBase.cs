using Microsoft.AspNetCore.Mvc;
using MockSensors.Enums;
using MockSensors.Sensors.Managers;

public abstract class SensorControllerBase<TManager> : ControllerBase where TManager : ISensorManager
{
    protected readonly ILogger _logger;
    protected readonly TManager _sensorManager;

    protected SensorControllerBase(TManager sensorManager, ILogger logger)
    {
        _sensorManager = sensorManager;
        _logger = logger;
    }

    protected IActionResult StartSensor(string id)
    {
        _logger.LogInformation($"Starting sensor: {id}");
        if (_sensorManager.TryStartSensor(id))
        {
            return Ok($"Sensor {id} started");
        }
        else
        {
            return BadRequest($"Sensor {id} is already running");
        }
    }

    protected IActionResult StopSensor(string id)
    {
        _logger.LogInformation($"Attempting to stop sensor: {id}");
        var result = _sensorManager.TryStopSensor(id);

        return result switch
        {
            SensorStopResult.Stopped => Ok($"Sensor {id} stopped"),
            SensorStopResult.AlreadyStopped => BadRequest($"Sensor {id} is already stopped"),
            SensorStopResult.NotFound => NotFound($"Sensor {id} was not found"),
            _ => throw new InvalidOperationException("Unexpected result when trying to stop sensor")
        };
    }

    protected IActionResult GetActiveSensors()
    {
        _logger.LogInformation("Getting active sensors");
        var activeSensors = _sensorManager.GetActiveSensors();
        return Ok(activeSensors);
    }

    protected IActionResult GetAllSensors()
    {
        _logger.LogInformation("Getting all sensors");
        var sensors = _sensorManager.GetAllSensorsWithStatus();
        return Ok(sensors);
    }

    protected IActionResult StopAllSensors()
    {
        _logger.LogInformation("Stopping all sensors");
        _sensorManager.StopAllSensors();
        return Ok("All sensors stopped");
    }
}
