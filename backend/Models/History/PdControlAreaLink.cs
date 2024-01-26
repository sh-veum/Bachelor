using NetBackend.Models.ControlAreas;

namespace NetBackend.Models.History;

public class PdSurveillanceAreaLink
{
    public int PdControlAreaId { get; set; }
    public PdControlArea? PdControlArea { get; set; }
    public int DiseaseZoneHistoryId { get; set; }
    public DiseaseZoneHistory? DiseaseZoneHistory { get; set; }
}

public class PdProtectionAreaLink
{
    public int PdControlAreaId { get; set; }
    public PdControlArea? PdControlArea { get; set; }
    public int DiseaseZoneHistoryId { get; set; }
    public DiseaseZoneHistory? DiseaseZoneHistory { get; set; }
}