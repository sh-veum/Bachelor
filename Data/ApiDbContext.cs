using Microsoft.EntityFrameworkCore;
using NetBackend.Models;
using NetBackend.Models.Geometry;
using NetBackend.Models.History;
using NetBackend.Models.Dto;

namespace NetBackend.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<Species> Species { get; set; }
    public DbSet<ExportRestrictionArea> ExportRestrictionAreas { get; set; }
    // public DbSet<DiseaseZoneHistory> DiseaseZoneHistories { get; set; }
    // public DbSet<ExportRestrictionAreaLink> ExportRestrictionAreaLinks { get; set; }
    // public DbSet<IlaControlAreaLink> IlaControlAreaLinks { get; set; }
    // public DbSet<PdControlAreaLink> PdControlAreaLinks { get; set; }
    public DbSet<GeoJsonLineString> GeoJsonLineStrings { get; set; }
    public DbSet<GeoJsonPoint> GeoJsonPoints { get; set; }
    public DbSet<GeoJsonPolygon> GeoJsonPolygons { get; set; }
    public DbSet<CodSpawningGroundDto> CodSpawningGroundDtos { get; set; }
    public DbSet<LocalityDto> LocalityDtos { get; set; }
    public DbSet<LocalityIlaLink> LocalityIlaLinks { get; set; }
    public DbSet<OrganizationNameIdDto> OrganizationNameIdDtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data for Species
        modelBuilder.Entity<Species>().HasData(
            new Species { Id = 1, Name = "Liten kantnål" },
            new Species { Id = 2, Name = "Torsk" },
            new Species { Id = 3, Name = "Sei" },
            new Species { Id = 4, Name = "Laks" },
            new Species { Id = 5, Name = "Ørret" }
        );

        // Seed data for ExportRestrictionArea
        modelBuilder.Entity<ExportRestrictionArea>().HasData(
            new ExportRestrictionArea { Id = 1, LocalityNo = 1, Year = 2022, Week = 1 },
            new ExportRestrictionArea { Id = 2, LocalityNo = 2, Year = 2022, Week = 2 });

        // Seed data for DiseaseZoneHistory
        // modelBuilder.Entity<DiseaseZoneHistory>().HasData(
        //     new DiseaseZoneHistory
        //     {
        //         Id = 1,
        //         PdSurveilanzeZones = new List<PdControlAreaLink> { new PdControlAreaLink { Id = 1 } },
        //         PdProtectionZones = new List<PdControlAreaLink> { new PdControlAreaLink { Id = 2 } },
        //         IlaSurveilanzeZones = new List<IlaControlAreaLink> { new IlaControlAreaLink { Id = 1 } },
        //         IlaProtectionZones = new List<IlaControlAreaLink> { new IlaControlAreaLink { Id = 2 } },
        //         ExportRestrictions = new List<ExportRestrictionAreaLink> { new ExportRestrictionAreaLink { LocalityNo = 100 } }
        //     },
        //     new DiseaseZoneHistory
        //     {
        //         Id = 1,
        //         PdSurveilanzeZones = new List<PdControlAreaLink> { new PdControlAreaLink { Id = 2 } },
        //         PdProtectionZones = new List<PdControlAreaLink> { new PdControlAreaLink { Id = 1 } },
        //         IlaSurveilanzeZones = new List<IlaControlAreaLink> { new IlaControlAreaLink { Id = 2 } },
        //         IlaProtectionZones = new List<IlaControlAreaLink> { new IlaControlAreaLink { Id = 1 } },
        //         ExportRestrictions = new List<ExportRestrictionAreaLink> { new ExportRestrictionAreaLink { LocalityNo = 101 } }
        //     });

        // Seed data for ExportRestrictionAreaLink
        modelBuilder.Entity<ExportRestrictionAreaLink>().HasData(
            new ExportRestrictionAreaLink { Id = 1, LocalityNo = 1, Year = 2023, Week = 1 },
            new ExportRestrictionAreaLink { Id = 2, LocalityNo = 2, Year = 2023, Week = 2 });

        // Seed data for IlaControlAreaLink
        // modelBuilder.Entity<IlaControlAreaLink>().HasData(
        //     new IlaControlAreaLink
        //     {
        //         Id = 1,
        //         ForskNr = "ILA001",
        //         ForskNavn = "Control Area 1",
        //         ForskLink = "http://example.com/controlarea1",
        //         SistEndret = new DateTime(2023, 01, 15),
        //         FromDate = new DateTime(2023, 01, 01),
        //         ToDate = new DateTime(2023, 06, 30),
        //         Version = 1,
        //         OriginalDate = new DateTime(2022, 12, 31)
        //     },
        //     new IlaControlAreaLink
        //     {
        //         Id = 2,
        //         ForskNr = "ILA002",
        //         ForskNavn = "Control Area 2",
        //         ForskLink = "http://example.com/controlarea2",
        //         SistEndret = new DateTime(2023, 02, 20),
        //         FromDate = new DateTime(2023, 07, 01),
        //         ToDate = new DateTime(2023, 12, 31),
        //         Version = 1,
        //         OriginalDate = new DateTime(2023, 01, 01)
        //     });

        // Seed data for PdControlAreaLink
        // modelBuilder.Entity<PdControlAreaLink>().HasData(
        //     new PdControlAreaLink
        //     {
        //         Id = 1,
        //         ForskNr = "001",
        //         ForskNavn = "Area 1",
        //         ForskLink = "http://example.com/area1",
        //         SistEndret = new DateTime(2023, 01, 01),
        //         FromDate = new DateTime(2023, 01, 01),
        //         ToDate = new DateTime(2023, 12, 31),
        //         Version = 1,
        //         OriginalDate = new DateTime(2023, 01, 01)
        //     },
        //     new PdControlAreaLink
        //     {
        //         Id = 2,
        //         ForskNr = "002",
        //         ForskNavn = "Area 2",
        //         ForskLink = "http://example.com/area2",
        //         SistEndret = new DateTime(2023, 01, 01),
        //         FromDate = new DateTime(2023, 01, 01),
        //         ToDate = new DateTime(2023, 12, 31),
        //         Version = 1,
        //         OriginalDate = new DateTime(2023, 01, 01)
        //     });

        // Seed data for LocalityIlaLink
        modelBuilder.Entity<LocalityIlaLink>().HasData(
            new LocalityIlaLink
            {
                Id = 1,
                LocalityNo = 1,
                Name = "Locality One",
                IsReportingLocality = true,
                IlaSuspected = false,
                IlaConfirmed = true,
                Position = null
            },
            new LocalityIlaLink
            {
                Id = 2,
                LocalityNo = 2,
                Name = "Locality Two",
                IsReportingLocality = false,
                IlaSuspected = true,
                IlaConfirmed = false,
                Position = null
            });

        // Seed data for CodSpawningGroundDto
        modelBuilder.Entity<CodSpawningGroundDto>().HasData(
            new CodSpawningGroundDto
            {
                Id = 1,
                PlaceName = "Spawning Ground One",
                Information = "Info about Spawning Ground One",
                AreaDescription = "Description of Area One",
                Origin = "Origin One",
                Bmvalue = "BMValue1",
                Value = 100,
                RegisteredDate = DateTime.SpecifyKind(new DateTime(2023, 01, 01), DateTimeKind.Utc),
                Geometry = null
            },
            new CodSpawningGroundDto
            {
                Id = 2,
                PlaceName = "Spawning Ground Two",
                Information = "Info about Spawning Ground Two",
                AreaDescription = "Description of Area Two",
                Origin = "Origin Two",
                Bmvalue = "BMValue2",
                Value = 200,
                RegisteredDate = DateTime.SpecifyKind(new DateTime(2023, 02, 01), DateTimeKind.Utc),
                Geometry = null
            });

        // Seed data for LocalityDto
        modelBuilder.Entity<LocalityDto>().HasData(
            new LocalityDto
            {
                Id = 1,
                LocalityNo = 101,
                LocalityWeekId = 202301,
                Name = "Locality A",
                HasReportedLice = true,
                IsFallow = false,
                AvgAdultFemaleLice = 0.3,
                HasCleanerfishDeployed = true,
                HasMechanicalRemoval = false,
                HasSubstanceTreatments = true,
                HasPd = false,
                HasIla = true,
                MunicipalityNo = "M001",
                Municipality = "Municipality One",
                Lat = 59.914,
                Lon = 10.752,
                IsOnLand = true,
                InFilteredSelection = false,
                HasSalmonoids = true,
                IsSlaughterHoldingCage = false
            },
            new LocalityDto
            {
                Id = 2,
                LocalityNo = 102,
                LocalityWeekId = 202302,
                Name = "Locality B",
                HasReportedLice = false,
                IsFallow = true,
                AvgAdultFemaleLice = 0.7,
                HasCleanerfishDeployed = false,
                HasMechanicalRemoval = true,
                HasSubstanceTreatments = false,
                HasPd = true,
                HasIla = false,
                MunicipalityNo = "M002",
                Municipality = "Municipality Two",
                Lat = 60.391,
                Lon = 5.322,
                IsOnLand = false,
                InFilteredSelection = true,
                HasSalmonoids = false,
                IsSlaughterHoldingCage = true
            });

        // Seed data for OrganizationNameIdDto
        modelBuilder.Entity<OrganizationNameIdDto>().HasData(
            new OrganizationNameIdDto
            {
                Id = 1,
                OrgNo = 1,
                Name = "Organization One"
            },
            new OrganizationNameIdDto
            {
                Id = 2,
                OrgNo = 2,
                Name = "Organization Two"
            });
    }
}
