using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Constants;
using NetBackend.Models.Dto;
using NetBackend.Tools;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.DatabaseControllerRoute)]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ILogger<DatabaseController> logger)
    {
        _logger = logger;
    }

    [HttpGet("get-database-names")]
    [ProducesResponseType(typeof(DatabaseNameDto), StatusCodes.Status200OK)]
    public ActionResult GetDatabaseNames()
    {
        _logger.LogInformation("Getting database names...");

        var databaseNames = typeof(DatabaseConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name != nameof(DatabaseConstants.MainDbName))
            .Select(fi => fi.GetRawConstantValue()?.ToString())
            .ToList();

        return Ok(databaseNames);
    }

    [HttpGet("get-default-endpoints")]
    [ProducesResponseType(typeof(RestApiEndpointSchema), StatusCodes.Status200OK)]
    public ActionResult GetDefaultApiEndpoints()
    {
        _logger.LogInformation("Getting default rest api endpoints...");

        var endpointsInfo = ApiConstants.DefaultApiEndpoints
            .Select(endpoint => new
            {
                endpoint.Path,
                endpoint.Method,
                ExpectedBody = endpoint.ExpectedBodyType != null ? DtoTools.GetDtoStructure(endpoint.ExpectedBodyType) : null
            })
            .ToList();

        return Ok(endpointsInfo);
    }
}