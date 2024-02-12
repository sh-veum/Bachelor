using HotChocolate;
using NetBackend.Data;
using NetBackend.Data.DbContexts;
using NetBackend.Models;
using NetBackend.Services;

namespace NetBackend.GraphQL;

public class Query
{
    public IQueryable<Species>? GetSpecies([Service] IDatabaseContextService dbContextService, string dbContextName)
    {
        var dbContext = dbContextService.GetDatabaseContextByName(dbContextName).Result as BaseDbContext;
        return dbContext?.GetSpecies();
    }

    public IQueryable<Organization>? GetOrganizations([Service] IDatabaseContextService dbContextService, string dbContextName)
    {
        var dbContext = dbContextService.GetDatabaseContextByName(dbContextName).Result as BaseDbContext;
        return dbContext?.GetOrganizations();
    }
}
