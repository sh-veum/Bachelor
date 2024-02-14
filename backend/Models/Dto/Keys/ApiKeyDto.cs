namespace NetBackend.Models.Keys.Dto;

public class ApiKeyDto : IApiKeyDto
{
    public List<string>? AccessibleEndpoints { get; set; }
}