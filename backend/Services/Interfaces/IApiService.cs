using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IApiService
{
    Task<ApiKey> CreateRESTApiKey(UserModel user, string keyName, List<ThemeDto> themes);
    Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<AccessKeyPermission> permissions);
}