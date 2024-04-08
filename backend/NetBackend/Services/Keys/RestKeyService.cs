using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class RestKeyService : IRestKeyService
{
    private readonly ILogger<RestKeyService> _logger;
    private readonly IDbContextService _dbContextService;
    private readonly IBaseKeyService _baseKeyService;
    private readonly IUserService _userService;

    public RestKeyService(
        ILogger<RestKeyService> logger,
        IDbContextService dbContextService,
        IBaseKeyService baseKeyService,
        IUserService userService)
    {
        _logger = logger;
        _dbContextService = dbContextService;
        _baseKeyService = baseKeyService;
        _userService = userService;
    }
    public async Task<RestApiKey> CreateRESTApiKey(UserModel user, string keyName, List<Guid> themeIds)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var apiKey = new RestApiKey
        {
            UserId = user.Id,
            KeyName = keyName,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = KeyConstants.ExpiresIn,
            Themes = [],
            IsEnabled = true
        };

        foreach (var themeId in themeIds)
        {
            var theme = await dbContext.Set<Theme>().FirstOrDefaultAsync(t => t.Id == themeId);

            if (theme != null)
            {
                apiKey.Themes.Add(theme);
            }
        }

        dbContext.Set<RestApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }

    public async Task<(IActionResult?, RestApiKey?)> ValidateRestAccessKey(string encryptedKey)
    {
        var (apiKey, errorResult) = await _baseKeyService.DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (errorResult, null);

        if (apiKey is not RestApiKey restApiKey || !restApiKey.IsEnabled)
        {
            return (new BadRequestObjectResult("API key is not found or disabled."), null);
        }

        var expirationDate = restApiKey.CreatedAt.AddDays(restApiKey.ExpiresIn);
        if (DateTime.UtcNow > expirationDate)
        {
            return (new UnauthorizedResult(), null);
        }

        var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
        var iApiKey = await mainDbContext.Set<RestApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
        if (iApiKey == null)
        {
            return (new UnauthorizedResult(), null);
        }

        if (apiKey.UserId == null) return (new BadRequestObjectResult("User ID not found in the access key."), null);

        return (null, restApiKey);
    }

    public async Task<(DbContext? dbContext, IActionResult? actionResult, string? userId)> ProcessAndGetDbContextAndUserIdFromKey(string encryptedKey, HttpContext httpContext)
    {
        var (actionResult, restApiKey) = await ValidateRestAccessKey(encryptedKey);
        if (actionResult != null || restApiKey == null)
        {
            return (null, actionResult, null);
        }

        if (restApiKey is RestApiKey api)
        {
            // Aggregate all accessible endpoints from themes
            var allAccessibleEndpoints = GetRESTApiKeyThemes(restApiKey.Id).Result.SelectMany(t => t.AccessibleEndpoints).ToList();

            if (!string.IsNullOrEmpty(httpContext?.Request.Path.Value) &&
                !allAccessibleEndpoints.Contains(httpContext.Request.Path.Value))
            {
                return (null, new UnauthorizedResult(), null);
            }

            // Compute hash of the encrypted key and check if it exists in the database
            var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
            var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
            var iApiKey = await mainDbContext.Set<RestApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
            if (iApiKey == null)
            {
                return (null, new UnauthorizedResult(), null);
            }
        }

        if (restApiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."), null);

        string databaseName = (await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName))
                              .Set<UserModel>().FirstOrDefault(u => u.Id == restApiKey.UserId)?.DatabaseName ?? "";

        var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);
        return (selectedContext, null, restApiKey.UserId);
    }

    public async Task<List<RestApiKey>> GetRestApiKeysByUserId(string userId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var apiKeys = await mainDbContext.Set<RestApiKey>()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return apiKeys;
    }

    public async Task<List<Theme>> GetRESTApiKeyThemes(Guid restApiKeyID)
    {
        var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        // First, get all Theme IDs associated with the RestApiKey
        var themeIds = await dbContext.Set<RestApiKey>()
            .Where(ak => ak.Id == restApiKeyID)
            .SelectMany(ak => ak.Themes.Select(t => t.Id))
            .ToListAsync();

        // Then, fetch the themes by their IDs
        var themes = await dbContext.Set<Theme>()
            .Where(t => themeIds.Contains(t.Id))
            .ToListAsync();

        return themes;
    }

    public async Task<List<Theme>> GetThemesByUserId(string userId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var themes = await mainDbContext.Set<Theme>()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return themes;
    }

    public async Task<Theme> CreateTheme(Theme theme)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        mainDbContext.Set<Theme>().Add(theme);
        await mainDbContext.SaveChangesAsync();

        return theme;
    }

    public async Task<Theme> UpdateTheme(Theme theme)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        mainDbContext.Set<Theme>().Update(theme);
        await mainDbContext.SaveChangesAsync();

        return theme;
    }

    public async Task<IActionResult> DeleteTheme(Guid themeId)
    {
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var theme = await mainDbContext.Set<Theme>().FirstOrDefaultAsync(t => t.Id == Guid.Parse(themeId.ToString()));
        if (theme == null)
        {
            return new NotFoundObjectResult("Theme not found.");
        }

        mainDbContext.Set<Theme>().Remove(theme);
        await mainDbContext.SaveChangesAsync();

        return new OkResult();
    }

    public async Task<(DbContext?, IActionResult?, string? userId)> ResolveDbContextAndUserId(AccessKeyDto? model, HttpContext httpContext)
    {
        DbContext? dbContext;
        string? userId;
        if (model == null || string.IsNullOrWhiteSpace(model.EncryptedKey) || model.EncryptedKey == "string")
        {
            var (user, error) = await _userService.GetUserByHttpContextAsync(httpContext);
            if (error != null)
            {
                return (null, error, null);
            }

            userId = user.Id;
            dbContext = await _dbContextService.GetUserDatabaseContext(user);
        }
        else
        {
            (dbContext, IActionResult? errorResult, userId) = await ProcessAndGetDbContextAndUserIdFromKey(model.EncryptedKey, httpContext);
            if (errorResult != null)
            {
                return (null, errorResult, null);
            }
        }

        if (dbContext == null)
        {
            return (null, new BadRequestObjectResult("Database context is null."), null);
        }

        return (dbContext, null, userId);
    }

    public Task<IActionResult> ToggleRestApiKey(Guid apiKeyId, bool isEnabled) => _baseKeyService.ToggleApiKeyEnabledStatus<RestApiKey>(apiKeyId, isEnabled);

    public Task<string> EncryptAndStoreRestAccessKey(RestApiKey restApiKey) => _baseKeyService.EncryptAndStoreAccessKey(restApiKey);

    public Task<IActionResult> RemoveRestAccessKey(string encryptedKey) => _baseKeyService.RemoveAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptRestAccessKey(string encryptedKey) => _baseKeyService.DecryptAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptRestAccessKeyUserCheck(string encryptedKey, string currentUserId) => _baseKeyService.DecryptAccessKeyUserCheck(encryptedKey, currentUserId);
    // TODO: change this too if you decide to use a constant
    public Task<IActionResult> DeleteRestApiKeyById(Guid id) => _baseKeyService.DeleteApiKeyById(id, "restapikey");
}