using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IKeyService
{
    Task<string> EncryptAndStoreAccessKey(IApiKey apiKey, UserModel user);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string userId);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey);
    Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessAccessKey(string encryptedKey);
    Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessGraphQLAccessKey(string encryptedKey);
    Task<IActionResult> RemoveAccessKey(string encryptedKey);
    Task<List<Theme>> GetApiKeyThemes(int apiKeyID);
    Task<List<Theme>> GetThemesByUserId(string userId);
    Task<Theme> CreateTheme(Theme theme);
    Task<Theme> UpdateTheme(Theme theme);
    Task<IActionResult> DeleteTheme(Guid themeId);
    Task<List<AccessKeyPermission>> GetGraphQLAccessKeyPermissions(int graphQLApiKeyId);
    Task<List<ApiKey>> GetRestApiKeysByUserId(string userId);
    Task<List<GraphQLApiKey>> GetGraphQLApiKeysByUserId(string userId);
}