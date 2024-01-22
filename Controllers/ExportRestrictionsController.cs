using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ExportRestrictionsController : ControllerBase
{
    private readonly ApiDbContext _context;

    public ExportRestrictionsController(ApiDbContext context)
    {
        _context = context;
    }

    // GET: /fishhealth/exportrestrictions/{year}/{week}
    [HttpGet("fishhealth/exportrestrictions/{year}/{week}")]
    public async Task<ActionResult<IEnumerable<ExportRestrictionArea>>> GetExportRestrictions(int year, int week)
    {
        return await _context.ExportRestrictionAreas
                             .Where(x => x.Year == year && x.Week == week)
                             .ToListAsync();
    }

    // GET: /fishhealth/exportrestrictions/{localityNo}/{year}/{week}
    [HttpGet("fishhealth/exportrestrictions/{localityNo}/{year}/{week}")]
    public async Task<ActionResult<IEnumerable<ExportRestrictionArea>>> GetExportRestrictionInfo(long localityNo, int year, int week)
    {
        return await _context.ExportRestrictionAreas
                             .Where(x => x.LocalityNo == localityNo && x.Year == year && x.Week == week)
                             .ToListAsync();
    }
}
