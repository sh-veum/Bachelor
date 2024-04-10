using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;

namespace NetBackend.Controllers;

[Route(ControllerConstants.SensorControllerRoute)]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly ISensorService _sensorService;
    private readonly IUserService _userService;
    private readonly IKafkaKeyService _kafkaKeyService;

    public SensorController(ILogger<SensorController> logger, ISensorService sensorService, IUserService userService, IKafkaKeyService kafkaKeyService)
    {
        _logger = logger;
        _sensorService = sensorService;
        _userService = userService;
        _kafkaKeyService = kafkaKeyService;
    }

    [HttpPost("{sensorType}/startSensor")]
    public async Task<IActionResult> StartSensor([FromQuery] StartSensorRequestDto request, SensorType sensorType, [FromBody] AccessKeyDto? accessKey)
    {
        var userIdResult = await ResolveUserId(accessKey?.EncryptedKey);
        if (userIdResult.Error != null)
        {
            return userIdResult.Error;
        }

        string userId = userIdResult.UserId!;

        _logger.LogInformation("Starting {SensorType} sensor for user {UserId}", sensorType, userId);
        _logger.LogInformation("SendHistoricalData: {SendHistoricalData}", request.SendHistoricalData);
        _logger.LogInformation("SessionId: {SessionId}", request.SessionId);

        var (success, message) = await _sensorService.StartSensorAsync(userId, sensorType, request.SendHistoricalData, request.SessionId);
        return success ? Ok(message) : BadRequest(message);
    }

    [HttpPost("{sensorType}/stopSensor")]
    public async Task<IActionResult> StopSensor(SensorType sensorType, [FromBody] AccessKeyDto? accessKey)
    {
        var userIdResult = await ResolveUserId(accessKey?.EncryptedKey);
        if (userIdResult.Error != null)
        {
            return userIdResult.Error;
        }

        string userId = userIdResult.UserId!;

        _logger.LogInformation("Stopping {SensorType} sensor for user {UserId}", sensorType, userId);

        var (success, message) = await _sensorService.StopSensorAsync(userId.ToString(), sensorType);
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

    [HttpPost("{sensorType}/activeSensors")]
    public async Task<IActionResult> GetActiveSensors(SensorType sensorType, [FromBody] AccessKeyDto? accessKey)
    {
        // If access key isn't connected to a user, return error
        var error = await ResolveUserId(accessKey?.EncryptedKey);
        if (error.Error != null)
        {
            return error.Error;
        }

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


    [HttpPost("{sensorType}/logs")]
    public async Task<IActionResult> GetLogs(SensorType sensorType, [FromBody] AccessKeyDto? accessKey)
    {
        var error = await ResolveUserId(accessKey?.EncryptedKey);
        if (error.Error != null)
        {
            return error.Error;
        }

        try
        {
            var (dbContext, errorResult) = await _kafkaKeyService.ResolveDbContextAsync(accessKey, HttpContext);
            if (errorResult != null)
            {
                return errorResult;
            }
            else if (dbContext is null)
            {
                return BadRequest("Database context is null.");
            }

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

    private async Task<(string? UserId, IActionResult? Error)> ResolveUserId(string? encryptedKey)
    {
        try
        {
            _logger.LogInformation("Resolving user ID.");
            _logger.LogInformation("Encrypted key: {EncryptedKey}", encryptedKey);
            if (encryptedKey != null)
            {
                var (validationActionResult, kafkaKey) = await _kafkaKeyService.ValidateKafkaAccessKey(encryptedKey);
                if (validationActionResult != null) return (null, validationActionResult);

                if (kafkaKey?.UserId == null) return (null, BadRequest("User ID is null."));
                return (kafkaKey.UserId, null);
            }
            else
            {
                var (user, error) = await _userService.GetUserByHttpContextAsync(HttpContext);
                if (error != null) return (null, error);
                if (user.Id == null) return (null, BadRequest("User ID is null."));
                return (user.Id, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving user ID.");
            return (null, BadRequest(ex.Message));
        }
    }
}