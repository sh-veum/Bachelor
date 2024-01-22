using NetBackend.Models.Dto;

namespace NetBackend.Models;

public class ExportRestrictionArea
{
    public int Id { get; set; }
    public long? LocalityNo { get; set; }
    public int? Year { get; set; }
    public int? Week { get; set; }
    public List<LocalityIlaLink>? Localities { get; set; }
}