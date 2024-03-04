using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetBackend.Models.Keys;
using NetBackend.Models.User;

namespace NetBackend.Data.DbContexts;

public class MainDbContext : IdentityDbContext<UserModel, IdentityRole, string>
{
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<GraphQLApiKey> GraphQLApiKeys { get; set; }
    public DbSet<AccessKeyPermission> AccessKeyPermissions { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }

    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>()
            .HasMany(u => u.ApiKey)
            .WithOne(k => k.User)
            .HasForeignKey(k => k.UserId);

        modelBuilder.Entity<UserModel>()
            .HasMany(u => u.GraphQLApiKey)
            .WithOne(k => k.User)
            .HasForeignKey(k => k.UserId);

        modelBuilder.Entity<GraphQLApiKey>()
            .HasMany(u => u.AccessKeyPermissions)
            .WithOne(k => k.GraphQLApiKey)
            .HasForeignKey(k => k.GraphQLApiKeyId);

        modelBuilder.Entity<ApiKey>()
            .HasMany(u => u.Themes)
            .WithOne(k => k.ApiKey)
            .HasForeignKey(k => k.ApiKeyID)
            .IsRequired(false);

        modelBuilder.Entity<UserModel>()
            .HasMany(u => u.Themes)
            .WithOne(k => k.User)
            .HasForeignKey(k => k.UserId);

        modelBuilder.Entity<ApiKey>()
            .HasOne(a => a.AccessKey)
            .WithOne(ak => ak.ApiKey)
            .HasForeignKey<AccessKey>(ak => ak.ApiKeyId)
            .IsRequired(false);

        modelBuilder.Entity<GraphQLApiKey>()
            .HasOne(a => a.AccessKey)
            .WithOne(ak => ak.GraphQLApiKey)
            .HasForeignKey<AccessKey>(ak => ak.GraphQLApiKeyId)
            .IsRequired(false);
    }
}
