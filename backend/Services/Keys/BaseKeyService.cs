using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class BaseKeyService : IBaseKeyService
{
    protected readonly ILogger _logger;
    protected readonly IDbContextService _dbContextService;
    protected readonly ICryptoService _cryptoService;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseKeyService(
        ILogger logger,
        IDbContextService dbContextService,
        ICryptoService cryptoService,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _dbContextService = dbContextService;
        _cryptoService = cryptoService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> EncryptAndStoreAccessKey(IApiKey iApiKey, UserModel user)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        var dataToEncrypt = $"Id:{iApiKey.Id},Type:{iApiKey.GetType().Name}";
        var encryptedKey = _cryptoService.Encrypt(dataToEncrypt, SecretConstants.SecretKey);

        iApiKey.KeyHash = ComputeHash.ComputeSha256Hash(encryptedKey);

        if (iApiKey is RestApiKey restApiKey)
        {
            dbContext.Entry(restApiKey).State = EntityState.Modified;
        }
        else if (iApiKey is GraphQLApiKey gqlApi)
        {
            dbContext.Entry(gqlApi).State = EntityState.Modified;
        }
        await dbContext.SaveChangesAsync();

        return encryptedKey;
    }

    public async Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey)
    {
        var decryptedData = _cryptoService.Decrypt(encryptedKey, SecretConstants.SecretKey);
        var dataParts = decryptedData.Split(',');

        var typePart = dataParts.FirstOrDefault(part => part.StartsWith("Type:"))?.Split(':')[1];
        var idPart = dataParts.FirstOrDefault(part => part.StartsWith("Id:"))?.Split(':')[1];

        if (typePart == null || idPart == null || !Guid.TryParse(idPart, out var id))
        {
            return (null, new BadRequestObjectResult("Invalid encrypted key format."));
        }

        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        return await FetchApiKeyAsync(typePart, id, dbContext);
    }

    public async Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId)
    {
        var (apiKey, result) = await DecryptAccessKey(encryptedKey);
        if (result != null)
        {
            return (null, result);
        }

        if (apiKey?.UserId != currentUserId)
        {
            return (null, new UnauthorizedResult());
        }

        return (apiKey, null);
    }

    public async Task<IActionResult> RemoveAccessKey(string encryptedKey)
    {
        var (apiKey, actionResult) = await DecryptAccessKey(encryptedKey);
        if (actionResult != null)
        {
            return actionResult;
        }

        // Remove the Api Key from the database
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        if (apiKey != null)
        {
            RemoveApiKey(apiKey, dbContext);
            await dbContext.SaveChangesAsync();
        }

        return new OkResult();
    }

    protected async Task<IActionResult> ToggleApiKeyEnabledStatus<T>(Guid keyId, bool isEnabled) where T : IApiKey
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        var apiKey = await dbContext.Set<T>().FirstOrDefaultAsync(a => a.Id == keyId);

        if (apiKey == null)
        {
            return new NotFoundObjectResult(typeof(T) == typeof(RestApiKey) ? "REST API key not found." : "GraphQLAPI key not found.");
        }

        apiKey.IsEnabled = isEnabled;
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("{KeyType} with ID {KeyId} has been {Action}.", typeof(T).Name, keyId, isEnabled ? "enabled" : "disabled");
        return new OkResult();
    }

    // Private methods
    private static void RemoveApiKey(IApiKey apiKey, DbContext dbContext)
    {
        switch (apiKey)
        {
            case RestApiKey api:
                dbContext.Set<RestApiKey>().Remove(api);
                break;
            case GraphQLApiKey gqlApi:
                dbContext.Set<GraphQLApiKey>().Remove(gqlApi);
                break;
        }
    }

    private static async Task<(IApiKey?, IActionResult?)> FetchApiKeyAsync(string typePart, Guid id, DbContext dbContext)
    {
        if (dbContext == null)
        {
            return (null, new NotFoundObjectResult("Database context not found."));
        }

        switch (typePart.ToLowerInvariant())
        {
            // TODO use constants instead
            case "restapikey":
                var apiKey = await dbContext.Set<RestApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                return apiKey == null ? (null, new NotFoundObjectResult("Api Key not found.")) : (apiKey, null);
            case "graphqlapikey":
                var graphQLApiKey = await dbContext.Set<GraphQLApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                return graphQLApiKey == null ? (null, new NotFoundObjectResult("GraphQL Api Key not found.")) : (graphQLApiKey, null);
            default:
                return (null, new BadRequestObjectResult($"Unknown key type: {typePart}."));
        }
    }
}