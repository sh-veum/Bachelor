namespace NetBackend.Models.ApiKey.Dto;

public class CreateApiKeyDto
{
    public required string KeyName { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}