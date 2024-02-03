using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Services;
using NetBackend.Models.User;

namespace NetBackend.Services;

public interface IKeyService
{
    Task EncryptAndStoreAccessKey(ApiKey apiKey, User user);
    Task<ActionResult<ApiKey>> DecryptAccessKey(string encryptedKey);
    Task<ApiKey> CreateApiKey(User user, List<string> endpoints);
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

    public async Task<ApiKey> CreateApiKey(User user, List<string> endpoints)
    {
        var dbContext = await _databaseContextService.GetDatabaseContextByName("Main");

        // Create a new ApiKey instance
        var apiKey = new ApiKey
        {
            UserId = user.Id,
            AccessibleEndpoints = endpoints,
            User = user
        };

        // Add the new ApiKey to the DbContext
        dbContext.Set<ApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }

    public async Task EncryptAndStoreAccessKey(ApiKey apiKey, User user)
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
    }

    public async Task<ActionResult<ApiKey>> DecryptAccessKey(string encryptedKey)
    {
        // Decrypt the data
        var decryptedData = _tokenService.Decrypt(encryptedKey, "SecretKey");

        // Parse the decrypted data to extract DatabaseName and Id
        var dataParts = decryptedData.Split(',');
        var idString = dataParts.FirstOrDefault(part => part.StartsWith("Id:"))?.Split(':')[1];

        if (idString == null || !int.TryParse(idString, out var id))
        {
            // If parsing fails or data is missing, return an appropriate error
            return new BadRequestObjectResult("Invalid encrypted key format.");
        }

        // Get the database context using the extracted DatabaseName
        var dbContext = await _databaseContextService.GetDatabaseContextByName("Main");
        if (dbContext == null)
        {
            // If the database context is not found, return an appropriate error
            return new NotFoundObjectResult("Database context not found.");
        }

        // Query the database for the ApiKey using the extracted Id
        var apiKey = await dbContext.Set<ApiKey>().FirstOrDefaultAsync(a => a.Id == id);

        if (apiKey == null)
        {
            // If the ApiKey is not found, return an appropriate error
            return new NotFoundObjectResult("Api Key not found.");
        }

        // Return the found ApiKey
        return apiKey;
    }
}