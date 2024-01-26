using NetBackend.Models.History;

namespace NetBackend.Models.ControlAreas;

public class ExportRestrictionArea
{
    public int Id { get; set; }
    public int LocalityNo { get; set; }
    public int Year { get; set; }
    public int Week { get; set; }
    // public List<LocalityIlaLink> Localities { get; set; }

    public List<ExportRestrictionAreaLink> ExportRestrictionAreaLinks { get; set; } = new List<ExportRestrictionAreaLink>();
}