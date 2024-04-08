using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Data.DbContexts;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services;

public class DbContextService : IDbContextService
{
    private readonly MainDbContext _mainDbContext;
    private readonly CustomerOneDbContext _customerOneContext;
    private readonly CustomerTwoDbContext _customerTwoContext;


    public DbContextService(
    MainDbContext mainDbContext,
    CustomerOneDbContext customerOneContext,
    CustomerTwoDbContext customerTwoContext)
    {
        _mainDbContext = mainDbContext;
        _customerOneContext = customerOneContext;
        _customerTwoContext = customerTwoContext;

    }

    public Task<DbContext> GetUserDatabaseContext(UserModel user)
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