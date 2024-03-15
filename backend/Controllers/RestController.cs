using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

[ApiController]
[Route(ControllerConstants.RestControllerRoute)]
public class RestController : ControllerBase
{
    private readonly ILogger<RestController> _logger;
    private readonly IRestKeyService _restKeyService;
    private readonly IUserService _userService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public RestController(ILogger<RestController> logger, IRestKeyService restKeyService, IUserService userService, IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _restKeyService = restKeyService;
        _userService = userService;
        _kafkaProducerService = kafkaProducerService;
    }

    [HttpPost("create-accesskey")]
    [Authorize]
    [ProducesResponseType(typeof(AccessKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAccessKey([FromBody] CreateAccessKeyDto createAccessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            // Create Api Key
            if (createAccessKeyDto.ThemeIds == null)
            {
                return BadRequest("Themes are null.");
            }

            var restApiKey = await _restKeyService.CreateRESTApiKey(user, createAccessKeyDto.KeyName, createAccessKeyDto.ThemeIds);

            if (restApiKey == null)
            {
                _logger.LogError("Failed to create REST API key for user: {UserId}", user.Id);
                return BadRequest("Failed to create REST API key.");
            }

            // Encrypt and store access key
            var accesKey = await _restKeyService.EncryptAndStoreRestAccessKey(restApiKey);

            var accesKeyDto = new AccessKeyDto
            {
                EncryptedKey = accesKey ?? ""
            };

            await _kafkaProducerService.ProduceAsync(KafkaConstants.RestKeyTopic, "New Rest Access Key Created");

            return Ok(accesKeyDto);
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

            var result = await _restKeyService.DeleteRestApiKeyById(id);
            if (result == null)
            {
                return BadRequest("Failed to delete API key.");
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.RestKeyTopic, "Rest Access Key Deleted");

            return Ok("API key deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("accesskey-themes")]
    [ProducesResponseType(typeof(List<RestApiThemeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThemeInfo([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (restApiKey, errorResult) = await _restKeyService.DecryptRestAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (restApiKey is RestApiKey api)
            {
                var themes = await _restKeyService.GetRESTApiKeyThemes(restApiKey.Id);

                if (themes.Count == 0)
                {
                    return NotFound("No valid themes found for the provided access key.");
                }

                var validThemes = themes.Select(theme => new
                {
                    Name = theme.ThemeName,
                    Endpoints = ApiConstants.DefaultApiEndpoints
                        .Where(endpoint => theme.AccessibleEndpoints.Contains(endpoint.Path))
                        .Select(endpoint => new
                        {
                            endpoint.Path,
                            endpoint.Method,
                            ExpectedBody = endpoint.ExpectedBodyType != null ? DtoTools.GetDtoStructure(endpoint.ExpectedBodyType) : null
                        }).ToList<object>()
                }).ToList();

                return Ok(validThemes);
            }

            return NotFound("REST API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving theme information.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("decrypt-rest-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(RestApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var restApiKey = await DecryptAndValidateApiKey(accessKeyDto.EncryptedKey, user.Id);

            var expiresInDays = CalculateExpiresInDays(restApiKey);

            var themes = await _restKeyService.GetRESTApiKeyThemes(restApiKey.Id);

            if (restApiKey is RestApiKey api)
            {
                var restApiKeyDto = new RestApiKeyDto
                {
                    Id = api.Id,
                    KeyName = api.KeyName ?? "",
                    CreatedBy = api.User.Email ?? "",
                    ExpiresIn = expiresInDays,
                    Themes = themes.Select(t => new ThemeDto
                    {
                        Id = t.Id,
                        ThemeName = t.ThemeName,
                        IsDeprecated = t.IsDeprecated,
                        AccessibleEndpoints = t.AccessibleEndpoints ?? []
                    }).ToList() ?? [],
                    IsEnabled = api.IsEnabled
                };

                return Ok(restApiKeyDto);
            }

            return NotFound("REST API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-keys-by-user")]
    [Authorize]
    [ProducesResponseType(typeof(List<IApiKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeysByUser()
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var apiKeysDto = new List<RestApiKeyDto>();

            var restApiKeys = await _restKeyService.GetRestApiKeysByUserId(user.Id);

            foreach (var restApiKey in restApiKeys)
            {
                var themes = await _restKeyService.GetRESTApiKeyThemes(restApiKey.Id);
                var restApiKeyDto = new RestApiKeyDto
                {
                    Id = restApiKey.Id,
                    KeyName = restApiKey.KeyName,
                    CreatedBy = userResult.user.Email ?? "error fetching user email",
                    ExpiresIn = restApiKey.ExpiresIn,
                    Themes = themes.Select(t => new ThemeDto
                    {
                        Id = t.Id,
                        ThemeName = t.ThemeName,
                        AccessibleEndpoints = t.AccessibleEndpoints,
                        IsDeprecated = t.IsDeprecated
                    }).ToList(),
                    IsEnabled = restApiKey.IsEnabled
                };
                apiKeysDto.Add(restApiKeyDto);
            }

            return Ok(apiKeysDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving API keys by user.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("toggle-apikey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleApiKey([FromBody] ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        try
        {
            return await _restKeyService.ToggleRestApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disabling the API key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-themes-by-user")]
    [Authorize]
    [ProducesResponseType(typeof(List<ThemeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThemesByUser()
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var themes = await _restKeyService.GetThemesByUserId(user.Id);

            if (themes.Count == 0)
            {
                return NotFound("No themes found for the user.");
            }

            var validThemes = themes.Select(theme => new ThemeDto
            {
                Id = theme.Id,
                ThemeName = theme.ThemeName,
                AccessibleEndpoints = theme.AccessibleEndpoints,
                IsDeprecated = theme.IsDeprecated
            }).ToList();

            return Ok(validThemes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving themes by user.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create-theme")]
    [Authorize]
    [ProducesResponseType(typeof(ThemeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTheme([FromBody] ThemeDto themeDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var theme = new Theme
            {
                ThemeName = themeDto.ThemeName,
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
                IsDeprecated = themeDto.IsDeprecated,
                UserId = user.Id,
                User = user
            };

            var createdTheme = await _restKeyService.CreateTheme(theme);

            if (createdTheme == null)
            {
                return BadRequest("Failed to create theme.");
            }

            var newThemeDto = new ThemeDto
            {
                Id = createdTheme.Id,
                ThemeName = createdTheme.ThemeName,
                AccessibleEndpoints = createdTheme.AccessibleEndpoints,
                IsDeprecated = createdTheme.IsDeprecated
            };

            return Ok(newThemeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating theme.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update-theme")]
    [Authorize]
    [ProducesResponseType(typeof(ThemeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTheme([FromBody] ThemeDto themeDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var theme = new Theme
            {
                Id = themeDto.Id ?? Guid.Empty,
                ThemeName = themeDto.ThemeName,
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
                IsDeprecated = themeDto.IsDeprecated,
                UserId = user.Id,
                User = user
            };

            var updatedTheme = await _restKeyService.UpdateTheme(theme);

            if (updatedTheme == null)
            {
                return BadRequest("Failed to update theme.");
            }

            return Ok(themeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating theme.");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("delete-theme")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTheme([FromQuery] Guid id)
    {
        try
        {
            var result = await _restKeyService.DeleteTheme(id);
            if (result == null)
            {
                return BadRequest("Failed to delete theme.");
            }

            return Ok("Theme deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting theme.");
            return BadRequest(ex.Message);
        }
    }

    private async Task<IApiKey> DecryptAndValidateApiKey(string encryptedKey, string userId)
    {
        var (apiKey, errorResult) = await _restKeyService.DecryptRestAccessKeyUserCheck(encryptedKey, userId);
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

    private static int CalculateExpiresInDays(IApiKey apiKey)
    {
        var currentTime = DateTime.UtcNow;
        var expiresInDays = (apiKey.CreatedAt.AddDays(apiKey.ExpiresIn) - currentTime).TotalDays;
        return expiresInDays > 0 ? (int)expiresInDays : 0;
    }
}