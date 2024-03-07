using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces.Keys;

public interface IGraphQLKeyService
{
    // GraphQL
    Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<AccessKeyPermission> permissions);
    Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessGraphQLAccessKey(string encryptedKey);
    Task<List<AccessKeyPermission>> GetGraphQLAccessKeyPermissions(Guid graphQLApiKeyId);
    Task<List<GraphQLApiKey>> GetGraphQLApiKeysByUserId(string userId);
    Task<IActionResult> ToggleGraphQLApiKey(Guid graphQLApiKeyId, bool isEnabled);
}