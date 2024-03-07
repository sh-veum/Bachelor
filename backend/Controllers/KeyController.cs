using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Models.Dto.Keys;
using NetBackend.Tools;
using NetBackend.Models.Dto;
using NetBackend.Services.Keys;
using NetBackend.Services.Interfaces.Keys;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.KeyControllerRoute)]
public class KeyController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IRestKeyService _restKeyService;
    private readonly IGraphQLKeyService _graphQlKeyService;
    private readonly IBaseKeyService _baseKeyService;
    private readonly IUserService _userService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public KeyController(ILogger<UserController> logger, IRestKeyService restKeyService, IGraphQLKeyService graphQLKeyService, IBaseKeyService baseKeyService, IUserService userService, IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _restKeyService = restKeyService;
        _graphQlKeyService = graphQLKeyService;
        _baseKeyService = baseKeyService;
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
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            // Create Api Key
            if (createAccessKeyDto.Themes == null)
            {
                return BadRequest("Endpoints are null.");
            }

            var restApiKey = await _restKeyService.CreateRESTApiKey(user, createAccessKeyDto.KeyName, createAccessKeyDto.Themes);

            if (restApiKey == null)
            {
                _logger.LogError("Failed to create REST API key for user: {UserId}", user.Id);
                return BadRequest("Failed to create REST API key.");
            }

            // Encrypt and store access key
            var accesKey = await _baseKeyService.EncryptAndStoreAccessKey(restApiKey, user);

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

    [HttpPost("decrypt-rest-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(RestApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
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
                        ThemeName = t.ThemeName,
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


    [HttpPost("decrypt-graphql-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(GraphQLApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptGraphQlAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var apiKey = await DecryptAndValidateApiKey(accessKeyDto.EncryptedKey, user.Id);

            var expiresInDays = CalculateExpiresInDays(apiKey);

            var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(apiKey.Id);

            if (apiKey is GraphQLApiKey api)
            {
                var graphQLApiKeyDto = new GraphQLApiKeyDto
                {
                    Id = api.Id,
                    KeyName = api.KeyName ?? "",
                    CreatedBy = api.User.Email ?? "",
                    ExpiresIn = expiresInDays,
                    GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                    {
                        QueryName = p.QueryName,
                        AllowedFields = p.AllowedFields ?? []
                    }).ToList() ?? [],
                    IsEnabled = api.IsEnabled
                };

                return Ok(graphQLApiKeyDto);
            }

            return NotFound("API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting access key.");
            return BadRequest(ex.Message);
        }
    }


    [HttpPost("delete-accesskey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var result = await _baseKeyService.RemoveAccessKey(accessKeyDto.EncryptedKey);
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
            var (restApiKey, errorResult) = await _baseKeyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
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

    [HttpPost("accesskey-graphql-permissions")]
    [ProducesResponseType(typeof(List<GraphQLAccessKeyPermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGraphQLPermissions([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (graphQLApiKey, errorResult) = await _baseKeyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (graphQLApiKey is GraphQLApiKey api)
            {
                var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(graphQLApiKey.Id);

                var validPermissions = permissions
                    .Where(p => GraphQLConstants.AvailableQueries
                    .Any(aqt => string.Equals(aqt[0], p.QueryName, StringComparison.OrdinalIgnoreCase)))
                    .Select(permission => new GraphQLAccessKeyPermissionDto
                    {
                        QueryName = permission.QueryName,
                        AllowedFields = permission.AllowedFields ?? []
                    }).ToList();

                return Ok(validPermissions);
            }

            return NotFound("GraphQL API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving GraphQL permissions.");
            return BadRequest(ex.Message);
        }
    }

    private async Task<IApiKey> DecryptAndValidateApiKey(string encryptedKey, string userId)
    {
        var (apiKey, errorResult) = await _baseKeyService.DecryptAccessKeyUserCheck(encryptedKey, userId);
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

    [HttpGet("get-apikeys-by-user")]
    [Authorize]
    [ProducesResponseType(typeof(List<IApiKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeysByUser([FromQuery] string type)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            if (type == "rest")
            {
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
                            AccessibleEndpoints = t.AccessibleEndpoints
                        }).ToList(),
                        IsEnabled = restApiKey.IsEnabled
                    };
                    apiKeysDto.Add(restApiKeyDto);
                }

                return Ok(apiKeysDto);
            }
            else if (type == "graphql")
            {
                var apiKeysDto = new List<GraphQLApiKeyDto>();
                var graphQLApiKeys = await _graphQlKeyService.GetGraphQLApiKeysByUserId(user.Id);

                foreach (var graphQLApiKey in graphQLApiKeys)
                {
                    var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(graphQLApiKey.Id);
                    var graphQLApiKeyDto = new GraphQLApiKeyDto
                    {
                        Id = graphQLApiKey.Id,
                        KeyName = graphQLApiKey.KeyName,
                        CreatedBy = userResult.user.Email ?? "error fetching user email",
                        ExpiresIn = graphQLApiKey.ExpiresIn,
                        GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                        {
                            QueryName = p.QueryName,
                            AllowedFields = p.AllowedFields ?? []
                        }).ToList(),
                        IsEnabled = graphQLApiKey.IsEnabled
                    };
                    apiKeysDto.Add(graphQLApiKeyDto);
                }

                return Ok(apiKeysDto);
            }
            else
            {
                return BadRequest("Invalid type. Choose rest or graphql.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving API keys by user.");
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
            var userResult = await _userService.GetUserAsync(HttpContext);
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
                AccessibleEndpoints = theme.AccessibleEndpoints
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
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var theme = new Theme
            {
                ThemeName = themeDto.ThemeName,
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
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
                AccessibleEndpoints = createdTheme.AccessibleEndpoints
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
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var theme = new Theme
            {
                Id = themeDto.Id ?? Guid.Empty,
                ThemeName = themeDto.ThemeName,
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
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

    [HttpPatch("toggle-apikey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleApiKey([FromBody] ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        try
        {
            string keyType = toggleApiKeyStatusDto.KeyType.ToUpper();

            if (keyType == "REST")
            {
                return await _restKeyService.ToggleRestApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
            }
            else if (keyType == "GRAPHQL")
            {
                return await _graphQlKeyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
            }
            else
            {
                return BadRequest("Invalid key type specified. Use 'REST' or 'GraphQL'.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disabling the API key.");
            return BadRequest(ex.Message);
        }
    }
}