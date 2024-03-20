using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces.Keys;

public interface IKafkaKeyService
{
    Task<KafkaKey> CreateKafkaKey(UserModel user, string keyName, List<string> topics);
    Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessKafkaAccessKey(string encryptedKey, HttpContext httpContext);
    Task<List<KafkaKey>> GetKafkaKeysByUserId(string userId);
    Task<List<string>> GetKafkaKeyTopics(Guid kafkaKeyID);
    Task<IActionResult> ToggleKafkaKey(Guid apiKeyId, bool isEnabled);
    Task<string> EncryptAndStoreKafkaAccessKey(KafkaKey kafkaKey);
    Task<IActionResult> RemoveKafkaAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptKafkaAccessKey(string encryptedKey);
    Task<(IApiKey?, IActionResult?)> DecryptKafkaAccessKeyUserCheck(string encryptedKey, string currentUserId);
    Task<IActionResult> DeleteKafkaKeyById(Guid id);
    Task<(DbContext?, IActionResult?)> ResolveDbContextAsync(AccessKeyDto? model, HttpContext httpContext);
}