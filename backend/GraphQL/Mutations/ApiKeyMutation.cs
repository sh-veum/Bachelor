using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Types;

namespace NetBackend.GraphQL.Mutations;

public class ApiKeyMutation
{
    private readonly ILogger<ApiKeyMutation> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiKeyMutation(IHttpContextAccessor httpContextAccessor, ILogger<ApiKeyMutation> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
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

        return new AccessKeyDto
        {
            EncryptedKey = encryptedKey ?? throw new Exception("Failed to generate encrypted key.")
        };
    }
}