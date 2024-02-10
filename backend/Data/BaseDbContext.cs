using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NetBackend.Models.User;
using Microsoft.AspNetCore.Identity;

namespace NetBackend.Data;

public abstract class BaseDbContext : IdentityDbContext<User, IdentityRole, string>
{
    public DbSet<Species> Species { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }

    public BaseDbContext(DbContextOptions options) : base(options)
    {
    }
}
