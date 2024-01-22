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
    private readonly ApiDbContext _context;

    public AquaCultureListsController(ApiDbContext context)
    {
        _context = context;
    }

    // GET: /fishhealth/licenseelist
    [HttpGet("fishhealth/licenseelist")]
    public async Task<ActionResult<IEnumerable<OrganizationNameIdDto>>> GetLicenseeList()
    {
        return await _context.OrganizationNameIdDtos.ToListAsync();
    }

    // GET: /fishhealth/species
    [HttpGet("fishhealth/species")]
    public async Task<ActionResult<IEnumerable<Species>>> GetSpecies()
    {
        return await _context.Species.ToListAsync();
    }
}