using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Constants;
using NetBackend.Services.Interfaces;

namespace NetBackend.Controllers.SensorController;

[Route(ControllerConstants.SensorControllerRoute)]
[Authorize]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly ISensorService _sensorService;
    private readonly IUserService _userService;

    public SensorController(ILogger<SensorController> logger, ISensorService sensorService, IUserService userService)
    {
        _logger = logger;
        _sensorService = sensorService;
        _userService = userService;
    }

    [HttpPost("waterQuality/startSensor")]
    public async Task<IActionResult> StartWaterQualitySensor()
    {
        try
        {
            var (user, error) = await _userService.GetUserAsync(HttpContext);
            if (error != null) return error;

            var userId = user.Id;

            _logger.LogInformation("Starting sensor for user {UserId}", userId);

            var (success, message) = await _sensorService.StartWaterQualitySensorAsync(userId.ToString());
            if (success)
            {
                return Ok(message);
            }
            else
            {
                return BadRequest(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting Water Quality Sensor.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("waterQuality/stopSensor")]
    public async Task<IActionResult> StopWaterQualitySensor()
    {
        try
        {
            var (user, error) = await _userService.GetUserAsync(HttpContext);
            if (error != null) return error;

            var userId = user.Id;

            _logger.LogInformation("Stopping sensor for user {UserId}", userId);

            var (success, message) = await _sensorService.StopWaterQualitySensorAsync(userId.ToString());
            if (success)
            {
                return Ok(message);
            }
            else
            {
                return BadRequest(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping Water Quality Sensor.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stopAll")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    public async Task<IActionResult> StopAllSensors()
    {
        try
        {
            var result = await _sensorService.StopAllSensorsAsync();
            if (result)
            {
                return Ok("Sensors stopped successfully.");
            }
            else
            {
                return BadRequest("Failed to stop sensors.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping Water Quality Sensor.");
            return BadRequest(ex.Message);
        }
    }
}