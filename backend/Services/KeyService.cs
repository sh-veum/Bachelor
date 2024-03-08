// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using NetBackend.Constants;
// using NetBackend.Models.Keys;
// using NetBackend.Models.User;
// using NetBackend.Services.Interfaces;
// using NetBackend.Tools;

// namespace NetBackend.Services;

// public partial class KeyService : IKeyService
// {
//     private readonly ILogger<KeyService> _logger;
//     private readonly ICryptoService _cryptoService;
//     private readonly IDbContextService _dbContextService;
//     private readonly IHttpContextAccessor _httpContextAccessor;

//     public KeyService(
//         ILogger<KeyService> logger,
//         ICryptoService cryptoService,
//         IDbContextService dbContextService,
//         IHttpContextAccessor httpContextAccessor)
//     {
//         _logger = logger;
//         _cryptoService = cryptoService;
//         _dbContextService = dbContextService;
//         _httpContextAccessor = httpContextAccessor;
//     }

//     public async Task<string> EncryptAndStoreAccessKey(IApiKey iApiKey, UserModel user)
//     {
//         var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
//         var dataToEncrypt = $"Id:{iApiKey.Id},Type:{iApiKey.GetType().Name}";
//         var encryptedKey = _cryptoService.Encrypt(dataToEncrypt, SecretConstants.SecretKey);

//         iApiKey.KeyHash = ComputeHash.ComputeSha256Hash(encryptedKey);

//         dbContext.Entry(iApiKey).State = EntityState.Modified;
//         await dbContext.SaveChangesAsync();

//         return encryptedKey;
//     }

//     public async Task<(IApiKey?, IActionResult?)> DecryptAccessKey(string encryptedKey)
//     {
//         var decryptedData = _cryptoService.Decrypt(encryptedKey, SecretConstants.SecretKey);
//         var dataParts = decryptedData.Split(',');

//         var typePart = dataParts.FirstOrDefault(part => part.StartsWith("Type:"))?.Split(':')[1];
//         var idPart = dataParts.FirstOrDefault(part => part.StartsWith("Id:"))?.Split(':')[1];

//         if (typePart == null || idPart == null || !Guid.TryParse(idPart, out var id))
//         {
//             return (null, new BadRequestObjectResult("Invalid encrypted key format."));
//         }

//         var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
//         return await FetchApiKeyAsync(typePart, id, dbContext);
//     }

//     public async Task<(IApiKey?, IActionResult?)> DecryptAccessKeyUserCheck(string encryptedKey, string currentUserId)
//     {
//         var (apiKey, result) = await DecryptAccessKey(encryptedKey);
//         if (result != null)
//         {
//             return (null, result);
//         }

//         if (apiKey?.UserId != currentUserId)
//         {
//             return (null, new UnauthorizedResult());
//         }

//         return (apiKey, null);
//     }

//     public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessAccessKey(string encryptedKey)
//     {
//         var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
//         if (errorResult != null) return (null, errorResult);

//         if (apiKey == null)
//         {
//             return (null, new BadRequestObjectResult("API key not found."));
//         }

//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         // Compute hash of the encrypted key and check if it exists in the database
//         var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
//         var iApiKey = await mainDbContext.Set<IApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
//         if (iApiKey == null)
//         {
//             return (null, new UnauthorizedResult());
//         }

//         if (!apiKey.IsEnabled) return (null, new BadRequestObjectResult("API key is disabled."));

//         var expirationDate = apiKey.CreatedAt.AddDays(apiKey.ExpiresIn);
//         if (DateTime.UtcNow > expirationDate)
//         {
//             return (null, new BadRequestObjectResult("API key has expired."));
//         }

//         var httpContext = _httpContextAccessor.HttpContext;
//         if (apiKey is ApiKey api)
//         {
//             // Aggregate all accessible endpoints from themes
//             var allAccessibleEndpoints = GetApiKeyThemes(apiKey.Id).Result.SelectMany(t => t.AccessibleEndpoints).ToList();

//             if (!string.IsNullOrEmpty(httpContext?.Request.Path.Value) &&
//                 !allAccessibleEndpoints.Contains(httpContext.Request.Path.Value))
//             {
//                 return (null, new UnauthorizedResult());
//             }
//         }

//         if (apiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

//         string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";
//         var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);

//         return (selectedContext, null);
//     }

//     public async Task<(DbContext? dbContext, IActionResult? actionResult)> ProcessGraphQLAccessKey(string encryptedKey)
//     {
//         var (apiKey, errorResult) = await DecryptAccessKey(encryptedKey);
//         if (errorResult != null) return (null, errorResult);

