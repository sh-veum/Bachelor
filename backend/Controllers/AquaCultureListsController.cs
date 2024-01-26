using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AquaCultureListsController : ControllerBase
{
    private readonly ILogger<AquaCultureListsController> _logger;
    private readonly ApiDbContext _context;

    public AquaCultureListsController(ILogger<AquaCultureListsController> logger, ApiDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: /fishhealth/licenseelist
    [HttpGet("fishhealth/licenseelist")]
    public ActionResult GetLicenseeList()
    {
        return Content("Not yet implemented");
    }

    // GET: /fishhealth/species
    [HttpGet("fishhealth/species")]
    public async Task<ActionResult<IEnumerable<Species>>> GetSpecies()
    {
        return await _context.Species.ToListAsync();
    }
}