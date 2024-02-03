using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using Netbackend.Services;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Models.User;
using NetBackend.Services;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AquaCultureListsController : ControllerBase
{
    private readonly ILogger<AquaCultureListsController> _logger;
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IKeyService _keyService;
    private readonly IUserService _userService;

    public AquaCultureListsController(
        ILogger<AquaCultureListsController> logger,
        IDatabaseContextService databaseContextService,
        IKeyService keyService,
        IUserService userService)
    {
        _logger = logger;
        _databaseContextService = databaseContextService;
        _keyService = keyService;
        _userService = userService;
    }

    [HttpPost("fishhealth/licenseelist")]
    public async Task<IActionResult> GetLicenseeList([FromBody] AccessKeyDto? model)
    {
        try
        {
            DbContext? dbContext = null;

            // If there is sent an access key, use it to get the database context
            if (model == null || model?.EncryptedKey == "string" || model?.EncryptedKey == "")
            {
                // If there is no access key, use the bearer key to get the database context
                var (user, error) = await _userService.GetUserAsync(HttpContext);
                if (error != null) return error;

                dbContext = await _databaseContextService.GetUserDatabaseContext(user);
            }
            else
            {
                if (model != null)
                {
                    // Get the database context using the access key
                    (dbContext, var errorResult) = await _keyService.ProcessAccessKey(model.EncryptedKey);
                    if (errorResult != null) return errorResult;

                    if (dbContext is null) return BadRequest("Database context is null.");
                }
            }

            if (dbContext is null) return BadRequest("Database context is null.");

            // Fetch all licensees
            var allLicenses = await dbContext.Set<Organization>()
                .Select(o => new OrganizationNameIdDto { OrgNo = o.OrgNo, Name = o.Name })
                .ToListAsync();
            return Ok(allLicenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching licensee list.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("fishhealth/species")]
    public async Task<IActionResult> GetAllSpecies([FromBody] AccessKeyDto? model)
    {
        try
        {
            DbContext? dbContext = null;

            // If there is sent an access key, use it to get the database context
            if (model == null || model?.EncryptedKey == "string" || model?.EncryptedKey == "")
            {
                // If there is no access key, use the bearer key to get the database context
                var (user, error) = await _userService.GetUserAsync(HttpContext);
                if (error != null) return error;

                dbContext = await _databaseContextService.GetUserDatabaseContext(user);
            }
            else
            {
                if (model != null)
                {
                    // Get the database context using the access key
                    (dbContext, var errorResult) = await _keyService.ProcessAccessKey(model.EncryptedKey);
                    if (errorResult != null) return errorResult;

                    if (dbContext is null) return BadRequest("Database context is null.");
                }
            }

            if (dbContext is null) return BadRequest("Database context is null.");

            // Fetch all species
            var allSpecies = await dbContext.Set<Species>().ToListAsync();
            return Ok(allSpecies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching species.");
            return BadRequest(ex.Message);
        }
    }
}