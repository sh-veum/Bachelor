using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class BaseKeyService : IBaseKeyService
{
    private readonly ILogger<BaseKeyService> _logger;
    private readonly IDbContextService _dbContextService;
    private readonly ICryptoService _cryptoService;

    public BaseKeyService(
        ILogger<BaseKeyService> logger,
        IDbContextService dbContextService,
        ICryptoService cryptoService)
    {
        _logger = logger;
        _dbContextService = dbContextService;
        _cryptoService = cryptoService;
    }

    public async Task<string> EncryptAndStoreAccessKey(IApiKey iApiKey)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        var dataToEncrypt = $"Id:{iApiKey.Id},Type:{iApiKey.GetType().Name}";
        var encryptedKey = _cryptoService.Encrypt(dataToEncrypt, SecretConstants.SecretKey);

        iApiKey.KeyHash = ComputeHash.ComputeSha256Hash(encryptedKey);

        dbContext.Entry(iApiKey).State = EntityState.Modified;

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
            await RemoveApiKeyAsync(apiKey, dbContext);
        }

        return new OkResult();
    }

    public async Task<IActionResult> DeleteApiKeyById(Guid id, string typePart)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        var (apiKey, actionResult) = await FetchApiKeyAsync(typePart, id, dbContext);
        if (actionResult != null)
        {
            return actionResult;
        }

        if (apiKey == null)
        {
            return new NotFoundObjectResult("API key not found.");
        }

        await RemoveApiKeyAsync(apiKey, dbContext);

        _logger.LogInformation("{KeyType} with ID {KeyId} has been deleted.", typePart, id);
        return new OkResult();

    }

    public async Task<IActionResult> ToggleApiKeyEnabledStatus<T>(Guid keyId, bool isEnabled) where T : IApiKey
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
    private static async Task RemoveApiKeyAsync(IApiKey apiKey, DbContext dbContext)
    {
        switch (apiKey)
        {
            case RestApiKey api:
                // var themes = await dbContext.Set<Theme>().Where(t => t.RestApiKeyID == api.Id).ToListAsync();
                // dbContext.Set<Theme>().RemoveRange(themes);
                // await dbContext.SaveChangesAsync();

                dbContext.Set<RestApiKey>().Remove(api);
                await dbContext.SaveChangesAsync();
                break;

            case GraphQLApiKey gqlApi:
                var permissions = await dbContext.Set<AccessKeyPermission>()
                                         .Where(p => p.GraphQLApiKeyId == gqlApi.Id)
                                         .ToListAsync();

                dbContext.Set<AccessKeyPermission>().RemoveRange(permissions);
                await dbContext.SaveChangesAsync();

                dbContext.Set<GraphQLApiKey>().Remove(gqlApi);
                await dbContext.SaveChangesAsync();
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