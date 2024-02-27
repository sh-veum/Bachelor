using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys.Dto;
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
                return BadRequest("Endpoints are be null.");
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

            var themes = await _keyService.GetAccessKeyThemes(apiKey.Id);

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
                    }).ToList() ?? []
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

            var permissions = await _keyService.GetAccessKeyPermissions(apiKey.Id);

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
                    }).ToList() ?? []
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
                var themes = await _keyService.GetAccessKeyThemes(apiKey.Id);
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
                var themes = await _keyService.GetAccessKeyThemes(apiKey.Id);

                // Check if there are no themes
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
}