using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Enums;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.KafkaControllerRoute)]
public class KafkaController : ControllerBase
{
    private readonly ILogger<KafkaController> _logger;
    private readonly IKafkaKeyService _kafkaKeyService;
    private readonly IUserService _userService;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IKafkaConsumerService _kafkaConsumerService;
    private readonly IKafkaService _kafkaService;

    public KafkaController(ILogger<KafkaController> logger, IKafkaKeyService kafkaKeyService, IUserService userService, IKafkaProducerService kafkaProducerService, IKafkaConsumerService kafkaConsumerService, IKafkaService kafkaService)
    {
        _logger = logger;
        _kafkaKeyService = kafkaKeyService;
        _userService = userService;
        _kafkaProducerService = kafkaProducerService;
        _kafkaConsumerService = kafkaConsumerService;
        _kafkaService = kafkaService;
    }

    [HttpPost("create-accesskey")]
    [Authorize]
    [ProducesResponseType(typeof(AccessKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAccessKey([FromBody] CreateKafkaKeyDto createKafkaKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var restApiKey = await _kafkaKeyService.CreateKafkaKey(user, createKafkaKeyDto.KeyName, createKafkaKeyDto.Topics);

            if (restApiKey == null)
            {
                _logger.LogError("Failed to create REST API key for user: {UserId}", user.Id);
                return BadRequest("Failed to create REST API key.");
            }

            // Encrypt and store access key
            var accessKey = await _kafkaKeyService.EncryptAndStoreKafkaAccessKey(restApiKey);

            var accessKeyDto = new AccessKeyDto
            {
                EncryptedKey = accessKey ?? ""
            };

            await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + user.Id, "Created Kafka Key with id: " + restApiKey.Id);

            return Ok(accessKeyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("delete-accesskey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAccessKey([FromQuery] Guid id)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var result = await _kafkaKeyService.DeleteKafkaKeyById(id);
            if (result == null)
            {
                return BadRequest("Failed to delete API key.");
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + user.Id, "Delete Kafka key with id: " + id);

            return Ok("API key deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("decrypt-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(KafkaKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var kafkaKey = await DecryptAndValidateApiKey(accessKeyDto.EncryptedKey, user.Id);

            var expiresInDays = CalculateExpiresIn.CalculateExpiresInDays(kafkaKey);

            if (kafkaKey is KafkaKey key)
            {
                var restApiKeyDto = new KafkaKeyDto
                {
                    Id = key.Id,
                    KeyName = key.KeyName ?? "",
                    CreatedBy = key.User.Email ?? "",
                    ExpiresIn = expiresInDays,
                    IsEnabled = key.IsEnabled,
                    Topics = key.Topics ?? []
                };

                await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + key.UserId, "Decrypted Kafka Access Key with id: " + key.Id);

                return Ok(restApiKeyDto);
            }

            return NotFound("Kafka key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-keys-by-user")]
    [Authorize]
    [ProducesResponseType(typeof(List<KafkaKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeysByUser()
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var kafkaKeysDto = new List<KafkaKeyDto>();

            var kafkaKeys = await _kafkaKeyService.GetKafkaKeysByUserId(user.Id);

            foreach (var kafkaKey in kafkaKeys)
            {
                var kafkaKeyDto = new KafkaKeyDto
                {
                    Id = kafkaKey.Id,
                    KeyName = kafkaKey.KeyName,
                    CreatedBy = userResult.user.Email ?? "error fetching user email",
                    ExpiresIn = kafkaKey.ExpiresIn,
                    IsEnabled = kafkaKey.IsEnabled,
                    Topics = kafkaKey.Topics
                };

                kafkaKeysDto.Add(kafkaKeyDto);
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + user.Id, "Got Kafka Keys");

            return Ok(kafkaKeysDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving Kafka keys by user.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("toggle-key")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleApiKey([FromBody] ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + user.Id, $"Toggled Kafka Key with id: {toggleApiKeyStatusDto.Id} to {toggleApiKeyStatusDto.IsEnabled}");

            return await _kafkaKeyService.ToggleKafkaKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disabling the API key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-available-topics")]
    [Authorize]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableTopics()
    {
        var availableTopics = typeof(KafkaConstants)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetRawConstantValue()?.ToString() ?? string.Empty)
            .ToList();

        return Ok(availableTopics);
    }

    [HttpPost("accesskey-kafka-topics")]
    [ProducesResponseType(typeof(KafkaTopicDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccessKeyKafkaTopics([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (validationActionResult, kafkaKey) = await _kafkaKeyService.ValidateKafkaAccessKey(accessKeyDto.EncryptedKey);
            if (validationActionResult != null) return validationActionResult;

            if (kafkaKey != null)
            {
                var topics = await _kafkaKeyService.GetKafkaKeyTopics(kafkaKey.Id);

                var kafkaTopicDto = new KafkaTopicDto
                {
                    SensorId = kafkaKey.UserId,
                    Topics = topics
                };

                await _kafkaProducerService.ProduceAsync(KafkaConstants.KafkaKeyTopic + "-" + kafkaKey.UserId, "Got Kafka Key Topics from key with id: " + kafkaKey.Id);

                return Ok(kafkaTopicDto);
            }
            else
            {
                return BadRequest("Invalid Kafka key.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving Kafka key topics.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("subscribe-to-topics")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubscribeToTopics([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (validationActionResult, kafkaKey) = await _kafkaKeyService.ValidateKafkaAccessKey(accessKeyDto.EncryptedKey);
            if (validationActionResult != null) return validationActionResult;

            if (kafkaKey == null)
            {
                return BadRequest("Invalid Kafka key.");
            }

            var topics = typeof(KafkaConstants)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetValue(null)?.ToString())
                .Where(topic => topic != KafkaConstants.BoatLogTopic && topic != KafkaConstants.WaterQualityLogTopic);

            foreach (var baseTopic in topics)
            {
                var userSpecificTopic = $"{baseTopic}-{kafkaKey.UserId}";
                _kafkaConsumerService.SubscribeToTopic(userSpecificTopic, SensorType.none);
            }

            return Ok("User topics updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while subscribing to topics.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("historical")]
    public async Task<IActionResult> GetHistoricalData([FromQuery] KafkaHistoricalRequestDto request, [FromBody] AccessKeyDto? accessKey)
    {
        string userId = string.Empty;

        if (accessKey != null)
        {
            var userIdResult = await ResolveUserId(accessKey?.EncryptedKey);
            if (userIdResult.Error != null)
            {
                return userIdResult.Error;
            }
            userId = userIdResult.UserId!;
        }
        else
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;
            userId = user.Id ?? throw new InvalidOperationException("User ID is null.");
        }

        if (request.SessionId == null)
        {
            return BadRequest("Session ID is null.");
        }

        var (success, message) = await _kafkaService.StartHistoricalConsumer(userId, request.SensorType ?? SensorType.none, request.SessionId);

        return success ? Ok(message) : BadRequest(message);
    }

    private async Task<IApiKey> DecryptAndValidateApiKey(string encryptedKey, string userId)
    {
        var (apiKey, errorResult) = await _kafkaKeyService.DecryptKafkaAccessKeyUserCheck(encryptedKey, userId);
        if (errorResult != null)
        {
            throw new InvalidOperationException("Error validating API key.");
        }

        if (apiKey == null)
        {
            throw new KeyNotFoundException("API key not found.");
        }

        return apiKey;
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