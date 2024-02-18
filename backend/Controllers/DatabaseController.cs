using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Constants;
using NetBackend.Models.Dto;
using NetBackend.Services;
using NetBackend.Services.Interfaces;
using NetBackend.Tools;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.DatabaseControllerRoute)]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IApiService _apiService;

    public DatabaseController(ILogger<UserController> logger, IApiService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    [HttpGet("get-database-names")]
    [ProducesResponseType(typeof(DatabaseNameDto), StatusCodes.Status200OK)]
    public ActionResult GetDatabaseNames()
    {
        var databaseNames = typeof(DatabaseConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name != nameof(DatabaseConstants.MainDbName))
            .Select(fi => fi.GetRawConstantValue()?.ToString())
            .ToList();

        return Ok(databaseNames);
    }

    [HttpGet("get-default-endpoints")]
    [ProducesResponseType(typeof(ApiEndpointSchema), StatusCodes.Status200OK)]
    public ActionResult GetDefaultApiEndpoints()
    {
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