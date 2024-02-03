using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NetBackend.Models.User;

namespace NetBackend.Data;

public abstract class BaseDbContext : IdentityDbContext<User>
{
    public DbSet<Species> Species { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }

    public BaseDbContext(DbContextOptions options) : base(options)
    {
    }
}
