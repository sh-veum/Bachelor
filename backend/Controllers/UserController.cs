using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Enums;
using NetBackend.Models.Dto;
using NetBackend.Models.User;
using NetBackend.Constants;
using Microsoft.EntityFrameworkCore;
using NetBackend.Services.Interfaces;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.UserControllerRoute)]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<UserModel> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(ILogger<UserController> logger, UserManager<UserModel> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("userinfo")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await _userManager.GetRolesAsync(user);

            var userDto = new UserInfoDto
            {
                Email = user.Email,
                UserName = user.UserName,
                AssignedDatabase = user.DatabaseName,
                Role = role.FirstOrDefault()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user info.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("update-database-name")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserDatabaseName([FromBody] UserDatabaseNameDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email ?? string.Empty);
            if (user == null)
            {
                return NotFound($"User with mail {model.Email} not found.");
            }

            user.DatabaseName = model.Database switch
            {
                DatabaseType.Main => DatabaseConstants.MainDbName,
                DatabaseType.Customer1 => DatabaseConstants.CustomerOneDbName,
                DatabaseType.Customer2 => DatabaseConstants.CustomerTwoDbName,
                _ => user.DatabaseName // In case of an invalid enum value, do not change the database name
            };

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to update user database name.");
            }

            return Ok($"User's database name updated to {user.DatabaseName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating database to user.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-all-users")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(List<UserInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();

            var userDtos = new List<UserInfoDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserInfoDto
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    AssignedDatabase = user.DatabaseName,
                    Role = roles.FirstOrDefault()
                };

                userDtos.Add(userDto);
            }

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all users.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("roles")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(List<IdentityRole>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all roles.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("change-role")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeUserRole([FromBody] UserRoleDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound($"User with email {model.Email} not found.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return BadRequest($"Role {model.Role} does not exist.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok($"User's role updated to {model.Role}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing user role.");
            return BadRequest(ex.Message);
        }
    }
}