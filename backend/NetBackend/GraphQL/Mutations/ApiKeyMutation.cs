using Microsoft.AspNetCore.Mvc;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.Keys;

namespace NetBackend.GraphQL.Mutations;

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
             [Service] IGraphQLKeyService graphQLKeyService,
             [Service] IUserService userService,
             string keyName,
             List<GraphQLAccessKeyPermissionDto> permissions)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null.");

        var (user, error) = await userService.GetUserByHttpContextAsync(httpContext);
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
        var encryptedKey = await graphQLKeyService.EncryptAndStoreGraphQLAccessKey(graphQLApiKey);

        await _kafkaProducerService.ProduceAsync(KafkaConstants.GraphQLKeyTopic + "-" + user.Id, $"New key added: {graphQLApiKey.KeyName} Update the database!");

        return new AccessKeyDto
        {
            EncryptedKey = encryptedKey ?? throw new Exception("Failed to generate encrypted key.")
        };
    }

    public async Task<ResponseDto?> ToggleApiKey(
        [Service] IRestKeyService restKeyService,
        [Service] IGraphQLKeyService graphQLKeyService,
        [Service] IKafkaKeyService kafkaKeyService,
        [Service] IUserService userService,
        ToggleApiKeyStatusDto toggleApiKeyStatusDto)
    {
        // NOTE: Workaround to replace [Autorize]
        var (user, error) = await userService.GetUserByHttpContextAsync(_httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(_httpContextAccessor), "HttpContextAccessor's HttpContext is null. User unauthorized or session expired."));

        if (user == null) return null;

        IActionResult result;
        try
        {
            string keyType = toggleApiKeyStatusDto.KeyType.ToLower();
            switch (keyType)
            {
                case "rest":
                    result = await restKeyService.ToggleRestApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                case "graphql":
                    result = await graphQLKeyService.ToggleGraphQLApiKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                case "kafka":
                    result = await kafkaKeyService.ToggleKafkaKey(toggleApiKeyStatusDto.Id, toggleApiKeyStatusDto.IsEnabled);
                    break;
                default:
                    return new ResponseDto { IsSuccess = false, Message = "Invalid key type." };
            }

            var successMessage = keyType == "rest" ? "REST API key status toggled successfully" : "GraphQL API key status toggled successfully";

            return ConvertToResponseDto(result, successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling the API key status.");
            return new ResponseDto
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    private static ResponseDto ConvertToResponseDto(IActionResult actionResult, string sucessMessage)
    {
        if (actionResult is OkResult)
        {
            return new ResponseDto { IsSuccess = true, Message = sucessMessage };
        }
        else if (actionResult is NotFoundObjectResult notFoundResult)
        {
            return new ResponseDto { IsSuccess = false, Message = notFoundResult.Value?.ToString() };
        }
        else
        {
            var badRequestResult = actionResult as BadRequestObjectResult;
            return new ResponseDto { IsSuccess = false, Message = badRequestResult?.Value?.ToString() };
        }
    }

    public async Task<ResponseDto?> DeleteGraphQLApiKey(
        [Service] IGraphQLKeyService graphQLKeyService,
        [Service] IUserService userService,
        Guid id)
    {
        // NOTE: Workaround to replace [Autorize]
        var (user, error) = await userService.GetUserByHttpContextAsync(_httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(_httpContextAccessor), "HttpContextAccessor's HttpContext is null. User unauthorized or session expired."));

        if (user == null) return null;

        IActionResult result;
        try
        {
            result = await graphQLKeyService.DeleteGraphQLApiKeyById(id, "graphqlapikey");

            var successMessage = "GraphQL API key deleted successfully";

            return ConvertToResponseDto(result, successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting the API key.");
            return new ResponseDto
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ResponseDto?> DeleteGraphQLApiKeyByEncryptedKey(
        [Service] IGraphQLKeyService graphQLKeyService,
        [Service] IUserService userService,
        string encryptedKey)
    {
        // NOTE: Workaround to replace [Autorize]
        var (user, error) = await userService.GetUserByHttpContextAsync(_httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(_httpContextAccessor), "HttpContextAccessor's HttpContext is null. User unauthorized or session expired."));

        if (user == null) return null;

        IActionResult result;
        try
        {
            result = await graphQLKeyService.RemoveGraphQLAccessKey(encryptedKey);

            var successMessage = "GraphQL API key deleted successfully";

            return ConvertToResponseDto(result, successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting the API key.");
            return new ResponseDto
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }
}