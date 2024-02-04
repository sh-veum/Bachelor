using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Data.DbContexts;
using NetBackend.Models.User;

namespace NetBackend.Services;

public interface IDatabaseContextService
{
    Task<DbContext> GetUserDatabaseContext(User user);
    Task<DbContext> GetDatabaseContextByName(string databaseName);
}

public class DatabaseContextService : IDatabaseContextService
{
    private readonly MainDbContext _mainDbContext;
    private readonly CustomerOneDbContext _customerOneContext;
    private readonly CustomerTwoDbContext _customerTwoContext;


    public DatabaseContextService(
    MainDbContext mainDbContext,
    CustomerOneDbContext customerOneContext,
    CustomerTwoDbContext customerTwoContext)
    {
        _mainDbContext = mainDbContext;
        _customerOneContext = customerOneContext;
        _customerTwoContext = customerTwoContext;

    }

    public Task<DbContext> GetUserDatabaseContext(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        DbContext context = user.DatabaseName switch
        {
            DatabaseConstants.MainDbName => _mainDbContext,
            DatabaseConstants.CustomerOneDbName => _customerOneContext,
            DatabaseConstants.CustomerTwoDbName => _customerTwoContext,
            _ => throw new ArgumentException("Database not found or not assigned to user.")
        };

        return Task.FromResult(context);
    }

    public Task<DbContext> GetDatabaseContextByName(string databaseName)
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentNullException(nameof(databaseName), "Database name cannot be null or empty.");
        }

        DbContext context = databaseName switch
        {
            DatabaseConstants.MainDbName => _mainDbContext,
            DatabaseConstants.CustomerOneDbName => _customerOneContext,
            DatabaseConstants.CustomerTwoDbName => _customerTwoContext,
            _ => throw new ArgumentException("Database not found or not assigned.")
        };

        return Task.FromResult(context);
    }
}