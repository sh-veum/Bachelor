using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Kafka;
using NetBackend.Types;

namespace NetBackend.GraphQL.Mutations;

public class ApiKeyMutation
{
    private readonly ILogger<ApiKeyMutation> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IKafkaProducerService _kafkaProducerService;

    public ApiKeyMutation(IHttpContextAccessor httpContextAccessor, ILogger<ApiKeyMutation> logger, IKafkaProducerService kafkaProducerService)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task<AccessKeyDto?> CreateGraphQLAccessKey(
            [Service] IKeyService keyService,
            [Service] IApiService apiService,
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

        var graphQLApiKey = await apiService.CreateGraphQLApiKey(user, keyName, accessKeyPermissions);
        _logger.LogInformation("graphQLApiKey: {graphQLApiKey}", graphQLApiKey);

        // Encrypt and store access key
        var encryptedKey = await keyService.EncryptAndStoreAccessKey(graphQLApiKey, user);

        await _kafkaProducerService.ProduceAsync("key-updates", $"New key added: {graphQLApiKey.KeyName} Update the database!");

        return new AccessKeyDto
        {
            EncryptedKey = encryptedKey ?? throw new Exception("Failed to generate encrypted key.")
        };
    }

    public async Task<ToggleApiKeyResponseDto> ToggleApiKey(
        [Service] IKeyService keyService,
        ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        IActionResult result;
        try
        {
            string keyType = toggleApiKeyStatusDto.KeyType.ToUpper();
            switch (keyType)
            {
                case "REST":
                    result = await keyService.ToggleApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                case "GRAPHQL":
                    result = await keyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
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