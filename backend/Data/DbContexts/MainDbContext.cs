using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetBackend.Models.User;

namespace NetBackend.Data.DbContexts;

public class MainDbContext : IdentityDbContext<User>
{
    public DbSet<ApiKey> Keys { get; set; }

    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ApiKey)
            .WithOne(k => k.User)
            .HasForeignKey(k => k.UserId);
    }
}
