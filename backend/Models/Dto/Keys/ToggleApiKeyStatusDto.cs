namespace NetBackend.Models.Dto.Keys;

public class ToggleApiKeyStatusDto
{
    public Guid Id { get; set; }
    public required bool IsEnabled { get; set; }
    public required string KeyType { get; set; } // Rest, GraphQL
}