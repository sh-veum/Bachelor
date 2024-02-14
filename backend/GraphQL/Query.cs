using NetBackend.Data;
using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Services;

namespace NetBackend.GraphQL;

public class Query
{
    public async Task<IQueryable<Species>?> GetSpecies([Service] IKeyService keyService, string encryptedKey)
    {
        var (dbContext, errorResult) = await keyService.ProcessAccessKey(encryptedKey, "GetSpecies");

        if (errorResult != null || dbContext == null || dbContext is not BaseDbContext baseDbContext)
        {
            return null;
        }

        return baseDbContext.GetSpecies();
    }

    public async Task<IQueryable<Organization>?> GetOrganizations([Service] IKeyService keyService, string encryptedKey)
    {
        var (dbContext, errorResult) = await keyService.ProcessAccessKey(encryptedKey, "GetOrganizations");

        if (errorResult != null || dbContext == null || dbContext is not BaseDbContext baseDbContext)
        {
            return null;
        }

        return baseDbContext.GetOrganizations();
    }
}
