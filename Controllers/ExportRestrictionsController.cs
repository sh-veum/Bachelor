using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models.ControlAreas;

namespace NetBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ExportRestrictionsController : ControllerBase
{

    private readonly ILogger<ExportRestrictionsController> _logger;
    private readonly ApiDbContext _context;

    public ExportRestrictionsController(ILogger<ExportRestrictionsController> logger, ApiDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: /fishhealth/exportrestrictions/{year}/{week}
    [HttpGet("fishhealth/exportrestrictions/{year}/{week}")]
    // public async Task<ActionResult<IEnumerable<ExportRestrictionArea>>> GetExportRestrictions(int year, int week)
    // {
    //     return await _context.ExportRestrictionAreas
    //                          .Where(x => x.Year == year && x.Week == week)
    //                          .ToListAsync();
    // }
    public ActionResult GetExportRestrictions()
    {
        return Content("Not yet implemented");
    }

    // GET: /fishhealth/exportrestrictions/{localityNo}/{year}/{week}
    [HttpGet("fishhealth/exportrestrictions/{localityNo}/{year}/{week}")]
    // public async Task<ActionResult<IEnumerable<ExportRestrictionArea>>> GetExportRestrictionInfo(long localityNo, int year, int week)
    // {
    //     return await _context.ExportRestrictionAreas
    //                          .Where(x => x.LocalityNo == localityNo && x.Year == year && x.Week == week)
    //                          .ToListAsync();
    // }
    public ActionResult GetExportRestrictionInfo()
    {
        return Content("Not yet implemented");
    }
}