//         if (apiKey == null)
//         {
//             return (null, new BadRequestObjectResult("API key not found."));
//         }

//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         // Compute hash of the encrypted key and check if it exists in the database
//         var keyHash = ComputeHash.ComputeSha256Hash(encryptedKey);
//         var iApiKey = await mainDbContext.Set<IApiKey>().FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
//         if (iApiKey == null)
//         {
//             return (null, new UnauthorizedResult());
//         }

//         if (!apiKey.IsEnabled) return (null, new BadRequestObjectResult("API key is disabled."));

//         var expirationDate = apiKey.CreatedAt.AddDays(apiKey.ExpiresIn);
//         if (DateTime.UtcNow > expirationDate)
//         {
//             return (null, new UnauthorizedResult());
//         }

//         var httpContext = _httpContextAccessor.HttpContext;
//         if (httpContext == null)
//         {
//             return (null, new BadRequestObjectResult("HttpContext is null."));
//         }

//         // Retrieve the stored GraphQL query from HttpContext
//         var graphqlQuery = httpContext.Items["GraphQLQuery"] as string;

//         if (string.IsNullOrEmpty(graphqlQuery))
//         {
//             return (null, new UnauthorizedResult()); // No query to authorize
//         }

//         if (apiKey is GraphQLApiKey api)
//         {
//             var permission = await GetGraphQLAccessKeyPermissions(apiKey.Id);
//             var isAuthorized = CheckQueryAuthorization(graphqlQuery, permission);
//             if (!isAuthorized)
//             {
//                 return (null, new UnauthorizedResult());
//             }
//         }

//         if (apiKey.UserId == null) return (null, new BadRequestObjectResult("User ID not found in the access key."));

//         string databaseName = mainDbContext.Set<UserModel>().FirstOrDefault(u => u.Id == apiKey.UserId)?.DatabaseName ?? "";
//         var selectedContext = await _dbContextService.GetDatabaseContextByName(databaseName);

//         return (selectedContext, null);
//     }

//     public async Task<IActionResult> RemoveAccessKey(string encryptedKey)
//     {
//         var (apiKey, actionResult) = await DecryptAccessKey(encryptedKey);
//         if (actionResult != null)
//         {
//             return actionResult;
//         }

//         // Remove the Api Key from the database
//         var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
//         if (apiKey != null)
//         {
//             RemoveApiKey(apiKey, dbContext);
//             await dbContext.SaveChangesAsync();
//         }

//         return new OkResult();
//     }

//     public async Task<List<Theme>> GetApiKeyThemes(Guid apiKeyID)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var themes = await mainDbContext.Set<Theme>()
//             .Where(p => p.ApiKeyID == apiKeyID)
//             .ToListAsync();

//         return themes;
//     }

//     public async Task<List<AccessKeyPermission>> GetGraphQLAccessKeyPermissions(Guid graphQLApiKeyId)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var accessKeyPermissions = await mainDbContext.Set<AccessKeyPermission>()
//             .Where(p => p.GraphQLApiKeyId == graphQLApiKeyId)
//             .ToListAsync();

//         return accessKeyPermissions;
//     }

//     private static async Task<(IApiKey?, IActionResult?)> FetchApiKeyAsync(string typePart, Guid id, DbContext dbContext)
//     {
//         if (dbContext == null)
//         {
//             return (null, new NotFoundObjectResult("Database context not found."));
//         }

//         switch (typePart.ToLowerInvariant())
//         {
//             case "apikey":
//                 var apiKey = await dbContext.Set<ApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
//                 return apiKey == null ? (null, new NotFoundObjectResult("Api Key not found.")) : (apiKey, null);
//             case "graphqlapikey":
//                 var graphQLApiKey = await dbContext.Set<GraphQLApiKey>().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
//                 return graphQLApiKey == null ? (null, new NotFoundObjectResult("GraphQL Api Key not found.")) : (graphQLApiKey, null);
//             default:
//                 return (null, new BadRequestObjectResult($"Unknown key type: {typePart}."));
//         }
//     }

//     private static void RemoveApiKey(IApiKey apiKey, DbContext dbContext)
//     {
//         switch (apiKey)
//         {
//             case ApiKey api:
//                 dbContext.Set<ApiKey>().Remove(api);
//                 break;
//             case GraphQLApiKey gqlApi:
//                 dbContext.Set<GraphQLApiKey>().Remove(gqlApi);
//                 break;
//         }
//     }

//     private bool CheckQueryAuthorization(string graphqlQuery, List<AccessKeyPermission> permissions)
//     {
//         _logger.LogInformation("graphqlQuery {graphqlQuery}", graphqlQuery);
//         var parsedQuery = GraphQLQueryParser.ParseQuery(graphqlQuery);

