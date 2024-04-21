using Microsoft.AspNetCore.Mvc;
using MockSensors.Sensors.Managers;

namespace MockSensors.Controllers;

[ApiController]
[Route("sensors/boat")]
public class BoatLocationSensorController : SensorControllerBase<BoatSensorManager>
{
    public BoatLocationSensorController(BoatSensorManager sensorManager, ILogger<BoatLocationSensorController> logger) : base(sensorManager, logger) { }

    [HttpPost("startSensor/{id}")]
    public IActionResult StartBoatSensor(string id) => StartSensor(id);

    [HttpPost("stopSensor/{id}")]
    public IActionResult StopBoatSensor(string id) => StopSensor(id);

    [HttpGet("activeSensors")]
    public IActionResult GetActiveBoatSensors() => GetActiveSensors();

    [HttpGet("allSensors")]
    public IActionResult GetAllBoatSensors() => GetAllSensors();

    [HttpPost("stopAll")]
    public IActionResult StopAllBoatSensors() => StopAllSensors();
}
