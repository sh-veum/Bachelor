using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;

namespace NetBackend.Controllers.SensorController;

[Route(ControllerConstants.SensorControllerRoute)]
[Authorize]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly ISensorService _sensorService;
    private readonly IUserService _userService;
    private readonly IDbContextService _dbContextService;

    public SensorController(ILogger<SensorController> logger, ISensorService sensorService, IUserService userService, IDbContextService dbContextService)
    {
        _logger = logger;
        _sensorService = sensorService;
        _userService = userService;
        _dbContextService = dbContextService;
    }

    [HttpPost("{sensorType}/startSensor")]
    public async Task<IActionResult> StartSensor([FromBody] StartSensorRequestDto request, SensorType sensorType)
    {
        var (user, error) = await _userService.GetUserByHttpContextAsync(HttpContext);
        if (error != null) return error;

        _logger.LogInformation("Starting {SensorType} sensor for user {UserId}", sensorType, user.Id);

        var (success, message) = await _sensorService.StartSensorAsync(user.Id.ToString(), sensorType, request.SendHistoricalData);
        return success ? Ok(message) : BadRequest(message);
    }

    [HttpPost("{sensorType}/stopSensor")]
    public async Task<IActionResult> StopSensor(SensorType sensorType)
    {
        var (user, error) = await _userService.GetUserByHttpContextAsync(HttpContext);
        if (error != null) return error;

        _logger.LogInformation("Stopping {SensorType} sensor for user {UserId}", sensorType, user.Id);

        var (success, message) = await _sensorService.StopSensorAsync(user.Id.ToString(), sensorType);
        return success ? Ok(message) : BadRequest(message);
    }

    [HttpPost("{sensorType}/stopAll")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    public async Task<IActionResult> StopAllSensors(SensorType sensorType)
    {
        try
        {
            var result = await _sensorService.StopAllSensorsAsync(sensorType);
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

    [HttpGet("{sensorType}/activeSensors")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    public async Task<IActionResult> GetActiveSensors(SensorType sensorType)
    {
        try
        {
            var (success, message) = await _sensorService.GetActiveSensors(sensorType);
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


    [HttpGet("{sensorType}/logs")]
    public async Task<IActionResult> GetLogs(SensorType sensorType)
    {
        try
        {
            var (user, error) = await _userService.GetUserByHttpContextAsync(HttpContext);
            if (error != null) return error;

            DbContext dbContext = await _dbContextService.GetUserDatabaseContext(user);

            if (sensorType == SensorType.waterQuality)
            {
                var logs = await dbContext.Set<WaterQualityLog>().ToListAsync();
                return Ok(logs);
            }
            else if (sensorType == SensorType.boat)
            {
                var logs = await dbContext.Set<BoatLocationLog>().ToListAsync();
                return Ok(logs);
            }
            else
            {
                return BadRequest("Invalid sensor type.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting logs for {sensorType}.");
            return BadRequest(ex.Message);
        }
    }
}