using backend.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Name.Models.Dto;
using Netbackend.Services;
using NetBackend.Models.ApiKey.Dto;
using NetBackend.Models.Dto;
using NetBackend.Models.User;
using NetBackend.Services;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IKeyService _keyService;
    private readonly IDatabaseContextService _databaseContextService;

    public UserController(ILogger<UserController> logger, UserManager<User> userManager, IKeyService keyService, IDatabaseContextService databaseContextService)
    {
        _logger = logger;
        _userManager = userManager;
        _keyService = keyService;
        _databaseContextService = databaseContextService;
    }

    [HttpGet("userinfo")]
    public async Task<ActionResult> GetUserInfo()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [HttpPost("update-database-name")]
    public async Task<IActionResult> UpdateUserDatabaseName([FromBody] UpdateUserDatabaseNameDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email ?? string.Empty);
        if (user == null)
        {
            return NotFound($"User with mail {model.Email} not found.");
        }

        user.DatabaseName = model.Database switch
        {
            DatabaseType.Main => "Main",
            DatabaseType.Customer1 => "Customer1",
            DatabaseType.Customer2 => "Customer2",
            _ => user.DatabaseName // In case of an invalid enum value, do not change the database name
        };

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest("Failed to update user database name.");
        }

        return Ok($"User's database name updated to {user.DatabaseName}.");
    }

    [HttpPost("create-apikey")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyDto model)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }

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

        var apiKeyDto = new ApiKeyDto
        {
            Id = apiKey.Id,
            KeyName = apiKey.KeyName,
            AccessibleEndpoints = apiKey.AccessibleEndpoints
        };

        return Ok(apiKeyDto);
    }
}