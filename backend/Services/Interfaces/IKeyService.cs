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
    Task<(DbContext?, IActionResult?)> ProcessAccessKey(string encryptedKey, string? query = null);
    Task<IActionResult> RemoveAccessKey(string encryptedKey);
}