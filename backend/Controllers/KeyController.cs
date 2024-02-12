using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Enums;
using NetBackend.Models.Dto;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys.Dto;
using NetBackend.Models.User;
using NetBackend.Services;
using NetBackend.Constants;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.KeyControllerRoute)]
public class KeyController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IKeyService _keyService;
    private readonly IApiService _apiService;

    public KeyController(ILogger<UserController> logger, UserManager<User> userManager, IKeyService keyService, IApiService apiService)
    {
        _logger = logger;
        _userManager = userManager;
        _keyService = keyService;
        _apiService = apiService;
    }

    [HttpPost("create-accesskey")]
    [Authorize]
    public async Task<IActionResult> CreateAccessKey([FromBody] CreateAccessKeyDto model)
    {
        try
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Create Api Key
            if (model.AccessibleEndpoints == null)
            {
                return BadRequest("Endpoints are be null.");
            }

            var apiKey = await _keyService.CreateApiKey(user, model.KeyName, model.AccessibleEndpoints);

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

    [HttpPost("decrypt-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    public async Task<IActionResult> DecryptAccessKey([FromBody] AccessKeyDto model)
    {
        try
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Unauthorized();
            }

            var (apiKey, errorResult) = await _keyService.DecryptAccessKeyUserCheck(model.EncryptedKey, user.Id);
            if (errorResult != null)
            {
                return errorResult;
            }

            var apiKeyDto = new ApiKeyDto
            {
                Id = apiKey?.Id ?? 0,
                KeyName = apiKey?.KeyName ?? "",
                CreatedBy = apiKey?.User.Email ?? "",
                AccessibleEndpoints = apiKey?.AccessibleEndpoints
            };

            return Ok(apiKeyDto);
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
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Unauthorized();
            }

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

            // Assuming apiKey.AccessibleEndpoints contains paths
            var validEndpoints = ApiConstants.DefaultApiEndpoints
                .Where(endpoint => apiKey?.AccessibleEndpoints?.Contains(endpoint.Path) ?? false)
                .Select(endpoint => new
                {
                    endpoint.Path,
                    endpoint.Method,
                    ExpectedBody = endpoint.ExpectedBodyType != null ? _apiService.GetDtoStructure(endpoint.ExpectedBodyType) : null
                })
                .ToList();

            if (!validEndpoints.Any())
            {
                return NotFound("No valid endpoints found for the provided access key.");
            }

            return Ok(validEndpoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving endpoint information.");
            return BadRequest(ex.Message);
        }
    }
}