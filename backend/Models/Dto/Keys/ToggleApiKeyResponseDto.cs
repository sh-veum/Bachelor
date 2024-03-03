namespace NetBackend.Models.Dto.Keys;

// Needed for GraphQL, I think. Not sure.
public class ToggleApiKeyResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}