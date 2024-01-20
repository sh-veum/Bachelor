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
  private readonly ApiDbContext _context;

  public SpeciesController(ILogger<SpeciesController> logger, ApiDbContext context)
  {
    _logger = logger;
    _context = context;
  }

  [HttpGet(Name = "GetAllSpecies")]
  public async Task<IActionResult> Get()
  {
    var species = new Species()
    {
      Name = "Liten kantål"
    };

    _context.Add(species);

    await _context.SaveChangesAsync();

    var allSpecies = await _context.Species.ToListAsync();
    return Ok(allSpecies);
  }

}