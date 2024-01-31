using Microsoft.EntityFrameworkCore;

namespace NetBackend.Data;

public class CustomerOneDbContext : BaseDbContext
{

    public CustomerOneDbContext(DbContextOptions<CustomerOneDbContext> options) : base(options)
    {
    }

}
