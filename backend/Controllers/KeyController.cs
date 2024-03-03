using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.User;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Models.Dto.Keys;
using NetBackend.Tools;
using NetBackend.Models.Dto;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.KeyControllerRoute)]
public class KeyController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<UserModel> _userManager;
    private readonly IKeyService _keyService;
    private readonly IApiService _apiService;
    private readonly IDbContextService _dbContextService;
    private readonly IUserService _userService;

    public KeyController(ILogger<UserController> logger, UserManager<UserModel> userManager, IKeyService keyService, IApiService apiService, IDbContextService dbContextService, IUserService userService)
    {
        _logger = logger;
        _userManager = userManager;
        _keyService = keyService;
        _apiService = apiService;
        _dbContextService = dbContextService;
        _userService = userService;
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

            var apiKey = await _apiService.CreateRESTApiKey(user, createAccessKeyDto.KeyName, createAccessKeyDto.Themes);

            if (apiKey == null)
            {
                _logger.LogError("Failed to create API key for user: {UserId}", user.Id);
                return BadRequest("Failed to create API key.");
            }

            // Encrypt and store access key
            var accesKey = await _keyService.EncryptAndStoreAccessKey(apiKey, user);

            var accesKeyDto = new AccessKeyDto
            {
                EncryptedKey = accesKey ?? ""
            };

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
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var apiKey = await DecryptAndValidateApiKey(accessKeyDto.EncryptedKey, user.Id);

            var expiresInDays = CalculateExpiresInDays(apiKey);

            var themes = await _keyService.GetApiKeyThemes(apiKey.Id);

            if (apiKey is ApiKey api)
            {
                var apiKeyDto = new ApiKeyDto
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

                return Ok(apiKeyDto);
            }

            return NotFound("API key type mismatch.");
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

            var permissions = await _keyService.GetGraphQLAccessKeyPermissions(apiKey.Id);

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

            var result = await _keyService.RemoveAccessKey(accessKeyDto.EncryptedKey);
            if (result == null)
            {
                return BadRequest("Failed to delete API key.");
            }

            return Ok("API key deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting access key.");
            return BadRequest(ex.Message);
        }
    }

    // TODO: Remove since themes are used instead
    [HttpPost("accesskey-rest-endpoints")]
    [ProducesResponseType(typeof(List<ApiEndpointSchema>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEndpointInfo([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (apiKey, errorResult) = await _keyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (apiKey is ApiKey api)
            {
                var themes = await _keyService.GetApiKeyThemes(apiKey.Id);
                var allAccessibleEndpoints = themes.SelectMany(t => t.AccessibleEndpoints).ToList();

                var validEndpoints = ApiConstants.DefaultApiEndpoints
                    .Where(endpoint => allAccessibleEndpoints.Contains(endpoint.Path))
                    .Select(endpoint => new
                    {
                        endpoint.Path,
                        endpoint.Method,
                        ExpectedBody = endpoint.ExpectedBodyType != null ? DtoTools.GetDtoStructure(endpoint.ExpectedBodyType) : null
                    })
                    .ToList<object>();

                if (validEndpoints.Count == 0)
                {
                    return NotFound("No valid endpoints found for the provided access key.");
                }

                return Ok(validEndpoints);
            }

            return NotFound("API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving endpoint information.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("accesskey-themes")]
    [ProducesResponseType(typeof(List<ApiThemeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThemeInfo([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (apiKey, errorResult) = await _keyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (apiKey is ApiKey api)
            {
                var themes = await _keyService.GetApiKeyThemes(apiKey.Id);

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

            return NotFound("API key type mismatch.");
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
            var (apiKey, errorResult) = await _keyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (apiKey is GraphQLApiKey api)
            {
                var permissions = await _keyService.GetGraphQLAccessKeyPermissions(apiKey.Id);

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

            return NotFound("API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving GraphQL permissions.");
            return BadRequest(ex.Message);
        }
    }

    private async Task<IApiKey> DecryptAndValidateApiKey(string encryptedKey, string userId)
    {
        var (apiKey, errorResult) = await _keyService.DecryptAccessKeyUserCheck(encryptedKey, userId);
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
                var apiKeysDto = new List<ApiKeyDto>();

                var apiKeys = await _keyService.GetRestApiKeysByUserId(user.Id);

                foreach (var apiKey in apiKeys)
                {
                    var themes = await _keyService.GetApiKeyThemes(apiKey.Id);
                    var apiKeyDto = new ApiKeyDto
                    {
                        Id = apiKey.Id,
                        KeyName = apiKey.KeyName,
                        CreatedBy = userResult.user.Email ?? "error fetching user email",
                        ExpiresIn = apiKey.ExpiresIn,
                        Themes = themes.Select(t => new ThemeDto
                        {
                            Id = t.Id,
                            ThemeName = t.ThemeName,
                            AccessibleEndpoints = t.AccessibleEndpoints
                        }).ToList(),
                        IsEnabled = apiKey.IsEnabled
                    };
                    apiKeysDto.Add(apiKeyDto);
                }

                return Ok(apiKeysDto);
            }
            else if (type == "graphql")
            {
                var apiKeysDto = new List<GraphQLApiKeyDto>();
                var apiKeys = await _keyService.GetGraphQLApiKeysByUserId(user.Id);

                foreach (var apiKey in apiKeys)
                {
                    var permissions = await _keyService.GetGraphQLAccessKeyPermissions(apiKey.Id);
                    var graphQLApiKeyDto = new GraphQLApiKeyDto
                    {
                        Id = apiKey.Id,
                        KeyName = apiKey.KeyName,
                        CreatedBy = userResult.user.Email ?? "error fetching user email",
                        ExpiresIn = apiKey.ExpiresIn,
                        GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                        {
                            QueryName = p.QueryName,
                            AllowedFields = p.AllowedFields ?? []
                        }).ToList(),
                        IsEnabled = apiKey.IsEnabled
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

            var themes = await _keyService.GetThemesByUserId(user.Id);

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

            var createdTheme = await _keyService.CreateTheme(theme);

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

            var updatedTheme = await _keyService.UpdateTheme(theme);

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
            var result = await _keyService.DeleteTheme(id);
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
                return await _keyService.ToggleApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
            }
            else if (keyType == "GRAPHQL")
            {
                return await _keyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
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