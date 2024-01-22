using NetBackend.Models.Geometry;

namespace NetBackend.Models.Dto;

public class LocalityIlaLink
{
    public int Id { get; set; }
    public int? LocalityNo { get; set; }
    public string? Name { get; set; }
    public bool IsReportingLocality { get; set; }
    public bool IlaSuspected { get; set; }
    public bool IlaConfirmed { get; set; }
    public GeoJsonGeometry? Position { get; set; }
}