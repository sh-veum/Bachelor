using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class KafkaKeyService : IKafkaKeyService
{
    private readonly ILogger<KafkaKeyService> _logger;
    private readonly IDbContextService _dbContextService;
    private readonly IBaseKeyService _baseKeyService;
    private readonly IUserService _userService;

    public KafkaKeyService(ILogger<KafkaKeyService> logger, IDbContextService dbContextService, IBaseKeyService baseKeyService, IUserService userService)
    {
        _logger = logger;
        _dbContextService = dbContextService;
        _baseKeyService = baseKeyService;
        _userService = userService;
    }

    public async Task<KafkaKey> CreateKafkaKey(UserModel user, string keyName, List<string> topics)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var kafkaKey = new KafkaKey
        {
            KeyName = keyName,
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn,
            IsEnabled = true,
            Topics = topics
        };

        dbContext.Set<KafkaKey>().Add(kafkaKey);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Created Kafka Key: {kafkaKey.KeyName}, Topics: {string.Join(", ", kafkaKey.Topics)}");

        return kafkaKey;
    }

    public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessKafkaAccessKey(string encryptedKey, HttpContext httpContext)
    {
        var (apiKey, errorResult) = await _baseKeyService.DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (null, errorResult);

        if (apiKey is not KafkaKey kafkaKey || !kafkaKey.IsEnabled)
        {
            return (null, new BadRequestObjectResult("API key is not found or disabled."));
        }

        var expirationDate = kafkaKey.CreatedAt.AddDays(kafkaKey.ExpiresIn);
        if (DateTime.UtcNow > expirationDate)
        {
            return (null, new UnauthorizedResult());
        }

        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        if (apiKey is KafkaKey key)
        {
            // TODO: (If necessary) check if the user has access to the topics

            // Compute hash of the encrypted key and check if it exists in the database
            var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
            var iApiKey = await mainDbContext.Set<KafkaKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
            if (iApiKey == null)
            {
                return (null, new UnauthorizedResult());
            }
        }

        if (apiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

        string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";
        var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);

        return (selectedContext, null);
    }

    public async Task<List<KafkaKey>> GetKafkaKeysByUserId(string userId)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        return await dbContext.Set<KafkaKey>().Where(k => k.UserId == userId).ToListAsync();
    }

    public async Task<List<string>> GetKafkaKeyTopics(Guid kafkaKeyID)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var kafkaKey = await dbContext.Set<KafkaKey>().FirstOrDefaultAsync(k => k.Id == kafkaKeyID);

        return kafkaKey?.Topics ?? [];
    }

    public Task<IActionResult> ToggleKafkaKey(Guid apiKeyId, bool isEnabled) => _baseKeyService.ToggleApiKeyEnabledStatus<KafkaKey>(apiKeyId, isEnabled);

    public Task<string> EncryptAndStoreKafkaAccessKey(KafkaKey kafkaKey) => _baseKeyService.EncryptAndStoreAccessKey(kafkaKey);

    public Task<IActionResult> RemoveKafkaAccessKey(string encryptedKey) => _baseKeyService.RemoveAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptKafkaAccessKey(string encryptedKey) => _baseKeyService.DecryptAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptKafkaAccessKeyUserCheck(string encryptedKey, string currentUserId) => _baseKeyService.DecryptAccessKeyUserCheck(encryptedKey, currentUserId);

    public Task<IActionResult> DeleteKafkaKeyById(Guid id) => _baseKeyService.DeleteApiKeyById(id, "kafkakey");

    public async Task<(DbContext?, IActionResult?)> ResolveDbContextAsync(AccessKeyDto? model, HttpContext httpContext)
    {
        DbContext? dbContext;
        if (model == null || string.IsNullOrWhiteSpace(model.EncryptedKey) || model.EncryptedKey == "string")
        {
            var (user, error) = await _userService.GetUserByHttpContextAsync(httpContext);
            if (error != null)
            {
                return (null, error);
            }

            dbContext = await _dbContextService.GetUserDatabaseContext(user);
        }
        else
        {
            (dbContext, IActionResult? errorResult) = await ProcessKafkaAccessKey(model.EncryptedKey, httpContext);
            if (errorResult != null)
            {
                return (null, errorResult);
            }
        }

        if (dbContext == null)
        {
            return (null, new BadRequestObjectResult("Database context is null."));
        }

        return (dbContext, null);
    }
}