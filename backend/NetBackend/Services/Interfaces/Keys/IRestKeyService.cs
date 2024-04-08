using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces.Keys;

public interface IRestKeyService
{
    Task<RestApiKey> CreateRESTApiKey(UserModel user, string keyName, List<Guid> themeIds);
    Task<(IActionResult?, RestApiKey?)> ValidateRestAccessKey(string encryptedKey);
    Task<(DbContext? dbContext, IActionResult? actionResult, string? userId)> ProcessAndGetDbContextAndUserIdFromKey(string encryptedKey, HttpContext httpContext);
    Task<List<RestApiKey>> GetRestApiKeysByUserId(string userId);
    Task<List<Theme>> GetRESTApiKeyThemes(Guid restApiKeyID);
    Task<List<Theme>> GetThemesByUserId(string userId);
    Task<Theme> CreateTheme(Theme theme);
    Task<Theme> UpdateTheme(Theme theme);
    Task<IActionResult> DeleteTheme(Guid themeId);
    Task<IActionResult> ToggleRestApiKey(Guid apiKeyId, bool isEnabled);
    Task<string> EncryptAndStoreRestAccessKey(RestApiKey restApiKey);
    Task<IActionResult> RemoveRestAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptRestAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptRestAccessKeyUserCheck(string encryptedKey, string currentUserId);
    Task<IActionResult> DeleteRestApiKeyById(Guid id);
    Task<(DbContext?, IActionResult?, string? userId)> ResolveDbContextAndUserId(AccessKeyDto? model, HttpContext httpContext);
}