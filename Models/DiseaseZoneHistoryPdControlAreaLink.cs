using NetBackend.Models.History;

namespace NetBackend.Models;

public class DiseaseZoneHistoryPdControlAreaLink
{
    public int DiseaseZoneHistoryId { get; set; }
    public required DiseaseZoneHistory DiseaseZoneHistory { get; set; }

    public int PdControlAreaLinkId { get; set; }
    public required PdControlAreaLink PdControlAreaLink { get; set; }
}