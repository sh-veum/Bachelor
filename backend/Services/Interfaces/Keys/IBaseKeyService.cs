using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.Keys;

namespace NetBackend.Services.Interfaces.Keys;

public interface IBaseKeyService
{
    Task<string> EncryptAndStoreAccessKey(IApiKey iApiKey);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId);
    Task<IActionResult> RemoveAccessKey(string encryptedKey);
    Task<IActionResult> ToggleApiKeyEnabledStatus<T>(Guid keyId, bool isEnabled) where T : IApiKey;
    Task<IActionResult> DeleteApiKeyById(Guid id, string typePart);
}