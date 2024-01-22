namespace NetBackend.Models.History;

public class PdControlAreaLink
{
    public int Id { get; set; }
    public string? ForskNr { get; set; }
    public string? ForskNavn { get; set; }
    public string? ForskLink { get; set; }
    public DateTime? SistEndret { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Version { get; set; }
    public DateTime? OriginalDate { get; set; }
}