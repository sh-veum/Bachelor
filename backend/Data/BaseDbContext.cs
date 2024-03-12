using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NetBackend.Models.User;
using Microsoft.AspNetCore.Identity;
using NetBackend.Models.Keys;

namespace NetBackend.Data;

public abstract class BaseDbContext(DbContextOptions options) : IdentityDbContext<UserModel, IdentityRole, string>(options)
{
    public DbSet<Species> Species { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<WaterQualityLog> WaterQualityLogs { get; set; }

    // For GraphQL
    public IQueryable<Species> GetSpecies() => Species;
    public IQueryable<Organization> GetOrganizations() => Organizations;
    public IQueryable<WaterQualityLog> GetWaterQualityLogs() => WaterQualityLogs;
}
