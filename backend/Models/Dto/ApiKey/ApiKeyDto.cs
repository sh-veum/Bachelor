namespace NetBackend.Models.ApiKey.Dto;

public class ApiKeyDto
{
    public int Id { get; set; }
    public required string KeyName { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}