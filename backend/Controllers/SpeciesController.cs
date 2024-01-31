using backend.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models;

namespace NetBackeng.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly ILogger<SpeciesController> _logger;
    private readonly MainDbContext _mainDbContext;
    private readonly CustomerOneDbContext _customerOneContext;
    private readonly CustomerTwoDbContext _customerTwoContext;


    public SpeciesController(ILogger<SpeciesController> logger, MainDbContext mainDbContext, CustomerOneDbContext customerOneContext, CustomerTwoDbContext customerTwoContext)
    {
        _logger = logger;
        _mainDbContext = mainDbContext;
        _customerOneContext = customerOneContext;
        _customerTwoContext = customerTwoContext;
    }

    [HttpGet(Name = "GetAllSpecies")]
    public async Task<IActionResult> GetAllSpecies([FromQuery] DatabaseType database)
    {
        try
        {
            DbContext selectedContext = database switch
            {
                DatabaseType.Main => _mainDbContext,
                DatabaseType.Customer1 => _customerOneContext,
                DatabaseType.Customer2 => _customerTwoContext,
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