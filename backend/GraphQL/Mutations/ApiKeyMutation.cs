using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;

namespace NetBackend.GraphQL.Mutations;

public class ApiKeyMutation
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiKeyMutation(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AccessKeyDto?> CreateGraphQLAccessKey(
        [Service] IKeyService keyService,
        [Service] IApiService apiService,
        [Service] IUserService userService,
        string keyName,
        List<string> allowedQueries)
    {
        // Get user to store access key
        var httpContext = _httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null.");

        var (user, error) = await userService.GetUserAsync(httpContext);
        if (user == null || error != null)
        {
            throw new Exception("User not found.");
        }

        var graphQLApiKey = await apiService.CreateGraphQLApiKey(user, keyName, allowedQueries);

        // Encrypt and store access key
        var accessKey = await keyService.EncryptAndStoreAccessKey(graphQLApiKey, user);

        var accesKeyDto = new AccessKeyDto
        {
            EncryptedKey = accessKey ?? ""
        };

        return accesKeyDto;
    }
}