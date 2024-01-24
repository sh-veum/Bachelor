using NetBackend.Models.ControlAreas;

namespace NetBackend.Models.History;

public class IlaSurveillanceAreaLink
{
    public int IlaControlAreaId { get; set; }
    public IlaControlArea? IlaControlArea { get; set; }
    public int DiseaseZoneHistoryId { get; set; }
    public DiseaseZoneHistory? DiseaseZoneHistory { get; set; }
}

public class IlaProtectionAreaLink
{
    public int IlaControlAreaId { get; set; }
    public IlaControlArea? IlaControlArea { get; set; }
    public int DiseaseZoneHistoryId { get; set; }
    public DiseaseZoneHistory? DiseaseZoneHistory { get; set; }
}