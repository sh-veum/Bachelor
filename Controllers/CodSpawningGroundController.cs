using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models.Dto;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class CodSpawningGroundController : ControllerBase
{
    private readonly ApiDbContext _context;

    public CodSpawningGroundController(ApiDbContext context)
    {
        _context = context;
    }

    // GET: /codspawningground/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CodSpawningGroundDto>> GetCodSpawningGround(int id)
    {
        var codSpawningGround = await _context.CodSpawningGroundDtos.FindAsync(id);

        if (codSpawningGround == null)
        {
            return NotFound();
        }

        return codSpawningGround;
    }
}
