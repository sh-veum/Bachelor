using Microsoft.AspNetCore.Mvc;
using MockSensors.Dto;
using MockSensors.Sensors;

namespace MockSensors.Controllers;

[ApiController]
[Route("sensors/boat")]
public class BoatController : ControllerBase
{
    private readonly ILogger<BoatController> _logger;
    private readonly BoatSensorManager _boatSensorManager;

    public BoatController(BoatSensorManager boatSensorManager, ILogger<BoatController> logger)
    {
        _boatSensorManager = boatSensorManager;
        _logger = logger;
    }

    [HttpPost("startSensor/{id}")]
    public IActionResult StartSensor(string id)
    {
        _logger.LogInformation($"Starting boat sensor: {id}");
        if (_boatSensorManager.TryStartSensor(id))
        {
            return Ok($"Boat sensor {id} started");
        }
        else
        {
            return BadRequest($"Boat sensor {id} is already running");
        }
    }

    [HttpPost("stopSensor/{id}")]
    public IActionResult StopSensor(string id)
    {
        _logger.LogInformation($"Attempting to stop boat sensor: {id}");
        var result = _boatSensorManager.TryStopSensor(id);

        return result switch
        {
            SensorStopResult.Stopped => Ok($"Boat sensor {id} stopped"),
            SensorStopResult.AlreadyStopped => BadRequest($"Boat sensor {id} is already stopped"),
            SensorStopResult.NotFound => NotFound($"Boat sensor {id} was not found"),
            _ => throw new InvalidOperationException("Unexpected result when trying to stop boat sensor")
        };
    }

    [HttpGet("activeSensors")]
    [ProducesResponseType(typeof(List<SensorDto>), StatusCodes.Status200OK)]
    public IActionResult GetActiveSensors()
    {
        _logger.LogInformation("Getting active boat sensors");
        var activeSensors = _boatSensorManager.GetActiveSensors();
        return Ok(activeSensors);
    }

    [HttpGet("allSensors")]
    [ProducesResponseType(typeof(List<SensorDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllSensors()
    {
        _logger.LogInformation("Getting all boat sensors");
        var sensors = _boatSensorManager.GetAllSensorsWithStatus();
        return Ok(sensors);
    }

    [HttpPost("stopAll")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public IActionResult StopAllSensors()
    {
        _logger.LogInformation("Stopping all boat sensors");
        _boatSensorManager.StopAllSensors();
        return Ok("All boat sensors stopped");
    }
}
