using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Constants;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.DatabaseControllerRoute)]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public DatabaseController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpGet("get-database-names")]
    public ActionResult GetDatabaseNames()
    {
        var databaseNames = typeof(DatabaseConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name != nameof(DatabaseConstants.MainDbName))
            .Select(fi => fi.GetRawConstantValue()?.ToString())
            .ToList();

        return Ok(databaseNames);
    }
}