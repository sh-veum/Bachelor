using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netbackend.Models.Dto.Keys;
using NetBackend.Data;
using NetBackend.Models.User;
using NetBackend.Services;

namespace Netbackend.Services;

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
            "Main" => _mainDbContext,
            "Customer1" => _customerOneContext,
            "Customer2" => _customerTwoContext,
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
            "Main" => _mainDbContext,
            "Customer1" => _customerOneContext,
            "Customer2" => _customerTwoContext,
            _ => throw new ArgumentException("Database not found or not assigned.")
        };

        return Task.FromResult(context);
    }
}