using Microsoft.EntityFrameworkCore;
using NetBackend.Models;

namespace NetBackend.Data;

public class ApiDbContext : DbContext
{
  public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
  {
  }

  public DbSet<Species> Species { get; set; }
}
