using Microsoft.EntityFrameworkCore;

namespace NetBackend.Data;

public class CustomerTwoDbContext : BaseDbContext
{

    public CustomerTwoDbContext(DbContextOptions<CustomerTwoDbContext> options) : base(options)
    {
    }

}