//         if (parsedQuery.Count == 0)
//         {
//             _logger.LogWarning("Parsed query is empty. Authorization check failed.");
//             return false;
//         }

//         _logger.LogInformation("Starting authorization check for GraphQL query.");

//         foreach (var operation in parsedQuery)
//         {
//             var operationName = operation.Key.ToLowerInvariant();
//             var requestedFields = operation.Value.Select(f => f.ToLowerInvariant()).ToList();

//             _logger.LogInformation($"Checking operation: {operation.Key} with fields: {string.Join(", ", operation.Value)}");

//             var permission = permissions.FirstOrDefault(p => p.QueryName.Equals(operationName, StringComparison.OrdinalIgnoreCase));

//             if (permission == null)
//             {
//                 _logger.LogWarning($"Unauthorized operation: {operation.Key}. No matching permission found.");
//                 return false;
//             }

//             var allowedFields = permission.AllowedFields?.Select(f => f.ToLowerInvariant()).ToList() ?? [];

//             _logger.LogInformation($"Allowed fields for operation '{operation.Key}': {string.Join(", ", allowedFields)}");

//             foreach (var field in requestedFields)
//             {
//                 if (!allowedFields.Contains(field))
//                 {
//                     _logger.LogWarning($"Unauthorized field: {field} in operation: {operation.Key}. Field is not in the allowed list.");
//                     return false;
//                 }
//             }
//         }

//         _logger.LogInformation("GraphQL query authorization check passed.");

//         return true; // All requested operations and fields are allowed
//     }

//     public async Task<List<Theme>> GetThemesByUserId(string userId)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var themes = await mainDbContext.Set<Theme>()
//             .Where(p => p.UserId == userId)
//             .ToListAsync();

//         return themes;
//     }

//     public async Task<Theme> CreateTheme(Theme theme)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         mainDbContext.Set<Theme>().Add(theme);
//         await mainDbContext.SaveChangesAsync();

//         return theme;
//     }

//     public async Task<Theme> UpdateTheme(Theme theme)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         mainDbContext.Set<Theme>().Update(theme);
//         await mainDbContext.SaveChangesAsync();

//         return theme;
//     }

//     public async Task<IActionResult> DeleteTheme(Guid themeId)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var theme = await mainDbContext.Set<Theme>().FirstOrDefaultAsync(t => t.Id == Guid.Parse(themeId.ToString()));
//         if (theme == null)
//         {
//             return new NotFoundObjectResult("Theme not found.");
//         }

//         mainDbContext.Set<Theme>().Remove(theme);
//         await mainDbContext.SaveChangesAsync();

//         return new OkResult();
//     }

//     public async Task<List<ApiKey>> GetRestApiKeysByUserId(string userId)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var apiKeys = await mainDbContext.Set<ApiKey>()
//             .Where(p => p.UserId == userId)
//             .ToListAsync();

//         return apiKeys;
//     }

//     public async Task<List<GraphQLApiKey>> GetGraphQLApiKeysByUserId(string userId)
//     {
//         var mainDbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

//         var apiKeys = await mainDbContext.Set<GraphQLApiKey>()
//             .Where(p => p.UserId == userId)
//             .ToListAsync();

//         return apiKeys;
//     }

//     public Task<IActionResult> ToggleApiKey(Guid apiKeyId, bool isEnabled) => ToggleApiKeyEnabledStatus<ApiKey>(apiKeyId, isEnabled);

//     public Task<IActionResult> ToggleGraphQLApiKey(Guid graphQLApiKeyId, bool isEnabled) => ToggleApiKeyEnabledStatus<GraphQLApiKey>(graphQLApiKeyId, isEnabled);

//     private async Task<IActionResult> ToggleApiKeyEnabledStatus<T>(Guid keyId, bool isEnabled) where T : IApiKey
//     {
//         var dbContext = await _dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);
//         var apiKey = await dbContext.Set<T>().FirstOrDefaultAsync(a => a.Id == keyId);

//         if (apiKey == null)
//         {
//             return new NotFoundObjectResult(typeof(T) == typeof(ApiKey) ? "API key not found." : "GraphQLAPI key not found.");
//         }

//         apiKey.IsEnabled = isEnabled;
//         await dbContext.SaveChangesAsync();

//         _logger.LogInformation("{KeyType} with ID {KeyId} has been {Action}.", typeof(T).Name, keyId, isEnabled ? "enabled" : "disabled");

//         return new OkResult();
//     }
// }