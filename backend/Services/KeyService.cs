using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using Netbackend.Services;
using NetBackend.Models.Keys.Dto;
using NetBackend.Models.User;

namespace NetBackend.Services;

public interface IKeyService
{
    Task<AccessKey> EncryptAndStoreAccessKey(ApiKey apiKey, User user);
    Task<(ApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey, string userId);
    Task<ApiKey> CreateApiKey(User user, string keyName, List<string> endpoints);
}

public class KeyService : IKeyService
{
    private readonly ITokenService _tokenService;
    private readonly IDatabaseContextService _databaseContextService;

    public KeyService(ITokenService tokenService, IDatabaseContextService databaseContextService)
    {
        _tokenService = tokenService;
        _databaseContextService = databaseContextService;
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
        var encryptedKey = _tokenService.Encrypt(dataToEncrypt, "SecretKey"); // TODO: STORE THIS SECRET KEY IN A SAFE PLACE

        var accessKey = new AccessKey
        {
            EncryptedKey = encryptedKey,
        };

        dbContext.Set<AccessKey>().Add(accessKey);
        await dbContext.SaveChangesAsync();

        // Create and return an AccessKeyDto instance
        return accessKey;
    }

    public async Task<(ApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey, string currentUserId)
    {
        // Decrypt the data
        var decryptedData = _tokenService.Decrypt(encryptedKey, "SecretKey");
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

        // Check if the current user is the owner of the API key
        if (apiKey.UserId != currentUserId)
        {
            // If not, return unauthorized
            return (null, new UnauthorizedResult());
        }

        // Return the found ApiKey with no error
        return (apiKey, null);
    }

}