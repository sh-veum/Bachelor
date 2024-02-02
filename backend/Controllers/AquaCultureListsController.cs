using backend.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Models.User;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AquaCultureListsController : ControllerBase
{
    private readonly ILogger<AquaCultureListsController> _logger;
    private readonly MainDbContext _mainDbContext;
    private readonly CustomerOneDbContext _customerOneContext;
    private readonly CustomerTwoDbContext _customerTwoContext;
    private readonly UserManager<User> _userManager;


    public AquaCultureListsController(ILogger<AquaCultureListsController> logger, MainDbContext mainDbContext, CustomerOneDbContext customerOneContext, CustomerTwoDbContext customerTwoContext, UserManager<User> userManager)
    {
        _logger = logger;
        _mainDbContext = mainDbContext;
        _customerOneContext = customerOneContext;
        _customerTwoContext = customerTwoContext;
        _userManager = userManager;
    }

    // GET: /fishhealth/licenseelist
    [HttpGet("fishhealth/licenseelist")]
    public ActionResult GetLicenseeList()
    {
        return Content("Not yet implemented");
    }

    // GET: /fishhealth/species
    [HttpGet("fishhealth/species")]
    // public async Task<IActionResult> GetAllSpecies([FromQuery] DatabaseType database)
    // {
    //     try
    //     {
    //         DbContext selectedContext = database switch
    //         {
    //             DatabaseType.Main => _mainDbContext,
    //             DatabaseType.Customer1 => _customerOneContext,
    //             DatabaseType.Customer2 => _customerTwoContext,
    //             _ => throw new ArgumentException("Invalid database selection")
    //         };

    //         var allSpecies = await selectedContext.Set<Species>().ToListAsync();
    //         return Ok(allSpecies);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while fetching species.");
    //         return BadRequest(ex.Message);
    //     }
    // }
    public async Task<IActionResult> GetAllSpecies([FromQuery] DatabaseType database)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            DbContext selectedContext = user.DatabaseName switch
            {
                "Main" => _mainDbContext,
                "Customer1" => _customerOneContext,
                "Customer2" => _customerTwoContext,
                _ => throw new ArgumentException("Invalid database selection")
            };

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