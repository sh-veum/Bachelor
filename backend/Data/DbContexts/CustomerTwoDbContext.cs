using Microsoft.EntityFrameworkCore;

namespace NetBackend.Data.DbContexts;

public class CustomerTwoDbContext : BaseDbContext
{

    public CustomerTwoDbContext(DbContextOptions<CustomerTwoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed dummy data
        ModelSeedData.SeedAltTwo(modelBuilder);
    }
}
