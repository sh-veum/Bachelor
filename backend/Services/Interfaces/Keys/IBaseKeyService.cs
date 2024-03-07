using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces.Keys;

public interface IBaseKeyService
{
    Task<string> EncryptAndStoreAccessKey(IApiKey iApiKey, UserModel user);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId);
    Task<IActionResult> RemoveAccessKey(string encryptedKey);
}