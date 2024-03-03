namespace NetBackend.Models.Dto.Keys;

public class ToggleApiKeyStatusDto
{
    public int Id { get; set; }
    public required string KeyType { get; set; } // "REST" or "GraphQL"
    public required bool IsEnabled { get; set; }
}