using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IApiService
{
    object GetDtoStructure(Type dtoType);
    string GetFriendlyTypeName(Type type);
    Task<ApiKey> CreateRESTApiKey(UserModel user, string keyName, List<string> endpoints);
    Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<string> allowedQueries);
}