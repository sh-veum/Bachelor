using backend.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Name.Models.Dto;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.User;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<User> _userManager;


    public UserController(ILogger<UserController> logger, UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
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

    [HttpPost("updateDatabaseName")]
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
            _logger.LogError("Failed to update user database name.");
            return BadRequest("Failed to update user database name.");
        }

        return Ok($"User's database name updated to {user.DatabaseName}.");
    }
}