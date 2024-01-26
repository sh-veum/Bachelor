using NetBackend.Models.ControlAreas;

namespace NetBackend.Models.History;

public class ExportRestrictionAreaLink
{
    public int ExportRestrictionAreaId { get; set; }
    public ExportRestrictionArea? ExportRestrictionArea { get; set; }
    public int DiseaseZoneHistoryId { get; set; }
    public DiseaseZoneHistory? DiseaseZoneHistory { get; set; }
}