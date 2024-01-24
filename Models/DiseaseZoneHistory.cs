using NetBackend.Models.ControlAreas;
using NetBackend.Models.History;

namespace NetBackend.Models;

public class DiseaseZoneHistory
{
    public int Id { get; set; }
    public List<PdSurveillanceAreaLink>? PdSurveilanzeZoneLinks { get; set; } = [];
    public List<PdProtectionAreaLink>? PdProtectionZoneLinks { get; set; } = [];
    public List<IlaSurveillanceAreaLink>? IlaSurveilanzeZoneLinks { get; set; } = [];
    public List<IlaProtectionAreaLink>? IlaProtectionZoneLinks { get; set; } = [];
    public List<ExportRestrictionAreaLink>? ExportRestrictionLink { get; set; } = [];
}