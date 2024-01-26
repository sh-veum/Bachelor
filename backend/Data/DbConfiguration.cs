using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using NetBackend.Models.ControlAreas;
using NetBackend.Models.History;

namespace NetBackend.Data;

public static class DbConfiguration
{
    public static void ConfigureRelations(ModelBuilder modelBuilder)
    {
        ConfigureDiseaseZoneHistory(modelBuilder);

        ConfigurePdControlArea(modelBuilder);

        ConfigureIlaControlArea(modelBuilder);

        ConfigureExportRestrictionArea(modelBuilder);

        // Configures keys for the joining entities
        ConfigureJoiningEntitiesKeys(modelBuilder);
    }

    private static void ConfigureDiseaseZoneHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.PdSurveilanzeZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.PdProtectionZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.IlaSurveilanzeZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.IlaProtectionZoneLinks)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);

        modelBuilder.Entity<DiseaseZoneHistory>()
            .HasMany(e => e.ExportRestrictionLink)
            .WithOne(l => l.DiseaseZoneHistory)
            .HasForeignKey(l => l.DiseaseZoneHistoryId);
    }

    private static void ConfigurePdControlArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PdControlArea>()
            .HasMany(e => e.PdSurveillanceAreaLinks)
            .WithOne(l => l.PdControlArea)
            .HasForeignKey(l => l.PdControlAreaId);

        modelBuilder.Entity<PdControlArea>()
            .HasMany(e => e.PdProtectionAreaLinks)
            .WithOne(l => l.PdControlArea)
            .HasForeignKey(l => l.PdControlAreaId);
    }

    private static void ConfigureIlaControlArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IlaControlArea>()
            .HasMany(e => e.IlaSurveillanceAreaLinks)
            .WithOne(l => l.IlaControlArea)
            .HasForeignKey(l => l.IlaControlAreaId);

        modelBuilder.Entity<IlaControlArea>()
            .HasMany(e => e.IlaProtectionAreaLinks)
            .WithOne(l => l.IlaControlArea)
            .HasForeignKey(l => l.IlaControlAreaId);
    }

    private static void ConfigureExportRestrictionArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExportRestrictionArea>()
            .HasMany(e => e.ExportRestrictionAreaLinks)
            .WithOne(l => l.ExportRestrictionArea)
            .HasForeignKey(l => l.ExportRestrictionAreaId);
    }

    private static void ConfigureJoiningEntitiesKeys(ModelBuilder modelBuilder)
    {
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
    }
}