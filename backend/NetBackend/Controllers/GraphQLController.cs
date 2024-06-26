using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Controllers;

[ApiController]
[Route(ControllerConstants.GraphQLControllerRoute)]
public class GraphQLController : ControllerBase
{
    private readonly ILogger<GraphQLController> _logger;
    private readonly IGraphQLKeyService _graphQlKeyService;
    private readonly IUserService _userService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public GraphQLController(ILogger<GraphQLController> logger, IGraphQLKeyService graphQLKeyService, IUserService userService, IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _graphQlKeyService = graphQLKeyService;
        _userService = userService;
        _kafkaProducerService = kafkaProducerService;
    }

    [HttpPost("decrypt-accesskey")]
    [Authorize(Roles = RoleConstants.AdminRole)]
    [ProducesResponseType(typeof(GraphQLApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptGraphQlAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var apiKey = await DecryptAndValidateApiKey(accessKeyDto.EncryptedKey, user.Id);

            var expiresInDays = CalculateExpiresIn.CalculateExpiresInDays(apiKey);

            var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(apiKey.Id);

            if (apiKey is GraphQLApiKey api)
            {
                var graphQLApiKeyDto = new GraphQLApiKeyDto
                {
                    Id = api.Id,
                    KeyName = api.KeyName ?? "",
                    CreatedBy = api.User.Email ?? "",
                    ExpiresIn = expiresInDays,
                    GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                    {
                        QueryName = p.QueryName,
                        AllowedFields = p.AllowedFields ?? []
                    }).ToList() ?? [],
                    IsEnabled = api.IsEnabled
                };

                await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + api.UserId, "Decrypted GraphQL Access Key");

                return Ok(graphQLApiKeyDto);
            }

            return NotFound("API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("delete-accesskey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAccessKey([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var result = await _graphQlKeyService.RemoveGraphQLAccessKey(accessKeyDto.EncryptedKey);
            if (result == null)
            {
                return BadRequest("Failed to delete API key.");
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + user.Id, "GraphQL Key Deleted");

            return Ok("API key deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting access key.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("accesskey-graphql-permissions")]
    [ProducesResponseType(typeof(List<GraphQLAccessKeyPermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGraphQLPermissions([FromBody] AccessKeyDto accessKeyDto)
    {
        try
        {
            var (graphQLApiKey, errorResult) = await _graphQlKeyService.DecryptGraphQLAccessKey(accessKeyDto.EncryptedKey);
            if (errorResult != null)
            {
                return errorResult;
            }

            if (graphQLApiKey is GraphQLApiKey api)
            {
                var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(graphQLApiKey.Id);

                var validPermissions = permissions
                    .Where(p => GraphQLConstants.AvailableQueries
                    .Any(aqt => string.Equals(aqt[0], p.QueryName, StringComparison.OrdinalIgnoreCase)))
                    .Select(permission => new GraphQLAccessKeyPermissionDto
                    {
                        QueryName = permission.QueryName,
                        AllowedFields = permission.AllowedFields ?? []
                    }).ToList();

                await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + graphQLApiKey.UserId, "Got GraphQL Permissions");

                return Ok(validPermissions);
            }

            return NotFound("GraphQL API key type mismatch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving GraphQL permissions.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-keys-by-user")]
    [Authorize]
    [ProducesResponseType(typeof(List<GraphQLApiKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeysByUser()
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            var apiKeysDto = new List<GraphQLApiKeyDto>();
            var graphQLApiKeys = await _graphQlKeyService.GetGraphQLApiKeysByUserId(user.Id);

            foreach (var graphQLApiKey in graphQLApiKeys)
            {
                var permissions = await _graphQlKeyService.GetGraphQLAccessKeyPermissions(graphQLApiKey.Id);
                var graphQLApiKeyDto = new GraphQLApiKeyDto
                {
                    Id = graphQLApiKey.Id,
                    KeyName = graphQLApiKey.KeyName,
                    CreatedBy = userResult.user.Email ?? "error fetching user email",
                    ExpiresIn = graphQLApiKey.ExpiresIn,
                    GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                    {
                        QueryName = p.QueryName,
                        AllowedFields = p.AllowedFields ?? []
                    }).ToList(),
                    IsEnabled = graphQLApiKey.IsEnabled
                };
                apiKeysDto.Add(graphQLApiKeyDto);
            }

            await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + user.Id, "Got GraphQL Keys");

            return Ok(apiKeysDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving API keys by user.");
            return BadRequest(ex.Message);
        }
    }


    [HttpPatch("toggle-apikey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleApiKey([FromBody] ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        try
        {
            var userResult = await _userService.GetUserByHttpContextAsync(HttpContext);
            var user = userResult.user;

            await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + user.Id, $"Toggled GraphQL API Key with id {toggleApiKeyStatusDto.Id} to {toggleApiKeyStatusDto.IsEnabled}");

            return await _graphQlKeyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disabling the API key.");
            return BadRequest(ex.Message);
        }
    }

    private async Task<IApiKey> DecryptAndValidateApiKey(string encryptedKey, string userId)
    {
        var (apiKey, errorResult) = await _graphQlKeyService.DecryptGraphQLAccessKeyUserCheck(encryptedKey, userId);
        if (errorResult != null)
        {
            throw new InvalidOperationException("Error validating API key.");
        }

        if (apiKey == null)
        {
            throw new KeyNotFoundException("API key not found.");
        }

        return apiKey;
    }
}