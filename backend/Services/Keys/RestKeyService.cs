using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Tools;

namespace NetBackend.Services.Keys;

public class RestKeyService : BaseKeyService, IRestKeyService
{
    public RestKeyService(
        ILogger<RestKeyService> logger,
        IDbContextService dbContextService,
        ICryptoService cryptoService,
        IHttpContextAccessor httpContextAccessor)
        : base(logger, dbContextService, cryptoService, httpContextAccessor)
    {
    }

    public async Task<RestApiKey> CreateRESTApiKey(UserModel user, string keyName, List<ThemeDto> themeDtos)
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

        foreach (var themeDto in themeDtos)
        {
            var theme = new Theme
            {
                AccessibleEndpoints = themeDto.AccessibleEndpoints,
                ThemeName = themeDto.ThemeName,
                RestApiKeyID = apiKey.Id,
                RestApiKey = apiKey,
                UserId = user.Id,
                User = user
            };

            apiKey.Themes.Add(theme);
        }

        dbContext.Set<RestApiKey>().Add(apiKey);
        await dbContext.SaveChangesAsync();

        return apiKey;
    }

    public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessRESTAccessKey(string encryptedKey)
    {
        var (restApiKey, errorResult) = await DecryptAccessKey(encryptedKey);
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

        var httpContext = _httpContextAccessor.HttpContext;
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
        var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

        var themes = await mainDbContext.Set<Theme>()
            .Where(p => p.RestApiKeyID == restApiKeyID)
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

    public Task<IActionResult> ToggleRestApiKey(Guid apiKeyId, bool isEnabled) => ToggleApiKeyEnabledStatus<RestApiKey>(apiKeyId, isEnabled);
}