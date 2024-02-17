using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;

namespace NetBackend.Data;

public static class ModelSeedData
{

    public static void SeedAltOne(ModelBuilder modelBuilder)
    {
        // Seed data for Species
        modelBuilder.Entity<Species>().HasData(
            new Species { Id = 1, Name = "Liten kantnål" },
            new Species { Id = 2, Name = "Torsk" },
            new Species { Id = 3, Name = "Sei" },
            new Species { Id = 4, Name = "Laks" },
            new Species { Id = 5, Name = "Ørret" }
        );

        modelBuilder.Entity<Organization>().HasData(
            new Organization { Id = 1, OrgNo = 101, Name = "Firma AS", Address = "Gateveien 1", PostalCode = "1234", City = "Oslo" },
            new Organization { Id = 2, OrgNo = 102, Name = "Firma 2 AS", Address = "Gateveien 2", PostalCode = "4321", City = "Oslo" },
            new Organization { Id = 3, OrgNo = 103, Name = "Firma 3 AS", Address = "Gateveien 3", PostalCode = "5678", City = "Oslo" },
            new Organization { Id = 4, OrgNo = 104, Name = "Firma 4 AS", Address = "Gateveien 4", PostalCode = "8765", City = "Oslo" }
        );
    }

    public static void SeedAltTwo(ModelBuilder modelBuilder)
    {
        // Seed data for Species
        modelBuilder.Entity<Species>().HasData(
            new Species { Id = 1, Name = "Liten kantnål" },
            new Species { Id = 2, Name = "Torsk" },
            new Species { Id = 3, Name = "Sei" },
            new Species { Id = 4, Name = "Laks" },
            new Species { Id = 5, Name = "Ørret" },
            new Species { Id = 6, Name = "Makrell" },
            new Species { Id = 7, Name = "Sild" },
            new Species { Id = 8, Name = "Kveite" },
            new Species { Id = 9, Name = "Blåkveite" },
            new Species { Id = 10, Name = "Hyse" }
        );

        modelBuilder.Entity<Organization>().HasData(
            new Organization { Id = 1, OrgNo = 199, Name = "Kompni AS", Address = "Storvegen 303", PostalCode = "6000", City = "Ålesund" },
            new Organization { Id = 2, OrgNo = 200, Name = "Kompni 2 AS", Address = "Storvegen 304", PostalCode = "6001", City = "Ålesund" },
            new Organization { Id = 3, OrgNo = 201, Name = "Kompni 3 AS", Address = "Storvegen 305", PostalCode = "6002", City = "Ålesund" }
        );
    }

    public static void SeedAltThree(ModelBuilder modelBuilder, int speciesCount, int organizationCount, string identifierName)
    {
        // Seed data for Species
        var speciesList = new List<Species>();
        for (int i = 1; i <= speciesCount; i++)
        {
            speciesList.Add(new Species
            {
                Id = i + 100,
                Name = $"{identifierName}: Species {i}",
                SuperSecretNumber = i
            });
        }
        modelBuilder.Entity<Species>().HasData(speciesList);

        // Seed data for Organizations
        var organizationList = new List<Organization>();
        for (int i = 1; i <= organizationCount; i++)
        {
            organizationList.Add(new Organization { Id = i + 100, OrgNo = 100 + i, Name = $"{identifierName}: Organization {i}", Address = $"Address {i}", PostalCode = $"PostalCode {i}", City = $"City {i}" });
        }
        modelBuilder.Entity<Organization>().HasData(organizationList);
    }
}