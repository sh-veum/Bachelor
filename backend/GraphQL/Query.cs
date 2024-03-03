using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Tools;

namespace NetBackend.GraphQL;

public class Query
{
    private readonly ILogger<Query> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Query(ILogger<Query> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IQueryable<Species>?> GetSpecies(
        [Service] IKeyService keyService,
        [Service] IUserService userService,
        [Service(ServiceKind.Synchronized)] IDbContextService dbContextService,
        string? encryptedKey = null)
    {
        try
        {
            BaseDbContext? dbContext;

            if (string.IsNullOrWhiteSpace(encryptedKey))
            {
                var userResult = await GetContextFromUser(userService, dbContextService);
                if (userResult.Error != null) return null;

                dbContext = userResult.DbContext as BaseDbContext;
            }
            else
            {
                var keyResult = await keyService.ProcessGraphQLAccessKey(encryptedKey);
                if (keyResult.actionResult != null || keyResult.dbContext == null) return null;

                dbContext = keyResult.dbContext as BaseDbContext;
            }

            return dbContext?.GetSpecies();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting species");
            return null;
        }
    }

    public async Task<IQueryable<Organization>?> GetOrganizations([Service] IKeyService keyService, [Service] IUserService userService, [Service(ServiceKind.Synchronized)] IDbContextService dbContextService, string? encryptedKey = null)
    {
        try
        {
            BaseDbContext? dbContext;

            if (string.IsNullOrWhiteSpace(encryptedKey))
            {
                var userResult = await GetContextFromUser(userService, dbContextService);
                if (userResult.Error != null) return null;

                dbContext = userResult.DbContext as BaseDbContext;
            }
            else
            {
                var keyResult = await keyService.ProcessGraphQLAccessKey(encryptedKey);
                if (keyResult.actionResult != null || keyResult.dbContext == null) return null;

                dbContext = keyResult.dbContext as BaseDbContext;
            }

            return dbContext?.GetOrganizations();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations");
            return null;
        }
    }

    public List<ClassInfo> GetAvailableClassTables()
    {
        var classInfos = new List<ClassInfo>();

        foreach (var tableName in GraphQLConstants.AvailableQueryTables)
        {
            var type = Type.GetType($"NetBackend.Models.{tableName}");
            if (type != null)
            {
                var classInfo = ReflectionHelper.GetClassInfo(type);
                classInfos.Add(classInfo);
            }
        }

        return classInfos;
    }

    public List<List<string>> GetAvailableQueries()
    {
        return [.. GraphQLConstants.AvailableQueries];
    }

    // NOTE:
    // Need to split it up into two since the GraphQL schema cant detect the return type of the method since it returns IApiKeyDto
    // NOTE:
    // cant fetch GetRestApiKeysByUser and GetGraphQLApiKeysByUser unless (ServiceKind.Synchronized) is added to the IKeyService
    public async Task<List<ApiKeyDto>> GetRestApiKeysByUser(
        [Service(ServiceKind.Synchronized)] IKeyService keyService,
        [Service] IUserService userService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor), "HttpContextAccessor's HttpContext is null.");
        var userResult = await userService.GetUserAsync(httpContext);
        var user = userResult.user;

        var apiKeys = await keyService.GetRestApiKeysByUserId(user.Id);

        var apiKeysDto = new List<ApiKeyDto>();

        foreach (var apiKey in apiKeys)
        {
            var themes = await keyService.GetApiKeyThemes(apiKey.Id);
            var apiKeyDto = new ApiKeyDto
            {
                Id = apiKey.Id,
                KeyName = apiKey.KeyName,
                CreatedBy = userResult.user.Email ?? "error fetching user email",
                ExpiresIn = apiKey.ExpiresIn,
                Themes = themes.Select(t => new ThemeDto
                {
                    Id = t.Id,
                    ThemeName = t.ThemeName,
                    AccessibleEndpoints = t.AccessibleEndpoints
                }).ToList(),
                IsEnabled = apiKey.IsEnabled
            };
            apiKeysDto.Add(apiKeyDto);
        }

        return apiKeysDto;
    }

    public async Task<List<GraphQLApiKeyDto>> GetGraphQLApiKeysByUser(
    [Service(ServiceKind.Synchronized)] IKeyService keyService,
    [Service] IUserService userService,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor), "HttpContextAccessor's HttpContext is null.");
        var userResult = await userService.GetUserAsync(httpContext);
        var user = userResult.user;

        var apiKeys = await keyService.GetGraphQLApiKeysByUserId(user.Id);
        var apiKeysDto = new List<GraphQLApiKeyDto>();

        foreach (var apiKey in apiKeys)
        {
            var permissions = await keyService.GetGraphQLAccessKeyPermissions(apiKey.Id);
            var apiKeyDto = new GraphQLApiKeyDto
            {
                Id = apiKey.Id,
                KeyName = apiKey.KeyName,
                CreatedBy = user.Email ?? "error fetching user email",
                ExpiresIn = apiKey.ExpiresIn,
                GraphQLAccessKeyPermissionDto = permissions.Select(p => new GraphQLAccessKeyPermissionDto
                {
                    QueryName = p.QueryName,
                    AllowedFields = p.AllowedFields ?? []
                }).ToList(),
                IsEnabled = apiKey.IsEnabled
            };
            apiKeysDto.Add(apiKeyDto);
        }

        return apiKeysDto;
    }


    private async Task<(DbContext? DbContext, IActionResult? Error)> GetContextFromUser(IUserService userService, IDbContextService dbContextService)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return (null, null);

            var (user, error) = await userService.GetUserAsync(httpContext);
            if (error != null) return (null, error);

            var dbContext = await dbContextService.GetUserDatabaseContext(user);
            return (dbContext, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error context from user");
            return (null, new StatusCodeResult(StatusCodes.Status500InternalServerError));
        }
    }
}
