using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Services;
using NetBackend.Models.User;

namespace NetBackend.Services;

public interface IKeyService
{
    Task<AccessKey> EncryptAndStoreAccessKey(ApiKey apiKey, User user);
    Task<(ApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string userId);
    Task<(ApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey);
    Task<ApiKey> CreateApiKey(User user, string keyName, List<string> endpoints);
    Task<(DbContext?, IActionResult?)> ProcessAccessKey(string encryptedKey);
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

    public async Task<ApiKey> CreateApiKey(User user, string keyName, List<string> endpoints)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName("Main");

        // Create a new ApiKey instance
        var apiKey = new ApiKey
        {
            UserId = user.Id,
            KeyName = keyName,
            AccessibleEndpoints = endpoints,
            User = user
        };

        // Add the new ApiKey to the DbContext
        dbContext.Set<ApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }

    public async Task<AccessKey> EncryptAndStoreAccessKey(ApiKey apiKey, User user)
    {
        var dbContext = await _databaseContextService.GetUserDatabaseContext(user);
        var dataToEncrypt = $"Id:{apiKey.Id}";
        var encryptedKey = _cryptologyService.Encrypt(dataToEncrypt, "SecretKey"); // TODO: STORE THIS SECRET KEY IN A SAFE PLACE

        var accessKey = new AccessKey
        {
            EncryptedKey = encryptedKey,
        };

        dbContext.Set<AccessKey>().Add(accessKey);
        await dbContext.SaveChangesAsync();

        // Create and return an AccessKeyDto instance
        return accessKey;
    }

    public async Task<(ApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey)
    {
        // Decrypt the data
        var decryptedData = _cryptologyService.Decrypt(encryptedKey, "SecretKey");
        var dataParts = decryptedData.Split(',');
        var idString = dataParts.FirstOrDefault(part => part.StartsWith("Id:"))?.Split(':')[1];

        if (idString == null || !int.TryParse(idString, out var id))
        {
            return (null, new BadRequestObjectResult("Invalid encrypted key format."));
        }

        var dbContext = await _databaseContextService.GetDatabaseContextByName("Main");
        if (dbContext == null)
        {
            return (null, new NotFoundObjectResult("Database context not found."));
        }

        var apiKey = await dbContext.Set<ApiKey>().FirstOrDefaultAsync(a => a.Id == id);
        if (apiKey == null)
        {
            return (null, new NotFoundObjectResult("Api Key not found."));
        }

        return (apiKey, null);
    }

    public async Task<(ApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId)
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

    public async Task<(DbContext?, IActionResult?)> ProcessAccessKey(string encryptedKey)
    {
        var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (null, errorResult);

        var httpContext = _httpContextAccessor.HttpContext;
        if (apiKey != null
            && apiKey.AccessibleEndpoints != null
            && !string.IsNullOrEmpty(httpContext?.Request.Path.Value)
            && !apiKey.AccessibleEndpoints.Contains(httpContext.Request.Path.Value))
        {
            return (null, new UnauthorizedResult());
        }

        if (apiKey?.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

        var mainDbContext = await _databaseContextService.GetDatabaseContextByName("Main");
        string databaseName = mainDbContext.Set<User>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";

        var selectedContext = await _databaseContextService.GetDatabaseContextByName(databaseName);
        return (selectedContext, null);
    }
}