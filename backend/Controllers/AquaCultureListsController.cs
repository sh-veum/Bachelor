using backend.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Services;
using NetBackend.Models;
using NetBackend.Models.User;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AquaCultureListsController : ControllerBase
{
    private readonly ILogger<AquaCultureListsController> _logger;
    private readonly IUserService _userService;
    private readonly IDatabaseContextService _databaseContextService;

    public AquaCultureListsController(ILogger<AquaCultureListsController> logger, IUserService userService, IDatabaseContextService databaseContextService)
    {
        _logger = logger;
        _userService = userService;
        _databaseContextService = databaseContextService;
    }

    [HttpGet("fishhealth/licenseelist")]
    public ActionResult GetLicenseeList()
    {
        return Content("Not yet implemented");
    }

    [HttpGet("fishhealth/species")]
    public async Task<IActionResult> GetAllSpecies()
    {
        var (user, error) = await _userService.GetUserAsync(HttpContext);
        if (error != null) return error;

        try
        {
            var selectedContext = await _databaseContextService.GetUserDatabaseContext(user);
            var allSpecies = await selectedContext.Set<Species>().ToListAsync();
            return Ok(allSpecies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching species.");
            return BadRequest(ex.Message);
        }
    }
}