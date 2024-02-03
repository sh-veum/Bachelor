using Microsoft.EntityFrameworkCore;

namespace NetBackend.Data;

public class CustomerOneDbContext : BaseDbContext
{

    public CustomerOneDbContext(DbContextOptions<CustomerOneDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed dummy data
        ModelSeedData.SeedAltOne(modelBuilder);
    }
}
