using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Services.Interfaces;

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

    public async Task<IQueryable<Species>?> GetSpecies([Service] IKeyService keyService, [Service] IUserService userService, [Service] IDbContextService dbContextService, string? encryptedKey = null)
    {
        BaseDbContext? dbContext;

        if (string.IsNullOrWhiteSpace(encryptedKey))
        {
            var userResult = await GetUserFromContext(userService, dbContextService);
            if (userResult.Error != null) return null;

            dbContext = userResult.DbContext as BaseDbContext;
        }
        else
        {
            var keyResult = await keyService.ProcessAccessKey(encryptedKey, GraphQLConstants.GetSpeciesQuery);
            if (keyResult.actionResult != null || keyResult.dbContext == null) return null;

            dbContext = keyResult.dbContext as BaseDbContext;
        }

        return dbContext?.GetSpecies();
    }

    public async Task<IQueryable<Organization>?> GetOrganizations([Service] IKeyService keyService, [Service] IUserService userService, [Service] IDbContextService dbContextService, string? encryptedKey = null)
    {
        BaseDbContext? dbContext;

        if (string.IsNullOrWhiteSpace(encryptedKey))
        {
            var userResult = await GetUserFromContext(userService, dbContextService);
            if (userResult.Error != null) return null;

            dbContext = userResult.DbContext as BaseDbContext;
        }
        else
        {
            var keyResult = await keyService.ProcessAccessKey(encryptedKey, GraphQLConstants.GetOrganizationsQuery);
            if (keyResult.actionResult != null || keyResult.dbContext == null) return null;

            dbContext = keyResult.dbContext as BaseDbContext;
        }

        return dbContext?.GetOrganizations();
    }

    private async Task<(DbContext? DbContext, IActionResult? Error)> GetUserFromContext(IUserService userService, IDbContextService dbContextService)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return (null, null);

        var (user, error) = await userService.GetUserAsync(httpContext);
        if (error != null) return (null, error);

        var dbContext = await dbContextService.GetUserDatabaseContext(user);
        return (dbContext, null);
    }

    public List<string?> GetAvailableGraphQLQueries()
    {
        var constants = typeof(GraphQLConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetValue(null) as string)
            .ToList();

        return constants;
    }
}
