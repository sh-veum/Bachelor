using Microsoft.EntityFrameworkCore;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IDbContextService
{
    Task<DbContext> GetUserDatabaseContext(UserModel user);
    Task<DbContext> GetDatabaseContextByName(string databaseName);
}