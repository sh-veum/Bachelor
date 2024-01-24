using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using NetBackend.Models.Geometry;
using NetBackend.Models.History;
using NetBackend.Models.Dto;
using NetBackend.Models.ControlAreas;

namespace NetBackend.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PdSurveilanzeZoneLinks & PdProtectionZoneLinks
        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.PdSurveilanzeZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.PdProtectionZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<PdControlArea>()
            .HasMany(e => e.PdSurveillanceAreaLinks)
            .WithOne(l => l.PdControlArea)
            .HasForeignKey(l => l.PdControlAreaId);

        modelBuilder.Entity<PdControlArea>()
            .HasMany(e => e.PdProtectionAreaLinks)
            .WithOne(l => l.PdControlArea)
            .HasForeignKey(l => l.PdControlAreaId);

        // IlaSurveilanzeZoneLinks & IlaProtectionZoneLinks
        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.IlaSurveilanzeZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.IlaProtectionZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<IlaControlArea>()
            .HasMany(e => e.IlaSurveillanceAreaLinks)
            .WithOne(l => l.IlaControlArea)
            .HasForeignKey(l => l.IlaControlAreaId);

        modelBuilder.Entity<IlaControlArea>()
            .HasMany(e => e.IlaProtectionAreaLinks)
            .WithOne(l => l.IlaControlArea)
            .HasForeignKey(l => l.IlaControlAreaId);

        // ExportRestrictionLink
        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.ExportRestrictionLink)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<ExportRestrictionArea>()
            .HasMany(e => e.ExportRestrictionAreaLinks)
            .WithOne(l => l.ExportRestrictionArea)
            .HasForeignKey(l => l.ExportRestrictionAreaId);

        // Keys for the joining entities
        modelBuilder.Entity<PdSurveillanceAreaLink>()
            .HasKey(t => new { t.PdControlAreaId, t.DiseaseZoneHistoryId });
        modelBuilder.Entity<PdProtectionAreaLink>()
            .HasKey(t => new { t.PdControlAreaId, t.DiseaseZoneHistoryId });
        modelBuilder.Entity<IlaSurveillanceAreaLink>()
            .HasKey(t => new { t.IlaControlAreaId, t.DiseaseZoneHistoryId });
        modelBuilder.Entity<IlaProtectionAreaLink>()
            .HasKey(t => new { t.IlaControlAreaId, t.DiseaseZoneHistoryId });
        modelBuilder.Entity<ExportRestrictionAreaLink>()
            .HasKey(t => new { t.ExportRestrictionAreaId, t.DiseaseZoneHistoryId });

        // Seed data for Species
        modelBuilder.Entity<Species>().HasData(
            new Species { Id = 1, Name = "Liten kantnål" },
            new Species { Id = 2, Name = "Torsk" },
            new Species { Id = 3, Name = "Sei" },
            new Species { Id = 4, Name = "Laks" },
            new Species { Id = 5, Name = "Ørret" }
        );

        modelBuilder.Entity<DiseaseZoneHistory>().HasData(
            new DiseaseZoneHistory { Id = 1 },
            new DiseaseZoneHistory { Id = 2 },
            new DiseaseZoneHistory { Id = 3 });

        // Seed data for IlaControlAreaLink
        modelBuilder.Entity<IlaControlArea>().HasData(
            new IlaControlArea
            {
                Id = 1,
                ForskNr = "ILA001",
                ForskNavn = "Control Area 1",
                ForskLink = "http://example.com/controlarea1",
                SistEndret = DateTime.SpecifyKind(new DateTime(2023, 01, 15), DateTimeKind.Utc),
                FromDate = DateTime.SpecifyKind(new DateTime(2023, 01, 01), DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(new DateTime(2023, 06, 30), DateTimeKind.Utc),
                OriginalDate = DateTime.SpecifyKind(new DateTime(2022, 12, 31), DateTimeKind.Utc),
                Version = 1,
            },
            new IlaControlArea
            {
                Id = 2,
                ForskNr = "ILA002",
                ForskNavn = "Control Area 2",
                ForskLink = "http://example.com/controlarea2",
                SistEndret = DateTime.SpecifyKind(new DateTime(2023, 01, 15), DateTimeKind.Utc),
                FromDate = DateTime.SpecifyKind(new DateTime(2023, 01, 01), DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(new DateTime(2023, 06, 30), DateTimeKind.Utc),
                OriginalDate = DateTime.SpecifyKind(new DateTime(2022, 12, 31), DateTimeKind.Utc),
                Version = 1,
            });

        // Seed data for PdControlAreaLink
        modelBuilder.Entity<PdControlArea>().HasData(
            new PdControlArea
            {
                Id = 1,
                ForskNr = "001",
                ForskNavn = "Area 1",
                ForskLink = "http://example.com/area1",
                SistEndret = DateTime.SpecifyKind(new DateTime(2023, 01, 15), DateTimeKind.Utc),
                FromDate = DateTime.SpecifyKind(new DateTime(2023, 01, 01), DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(new DateTime(2023, 06, 30), DateTimeKind.Utc),
                OriginalDate = DateTime.SpecifyKind(new DateTime(2022, 12, 31), DateTimeKind.Utc),
                Version = 1,
            },
            new PdControlArea
            {
                Id = 2,
                ForskNr = "002",
                ForskNavn = "Area 2",
                ForskLink = "http://example.com/area2",
                SistEndret = DateTime.SpecifyKind(new DateTime(2023, 01, 15), DateTimeKind.Utc),
                FromDate = DateTime.SpecifyKind(new DateTime(2023, 01, 01), DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(new DateTime(2023, 06, 30), DateTimeKind.Utc),
                OriginalDate = DateTime.SpecifyKind(new DateTime(2022, 12, 31), DateTimeKind.Utc),
                Version = 1,
            });

        // Seed data for ExportRestrictionArea
        modelBuilder.Entity<ExportRestrictionArea>().HasData(
            new ExportRestrictionArea { Id = 1, LocalityNo = 1, Year = 2022, Week = 1 },
            new ExportRestrictionArea { Id = 2, LocalityNo = 2, Year = 2022, Week = 2 });

        // Seed data for linking entities
        modelBuilder.Entity<PdSurveillanceAreaLink>().HasData(
           new PdSurveillanceAreaLink { PdControlAreaId = 1, DiseaseZoneHistoryId = 1 },
           new PdSurveillanceAreaLink { PdControlAreaId = 2, DiseaseZoneHistoryId = 2 }
       );

        modelBuilder.Entity<PdProtectionAreaLink>().HasData(
            new PdProtectionAreaLink { PdControlAreaId = 1, DiseaseZoneHistoryId = 2 },
            new PdProtectionAreaLink { PdControlAreaId = 2, DiseaseZoneHistoryId = 3 }
        );

        modelBuilder.Entity<IlaSurveillanceAreaLink>().HasData(
            new IlaSurveillanceAreaLink { IlaControlAreaId = 1, DiseaseZoneHistoryId = 1 },
            new IlaSurveillanceAreaLink { IlaControlAreaId = 2, DiseaseZoneHistoryId = 2 }
        );

        modelBuilder.Entity<IlaProtectionAreaLink>().HasData(
            new IlaProtectionAreaLink { IlaControlAreaId = 1, DiseaseZoneHistoryId = 2 },
            new IlaProtectionAreaLink { IlaControlAreaId = 2, DiseaseZoneHistoryId = 3 }
        );

        modelBuilder.Entity<ExportRestrictionAreaLink>().HasData(
            new ExportRestrictionAreaLink { ExportRestrictionAreaId = 1, DiseaseZoneHistoryId = 1 },
            new ExportRestrictionAreaLink { ExportRestrictionAreaId = 2, DiseaseZoneHistoryId = 2 }
        );
    }
}
