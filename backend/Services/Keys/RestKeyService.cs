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
            Themes = new List<Theme>(),
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

    public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessRESTAccessKey(string encryptedKey, HttpContext httpContext)
    {
        var (restApiKey, errorResult) = await _baseKeyService.DecryptAccessKey(encryptedKey);
        if (errorResult != null) return (null, errorResult);

        if (restApiKey == null)
        {
            return (null, new BadRequestObjectResult("API key not found."));
        }

        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        if (!restApiKey.IsEnabled) return (null, new BadRequestObjectResult("API key is disabled."));

        var expirationDate = restApiKey.CreatedAt.AddDays(restApiKey.ExpiresIn);
        if (DateTime.UtcNow > expirationDate)
        {
            return (null, new BadRequestObjectResult("API key has expired."));
        }

        if (restApiKey is RestApiKey api)
        {
            // Aggregate all accessible endpoints from themes
            var allAccessibleEndpoints = GetRESTApiKeyThemes(restApiKey.Id).Result.SelectMany(t => t.AccessibleEndpoints).ToList();

            if (!string.IsNullOrEmpty(httpContext?.Request.Path.Value) &&
                !allAccessibleEndpoints.Contains(httpContext.Request.Path.Value))
            {
                return (null, new UnauthorizedResult());
            }

            // Compute hash of the encrypted key and check if it exists in the database
            var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
            var iApiKey = await mainDbContext.Set<RestApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
            if (iApiKey == null)
            {
                return (null, new UnauthorizedResult());
            }
        }

        if (restApiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

        string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == restApiKey.UserId)?.DatabaseName ?? "";
        var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);

        return (selectedContext, null);
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
        var themeIds = await dbContext.Set<RestApiKey>() // Adjust this line if you have a different way to access your DbSet
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

    public async Task<(DbContext?, IActionResult?)> ResolveDbContextAsync(AccessKeyDto? model, HttpContext httpContext)
    {
        DbContext? dbContext = null;

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
            (dbContext, IActionResult? errorResult) = await ProcessRESTAccessKey(model.EncryptedKey, httpContext);
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

    public Task<IActionResult> ToggleRestApiKey(Guid apiKeyId, bool isEnabled) => _baseKeyService.ToggleApiKeyEnabledStatus<RestApiKey>(apiKeyId, isEnabled);

    public Task<string> EncryptAndStoreRestAccessKey(RestApiKey restApiKey) => _baseKeyService.EncryptAndStoreAccessKey(restApiKey);

    public Task<IActionResult> RemoveRestAccessKey(string encryptedKey) => _baseKeyService.RemoveAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptRestAccessKey(string encryptedKey) => _baseKeyService.DecryptAccessKey(encryptedKey);

    public Task<(IApiKey?, IActionResult?)> DecryptRestAccessKeyUserCheck(string encryptedKey, string currentUserId) => _baseKeyService.DecryptAccessKeyUserCheck(encryptedKey, currentUserId);
    // TODO: change this too if you decide to use a constant
    public Task<IActionResult> DeleteRestApiKeyById(Guid id) => _baseKeyService.DeleteApiKeyById(id, "restapikey");
}