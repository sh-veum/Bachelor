namespace NetBackend.Models.Dto;

public class LocalityDto
{
    public int Id { get; set; }
    public long? LocalityNo { get; set; }
    public long? LocalityWeekId { get; set; }
    public string? Name { get; set; }
    public bool? HasReportedLice { get; set; }
    public bool? IsFallow { get; set; }
    public double? AvgAdultFemaleLice { get; set; }
    public bool? HasCleanerfishDeployed { get; set; }
    public bool? HasMechanicalRemoval { get; set; }
    public bool? HasSubstanceTreatments { get; set; }
    public bool? HasPd { get; set; }
    public bool? HasIla { get; set; }
    public string? MunicipalityNo { get; set; }
    public string? Municipality { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public bool? IsOnLand { get; set; }
    public bool? InFilteredSelection { get; set; }
    public bool? HasSalmonoids { get; set; }
    public bool? IsSlaughterHoldingCage { get; set; }
}