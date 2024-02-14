using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Services;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services;

public interface IKeyService
{
    Task<string> EncryptAndStoreAccessKey(IApiKey apiKey, UserModel user);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string userId);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey);
    Task<ApiKey> CreateApiKey(UserModel user, string keyName, List<string> endpoints);
    Task<(DbContext?, IActionResult?)> ProcessAccessKey(string encryptedKey, string? query = null);
    Task<IActionResult> RemoveAccessKey(string encryptedKey);
    Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<string> allowedQueries);
}

public class KeyService : IKeyService
{
    private readonly ILogger<KeyService> _logger;
    private readonly ICryptologyService _cryptologyService;
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KeyService(
        ILogger<KeyService> logger,
        ICryptologyService cryptologyService,
        IDatabaseContextService databaseContextService,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _cryptologyService = cryptologyService;
        _databaseContextService = databaseContextService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiKey> CreateApiKey(UserModel user, string keyName, List<string> endpoints)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        // Create a new ApiKey instance
        var apiKey = new ApiKey
        {
            UserId = user.Id,
            KeyName = keyName,
            AccessibleEndpoints = endpoints,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn
        };

        // Add the new ApiKey to the DbContext
        dbContext.Set<ApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }

    public async Task<string> EncryptAndStoreAccessKey(IApiKey apiKey, UserModel user)
    {
        var dbContext = await _databaseContextService.GetUserDatabaseContext(user);
        var dataToEncrypt = $"Id:{apiKey.Id},Type:{apiKey.GetType().Name}";
        var encryptedKey = _cryptologyService.Encrypt(dataToEncrypt, SecretConstants.SecretKey);

        // Compute hash of the encrypted key.
        var keyHash = ComputeSha256Hash(encryptedKey);

        var accessKey = new AccessKey
        {
            KeyHash = keyHash
        };

        dbContext.Set<AccessKey>().Add(accessKey);
        await dbContext.SaveChangesAsync();

        return encryptedKey;
    }

    public async Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey)
    {
        // Decrypt the data
        var decryptedData = _cryptologyService.Decrypt(encryptedKey, SecretConstants.SecretKey);
        var dataParts = decryptedData.Split(',');

        var typePart = dataParts.FirstOrDefault(part => part.StartsWith("Type:"))?.Split(':')[1];
        var idString = dataParts.FirstOrDefault(part => part.StartsWith("Id:"))?.Split(':')[1];

        _logger.LogInformation($"Type: {typePart}, Id: {idString}");
        _logger.LogInformation($"decryptedData: {decryptedData}");

        if (typePart == null || idString == null || !int.TryParse(idString, out var id))
        {
            return (null, new BadRequestObjectResult("Invalid encrypted key format."));
        }

        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        if (dbContext == null)
        {
            return (null, new NotFoundObjectResult("Database context not found."));
        }

        // Determine the type of the key and query accordingly
        if (typePart.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            var apiKey = await dbContext.Set<ApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (apiKey == null)
            {
                return (null, new NotFoundObjectResult("Api Key not found."));
            }
            return (apiKey, null);
        }
        else if (typePart.Equals("GraphQLApiKey", StringComparison.OrdinalIgnoreCase))
        {
            var graphQLApiKey = await dbContext.Set<GraphQLApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (graphQLApiKey == null)
            {
                return (null, new NotFoundObjectResult("GraphQL Api Key not found."));
            }
            return (graphQLApiKey, null);
        }
        else
        {
            return (null, new BadRequestObjectResult($"Unknown key type: {typePart}."));
        }
    }

    public async Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId)
    {
        var (apiKey, result) = await DecryptAccessKey(encryptedKey);
        if (result != null)
        {
            return (null, result);
        }

        // Check if the current user is the owner of the API key
        if (apiKey?.UserId != currentUserId)
        {
            // If not, return unauthorized
            return (null, new UnauthorizedResult());
        }

        // Return the found ApiKey with no error
        return (apiKey, null);
    }

    public async Task<(DbContext?, IActionResult?)> ProcessAccessKey(string encryptedKey, string? query = null)
    {
        var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (null, errorResult);

        if (apiKey == null)
        {
            return (null, new BadRequestObjectResult("API key not found."));
        }

        var expirationDate = apiKey.CreatedAt.AddDays(apiKey.ExpiresIn);
        if (DateTime.UtcNow > expirationDate)
        {
            return (null, new UnauthorizedResult());
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (apiKey is ApiKey api)
        {
            if (!string.IsNullOrEmpty(httpContext?.Request.Path.Value) &&
                api.AccessibleEndpoints != null &&
                !api.AccessibleEndpoints.Contains(httpContext.Request.Path.Value))
            {
                return (null, new UnauthorizedResult());
            }
        }
        else if (apiKey is GraphQLApiKey gqlApi)
        {
            if (!string.IsNullOrEmpty(query) && gqlApi.AllowedQueries != null && !gqlApi.AllowedQueries.Contains(query))
            {
                return (null, new UnauthorizedResult());
            }
        }

        if (apiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

        var mainDbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";

        var selectedContext = await _databaseContextService.GetDatabaseContextByName(databaseName);

        // Compute hash of the encrypted key and check if it exists in the database
        var keyHash = ComputeSha256Hash(encryptedKey);
        var accessKey = await selectedContext.Set<AccessKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
        if (accessKey == null)
        {
            return (null, new UnauthorizedResult());
        }

        return (selectedContext, null);
    }

    public async Task<IActionResult> RemoveAccessKey(string encryptedKey)
    {
        var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
        if (errorResult != null)
        {
            return errorResult;
        }

        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        if (dbContext == null)
        {
            return new NotFoundObjectResult("Database context not found.");
        }

        if (apiKey != null)
        {
            if (apiKey is ApiKey api)
            {
                dbContext.Set<ApiKey>().Remove(api);
            }
            else if (apiKey is GraphQLApiKey gqlApi)
            {
                dbContext.Set<GraphQLApiKey>().Remove(gqlApi);
            }
            await dbContext.SaveChangesAsync();
        }

        return new OkResult();
    }

    // public async Task<IActionResult> RemoveAccessKey(string encryptedKey)
    // {
    //     var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
    //     if (errorResult != null)
    //     {
    //         return errorResult;
    //     }

    //     if (apiKey != null)
    //     {
    //         apiKey.ExpiresIn = 0;
    //     }

    //     return new OkResult();
    // }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // GraphQL
    public async Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<string> allowedQueries)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var graphQLApiKey = new GraphQLApiKey
        {
            KeyName = keyName,
            UserId = user.Id,
            User = user,
            AllowedQueries = allowedQueries,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn
        };

        dbContext.Set<GraphQLApiKey>().Add(graphQLApiKey);
        await dbContext.SaveChangesAsync();

        return graphQLApiKey;
    }
}

