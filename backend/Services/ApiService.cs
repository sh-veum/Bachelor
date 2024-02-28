using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;


namespace NetBackend.Services;

public class ApiService : IApiService
{
    private readonly ILogger<ApiService> _logger;
    private readonly IDbContextService _databaseContextService;

    public ApiService(
        ILogger<ApiService> logger,
        IDbContextService databaseContextService)
    {
        _logger = logger;
        _databaseContextService = databaseContextService;
    }

    public async Task<ApiKey> CreateRESTApiKey(UserModel user, string keyName, List<ThemeDto> themeDtos)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        // Create a new ApiKey instance without directly setting AccessibleEndpoints
        var apiKey = new ApiKey
        {
            UserId = user.Id,
            KeyName = keyName,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn,
            Themes = new List<Theme>()
        };

        // Process each ThemeDto to create Theme entities
        foreach (var themeDto in themeDtos)
        {
            var theme = new Theme
            {
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
                ThemeName = themeDto.ThemeName,
                ApiKeyID = apiKey.Id,
                ApiKey = apiKey,
                UserId = user.Id,
                User = user
            };

            apiKey.Themes.Add(theme);
        }

        // Add the new ApiKey (with Themes) to the DbContext
        dbContext.Set<ApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }


    // GraphQL
    public async Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<AccessKeyPermission> permissions)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var graphQLApiKey = new GraphQLApiKey
        {
            KeyName = keyName,
            UserId = user.Id,
            User = user,
            AccessKeyPermissions = permissions,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn
        };

        dbContext.Set<GraphQLApiKey>().Add(graphQLApiKey);
        await dbContext.SaveChangesAsync();

        return graphQLApiKey;
    }
}

