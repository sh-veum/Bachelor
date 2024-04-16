using Microsoft.AspNetCore.Mvc;
using MockSensors.Sensors.Managers;

namespace MockSensors.Controllers;

[ApiController]
[Route("sensors/waterQuality")]
public class WaterQualitySensorController : SensorControllerBase<WaterQualitySensorManager>
{
    public WaterQualitySensorController(WaterQualitySensorManager sensorManager, ILogger<WaterQualitySensorController> logger) : base(sensorManager, logger) { }

    [HttpPost("startSensor/{id}")]
    public IActionResult StartWaterQualitySensor(string id) => StartSensor(id);

    [HttpPost("stopSensor/{id}")]
    public IActionResult StopWaterQualitySensor(string id) => StopSensor(id);

    [HttpGet("activeSensors")]
    public IActionResult GetActiveWaterQualitySensors() => GetActiveSensors();

    [HttpGet("allSensors")]
    public IActionResult GetAllWaterQualitySensors() => GetAllSensors();

    [HttpPost("stopAll")]
    public IActionResult StopAllWaterQualitySensors() => StopAllSensors();
}