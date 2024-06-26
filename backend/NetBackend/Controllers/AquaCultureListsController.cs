using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.Keys;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.AquaCultureListControllerRoute)]
public class AquaCultureListsController : ControllerBase
{
    private readonly ILogger<AquaCultureListsController> _logger;
    private readonly IDbContextService _databaseContextService;
    private readonly IRestKeyService _restKeyService;
    private readonly IUserService _userService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public AquaCultureListsController(
        ILogger<AquaCultureListsController> logger,
        IDbContextService databaseContextService,
        IRestKeyService restKeyService,
        IUserService userService,
        IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _databaseContextService = databaseContextService;
        _restKeyService = restKeyService;
        _userService = userService;
        _kafkaProducerService = kafkaProducerService;
    }

    [HttpPost("fishhealth/licenseelist")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLicenseeList([FromBody] AccessKeyDto? accessKey)
    {
        try
        {
            var (dbContext, errorResult, userId) = await _restKeyService.ResolveDbContextAndUserId(accessKey, HttpContext);
            if (errorResult != null)
            {
                return errorResult;
            }
            else if (dbContext is null)
            {
                return BadRequest("Database context is null.");
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.OrgTopic + "-" + userId, "Got Organizations");

            // Fetch all licensees
            var allLicenses = await dbContext.Set<Organization>()
                .Select(o => new OrganizationDto { OrgNo = o.OrgNo, Name = o.Name })
                .ToListAsync();
            return Ok(allLicenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching licensee list.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("fishhealth/species")]
    [ProducesResponseType(typeof(SpeciesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSpecies([FromBody] AccessKeyDto? accessKey)
    {
        try
        {
            var (dbContext, errorResult, userId) = await _restKeyService.ResolveDbContextAndUserId(accessKey, HttpContext);
            if (errorResult != null)
            {
                return errorResult;
            }
            else if (dbContext is null)
            {
                return BadRequest("Database context is null.");
            }

            // Fetch all species
            var allSpecies = await dbContext.Set<Species>()
                .Select(s => new SpeciesDto
                {
                    Name = s.Name!,
                    SuperSecretNumber = s.SuperSecretNumber
                })
                .ToListAsync();

            await _kafkaProducerService.ProduceAsync(KafkaConstants.SpeciesTopic + "-" + userId, "Got Species");

            return Ok(allSpecies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching species.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("fishhealth/species/add")]
    [Authorize]
    [ProducesResponseType(typeof(SpeciesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddSpecies([FromQuery] string speciesName)
    {
        try
        {
            DbContext? dbContext = null;

            var (user, error) = await _userService.GetUserByHttpContextAsync(HttpContext);
            if (error != null) return error;

            dbContext = await _databaseContextService.GetUserDatabaseContext(user);

            if (dbContext is null) return BadRequest("Database context is null.");

            var newSpecies = new Species
            {
                Name = speciesName,
                SuperSecretNumber = 42
            };

            dbContext.Set<Species>().Add(newSpecies);
            await dbContext.SaveChangesAsync();

            await _kafkaProducerService.ProduceAsync(KafkaConstants.SpeciesTopic + "-" + user.Id, $"New species added: {speciesName}. Update the database!");

            var newSpeciesDto = new SpeciesDto
            {
                Name = newSpecies.Name!,
                SuperSecretNumber = newSpecies.SuperSecretNumber
            };

            // Return the newly added species DTO
            return Ok(newSpeciesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding species.");
            return BadRequest(ex.Message);
        }
    }
}
