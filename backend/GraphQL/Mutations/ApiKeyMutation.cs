using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Services.Keys;
using NetBackend.Types;

namespace NetBackend.GraphQL.Mutations;

// TODO: NOTE: Might have to manually put everything in the constructors instead of private fields
public class ApiKeyMutation
{
    private readonly ILogger<ApiKeyMutation> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IKafkaProducerService _kafkaProducerService;

    public ApiKeyMutation(ILogger<ApiKeyMutation> logger, IHttpContextAccessor httpContextAccessor, IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task<AccessKeyDto> CreateGraphQLAccessKey(
             [Service] IBaseKeyService baseKeyService,
             [Service] IGraphQLKeyService graphQLKeyService,
             [Service] IUserService userService,
             string keyName,
             List<AccessKeyPermissionInput> permissions)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null.");

        var (user, error) = await userService.GetUserAsync(httpContext);
        if (user == null || error != null)
        {
            throw new Exception("User not found.");
        }

        var accessKeyPermissions = permissions.Select(p => new AccessKeyPermission
        {
            QueryName = p.QueryName,
            AllowedFields = p.AllowedFields
        }).ToList();
        _logger.LogInformation("accessKeyPermissions: {accessKeyPermissions}", accessKeyPermissions);

        var graphQLApiKey = await graphQLKeyService.CreateGraphQLApiKey(user, keyName, accessKeyPermissions);
        _logger.LogInformation("graphQLApiKey: {graphQLApiKey}", graphQLApiKey);

        // Encrypt and store access key
        var encryptedKey = await baseKeyService.EncryptAndStoreAccessKey(graphQLApiKey, user);

        await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic, $"New key added: {graphQLApiKey.KeyName} Update the database!");

        return new AccessKeyDto
        {
            EncryptedKey = encryptedKey ?? throw new Exception("Failed to generate encrypted key.")
        };
    }

    public async Task<ToggleApiKeyResponseDto> ToggleApiKey(
        [Service] IRestKeyService restKeyService,
        [Service] IGraphQLKeyService graphQLKeyService,
        ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        IActionResult result;
        try
        {
            string keyType = toggleApiKeyStatusDto.KeyType.ToUpper();
            switch (keyType)
            {
                case "REST":
                    result = await restKeyService.ToggleRestApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                case "GRAPHQL":
                    result = await graphQLKeyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                default:
                    return new ToggleApiKeyResponseDto { IsSuccess = false, Message = "Invalid key type." };
            }

            return ConvertToToggleApiKeyResponseDto(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling the API key status.");
            return new ToggleApiKeyResponseDto
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    private static ToggleApiKeyResponseDto ConvertToToggleApiKeyResponseDto(IActionResult actionResult)
    {
        if (actionResult is OkResult)
        {
            return new ToggleApiKeyResponseDto { IsSuccess = true, Message = "API key status toggled successfully." };
        }
        else if (actionResult is NotFoundObjectResult notFoundResult)
        {
            return new ToggleApiKeyResponseDto { IsSuccess = false, Message = notFoundResult.Value?.ToString() };
        }
        else
        {
            var badRequestResult = actionResult as BadRequestObjectResult;
            return new ToggleApiKeyResponseDto { IsSuccess = false, Message = badRequestResult?.Value?.ToString() };
        }
    }
}