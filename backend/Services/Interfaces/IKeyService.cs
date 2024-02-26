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
    Task<List<Theme>> GetAccessKeyThemes(int apiKeyID);
    Task<List<AccessKeyPermission>> GetAccessKeyPermissions(int graphQLApiKeyId);
}