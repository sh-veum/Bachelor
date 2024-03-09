using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces.Keys;

public interface IGraphQLKeyService
{
    Task<GraphQLApiKey> CreateGraphQLApiKey(UserModel user, string keyName, List<AccessKeyPermission> permissions);
    Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessGraphQLAccessKey(string encryptedKey, HttpContext httpContext);
    Task<List<AccessKeyPermission>> GetGraphQLAccessKeyPermissions(Guid graphQLApiKeyId);
    Task<List<GraphQLApiKey>> GetGraphQLApiKeysByUserId(string userId);
    Task<IActionResult> ToggleGraphQLApiKey(Guid graphQLApiKeyId, bool isEnabled);
    Task<string> EncryptAndStoreGraphQLAccessKey(GraphQLApiKey graphQLApiKey);
    Task<IActionResult> RemoveGraphQLAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptGraphQLAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptGraphQLAccessKeyUserCheck(string encryptedKey, string currentUserId);
}