using Microsoft.EntityFrameworkCore;

namespace NetBackend.Data;

public class MainDbContext : BaseDbContext
{

    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
    {
    }
}
