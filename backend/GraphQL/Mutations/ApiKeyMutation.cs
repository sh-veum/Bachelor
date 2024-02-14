using NetBackend.Models.Keys;
using NetBackend.Services;

public class ApiKeyMutation
{
    public async Task<GraphQLApiKey> CreateGraphQLApiKey(
        [Service] IKeyService keyService,
        string keyName,
        List<string> allowedQueries,
        string userEmail)
    {
        var apiKey = await keyService.CreateGraphQLApiKey(userEmail, keyName, allowedQueries);

        return apiKey;
    }
}