using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NetBackend.Models.User;
using Microsoft.AspNetCore.Identity;

namespace NetBackend.Data;

public abstract class BaseDbContext(DbContextOptions options) : IdentityDbContext<User, IdentityRole, string>(options)
{
    public DbSet<Species> Species { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }

    // For GraphQL
    public IQueryable<Species> GetSpecies() => Species;
    public IQueryable<Organization> GetOrganizations() => Organizations;

}
