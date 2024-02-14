using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;

public class ApiKeyMutation
{
    public async Task<AccessKeyDto> CreateGraphQLApiKey(
        [Service] IKeyService keyService,
        [Service] IApiService apiService,
        [Service] IDbContextService databaseContextService,
        string keyName,
        List<string> allowedQueries,
        string userEmail)
    {
        var dbContext = await databaseContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var user = await dbContext.Set<UserModel>().FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var graphQLApiKey = await apiService.CreateGraphQLApiKey(user, keyName, allowedQueries);

        // Encrypt and store access key
        var accesKey = await keyService.EncryptAndStoreAccessKey(graphQLApiKey, user);

        var accesKeyDto = new AccessKeyDto
        {
            EncryptedKey = accesKey ?? ""
        };

        return accesKeyDto;
    }
}