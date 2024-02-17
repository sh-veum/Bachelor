using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys.Dto;
using NetBackend.Models.User;
using NetBackend.Services;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Models.Dto.Keys;
using Microsoft.EntityFrameworkCore;
using NetBackend.Tools;

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
    public async Task<IActionResult> CreateAccessKey([FromBody] CreateAccessKeyDto model)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            // Create Api Key
            if (model.AccessibleEndpoints == null)
            {
                return BadRequest("Endpoints are be null.");
            }

            var apiKey = await _apiService.CreateRESTApiKey(user, model.KeyName, model.AccessibleEndpoints);

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
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto model)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var apiKey = await DecryptAndValidateApiKey(model.EncryptedKey, user.Id);

            var expiresInMinutes = CalculateExpiresInMinutes(apiKey);

            if (apiKey is ApiKey api)
            {
                var apiKeyDto = new ApiKeyDto
                {
                    Id = api.Id,
                    KeyName = api.KeyName ?? "",
                    CreatedBy = api.User.Email ?? "",
                    ExpiresIn = expiresInMinutes,
                    AccessibleEndpoints = api.AccessibleEndpoints
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
    public async Task<IActionResult> DecryptGraphQlAccessKey([FromBody] AccessKeyDto model)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var apiKey = await DecryptAndValidateApiKey(model.EncryptedKey, user.Id);

            var expiresInMinutes = CalculateExpiresInMinutes(apiKey);

            var permissions = await GetAccessKeyPermission(apiKey.Id);

            if (apiKey is GraphQLApiKey api)
            {
                var graphQLApiKeyDto = new GraphQLApiKeyDto
                {
                    Id = api.Id,
                    KeyName = api.KeyName ?? "",
                    CreatedBy = api.User.Email ?? "",
                    ExpiresIn = expiresInMinutes,
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
    public async Task<IActionResult> DeleteAccessKey([FromBody] AccessKeyDto model)
    {
        try
        {
            var userResult = await _userService.GetUserAsync(HttpContext);
            var user = userResult.user;

            var result = await _keyService.RemoveAccessKey(model.EncryptedKey);
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

    [HttpPost("accesskey-endpoints")]
    public async Task<IActionResult> GetEndpointInfo([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (apiKey, errorResult) = await _keyService.DecryptAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            var validEndpoints = new List<object>();

            if (apiKey is ApiKey api)
            {
                // Assuming apiKey.AccessibleEndpoints contains paths
                validEndpoints = ApiConstants.DefaultApiEndpoints
                    .Where(endpoint => api?.AccessibleEndpoints?.Contains(endpoint.Path) ?? false)
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
            }

            return Ok(validEndpoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving endpoint information.");
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

    private async Task<List<AccessKeyPermission>> GetAccessKeyPermission(int graphQLApiKeyId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var accessKeyPermissions = await mainDbContext.Set<AccessKeyPermission>()
            .Where(p => p.GraphQLApiKeyId == graphQLApiKeyId)
            .ToListAsync();

        return accessKeyPermissions;
    }

    private static int CalculateExpiresInMinutes(IApiKey apiKey)
    {
        var currentTime = DateTime.UtcNow;
        var expiresInMinutes = (apiKey.CreatedAt.AddMinutes(apiKey.ExpiresIn) - currentTime).TotalMinutes;
        return expiresInMinutes > 0 ? (int)expiresInMinutes : 0;
    }
}