using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class GraphQLKeyService : BaseKeyService, IGraphQLKeyService
{
    public GraphQLKeyService(
        ILogger<GraphQLKeyService> logger,
        IDbContextService dbContextService,
        ICryptoService cryptoService,
        IHttpContextAccessor httpContextAccessor)
        : base(logger, dbContextService, cryptoService, httpContextAccessor)
    {
    }

    public async Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<AccessKeyPermission> permissions)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var graphQLApiKey = new GraphQLApiKey
        {
            KeyName = keyName,
            UserId = user.Id,
            User = user,
            AccessKeyPermissions = permissions,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn,
            IsEnabled = true
        };

        dbContext.Set<GraphQLApiKey>().Add(graphQLApiKey);
        await dbContext.SaveChangesAsync();

        return graphQLApiKey;
    }

    public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessGraphQLAccessKey(string encryptedKey)
    {
        var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (null, errorResult);

        if (apiKey == null)
        {
            return (null, new BadRequestObjectResult("API key not found."));
        }

        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        if (!apiKey.IsEnabled) return (null, new BadRequestObjectResult("API key is disabled."));

        var expirationDate = apiKey.CreatedAt.AddDays(apiKey.ExpiresIn);
        if (DateTime.UtcNow > expirationDate)
        {
            return (null, new UnauthorizedResult());
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return (null, new BadRequestObjectResult("HttpContext is null."));
        }

        // Retrieve the stored GraphQL query from HttpContext
        var graphqlQuery = httpContext.Items["GraphQLQuery"] as string;

        if (string.IsNullOrEmpty(graphqlQuery))
        {
            return (null, new UnauthorizedResult()); // No query to authorize
        }

        if (apiKey is GraphQLApiKey api)
        {
            var permission = await GetGraphQLAccessKeyPermissions(apiKey.Id);
            var isAuthorized = CheckQueryAuthorization(graphqlQuery, permission);
            if (!isAuthorized)
            {
                return (null, new UnauthorizedResult());
            }

            // Compute hash of the encrypted key and check if it exists in the database
            var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
            var iApiKey = await mainDbContext.Set<GraphQLApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
            if (iApiKey == null)
            {
                return (null, new UnauthorizedResult());
            }
        }

        if (apiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

        string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";
        var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);

        return (selectedContext, null);
    }

    public async Task<List<AccessKeyPermission>> GetGraphQLAccessKeyPermissions(Guid graphQLApiKeyId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var accessKeyPermissions = await mainDbContext.Set<AccessKeyPermission>()
            .Where(p => p.GraphQLApiKeyId == graphQLApiKeyId)
            .ToListAsync();

        return accessKeyPermissions;
    }

    public async Task<List<GraphQLApiKey>> GetGraphQLApiKeysByUserId(string userId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var apiKeys = await mainDbContext.Set<GraphQLApiKey>()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return apiKeys;
    }

    public Task<IActionResult> ToggleGraphQLApiKey(Guid graphQLApiKeyId, bool isEnabled) => ToggleApiKeyEnabledStatus<GraphQLApiKey>(graphQLApiKeyId, isEnabled);

    // Private methods
    private bool CheckQueryAuthorization(string graphqlQuery, List<AccessKeyPermission> permissions)
    {
        _logger.LogInformation("graphqlQuery {graphqlQuery}", graphqlQuery);
        var parsedQuery = GraphQLQueryParser.ParseQuery(graphqlQuery);

        if (parsedQuery.Count == 0)
        {
            _logger.LogWarning("Parsed query is empty. Authorization check failed.");
            return false;
        }

        _logger.LogInformation("Starting authorization check for GraphQL query.");

        foreach (var operation in parsedQuery)
        {
            var operationName = operation.Key.ToLowerInvariant();
            var requestedFields = operation.Value.Select(f => f.ToLowerInvariant()).ToList();

            _logger.LogInformation($"Checking operation: {operation.Key} with fields: {string.Join(", ", operation.Value)}");

            var permission = permissions.FirstOrDefault(p => p.QueryName.Equals(operationName, StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                _logger.LogWarning($"Unauthorized operation: {operation.Key}. No matching permission found.");
                return false;
            }

            var allowedFields = permission.AllowedFields?.Select(f => f.ToLowerInvariant()).ToList() ?? [];

            _logger.LogInformation($"Allowed fields for operation '{operation.Key}': {string.Join(", ", allowedFields)}");

            foreach (var field in requestedFields)
            {
                if (!allowedFields.Contains(field))
                {
                    _logger.LogWarning($"Unauthorized field: {field} in operation: {operation.Key}. Field is not in the allowed list.");
                    return false;
                }
            }
        }

        _logger.LogInformation("GraphQL query authorization check passed.");

        return true;
    }
}