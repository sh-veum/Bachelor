using NetBackend.Models.History;

namespace NetBackend.Models;

public class DiseaseZoneHistory
{
    public int Id { get; set; }
    public List<PdControlAreaLink>? PdSurveilanzeZones { get; set; }
    public List<PdControlAreaLink>? PdProtectionZones { get; set; }
    public List<IlaControlAreaLink>? IlaSurveilanzeZones { get; set; }
    public List<IlaControlAreaLink>? IlaProtectionZones { get; set; }
    public List<ExportRestrictionAreaLink>? ExportRestrictions { get; set; }
    public required List<DiseaseZoneHistoryPdControlAreaLink> PdSurveilanzeZoneLinks { get; set; }
}