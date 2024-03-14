using Microsoft.AspNetCore.Mvc;
using MockSensors.Dto;
using MockSensors.Sensors;

namespace MockSensors.Controllers;

[ApiController]
[Route("sensors/waterQuality")]
public class WaterQualitySensorController : ControllerBase
{
    private readonly ILogger<WaterQualitySensorController> _logger;
    private readonly WaterQualitySensorManager _sensorManager;

    public WaterQualitySensorController(WaterQualitySensorManager sensorManager, ILogger<WaterQualitySensorController> logger)
    {
        _sensorManager = sensorManager;
        _logger = logger;
    }

    [HttpPost("startSensor/{id}")]
    public IActionResult StartSensor(string id)
    {
        _logger.LogInformation("Starting sensor: " + id);
        if (_sensorManager.TryStartSensor(id))
        {
            return Ok($"Sensor {id} started");
        }
        else
        {
            return BadRequest($"Sensor {id} is already running");
        }
    }

    [HttpPost("stopSensor/{id}")]
    public IActionResult StopSensor(string id)
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

    [HttpGet("activeSensors")]
    [ProducesResponseType(typeof(List<SensorDto>), StatusCodes.Status200OK)]
    public IActionResult GetActiveSensors()
    {
        _logger.LogInformation("Getting active sensors");
        var activeSensors = _sensorManager.GetActiveSensors();
        return Ok(activeSensors);
    }

    [HttpGet("allSensors")]
    [ProducesResponseType(typeof(List<SensorDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllSensors()
    {
        _logger.LogInformation("Getting all sensors");
        var sensors = _sensorManager.GetAllSensorsWithStatus();
        return Ok(sensors);
    }

    [HttpPost("stopAll")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public IActionResult StopAllSensors()
    {
        _logger.LogInformation("Stopping all sensors");
        _sensorManager.StopAllSensors();
        return Ok("All sensors stopped");
    }
}

