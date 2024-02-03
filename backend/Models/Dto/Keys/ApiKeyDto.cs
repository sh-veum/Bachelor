namespace NetBackend.Models.Keys.Dto;

public class ApiKeyDto
{
    public int Id { get; set; }
    public required string KeyName { get; set; }
    public required string CreatedBy { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}