using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using NetBackend.Models.History;
using NetBackend.Models.ControlAreas;
using NetBackend.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NetBackend.Data;

public class ApiDbContext : IdentityDbContext<User>
{
    public DbSet<Species> Species { get; set; }
    public DbSet<DiseaseZoneHistory> DiseaseZoneHistories { get; set; }
    public DbSet<ExportRestrictionArea> ExportRestrictionAreas { get; set; }
    public DbSet<IlaControlArea> IlaControlAreas { get; set; }
    public DbSet<PdControlArea> PdControlAreas { get; set; }

    // Links
    public DbSet<ExportRestrictionAreaLink> ExportRestrictionAreaLinks { get; set; }
    public DbSet<IlaSurveillanceAreaLink> IlaSurveillanceAreaLinks { get; set; }
    public DbSet<IlaProtectionAreaLink> IlaProtectionAreaLinks { get; set; }
    public DbSet<PdSurveillanceAreaLink> PdSurveillanceAreaLinks { get; set; }
    public DbSet<PdProtectionAreaLink> PdProtectionAreaLinks { get; set; }

    // Geo Placeholders
    public DbSet<GeoJsonLineString> GeoJsonLineStrings { get; set; }
    public DbSet<GeoJsonPoint> GeoJsonPoints { get; set; }
    public DbSet<GeoJsonPolygon> GeoJsonPolygons { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure foreign keys
        DbConfiguration.ConfigureRelations(modelBuilder);

        // Seed dummy data
        ModelSeedData.Seed(modelBuilder);
    }
}
