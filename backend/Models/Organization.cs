namespace NetBackend.Models;

public class Organization
{
    public int Id { get; set; }
    public int? OrgNo { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
}
