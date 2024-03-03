namespace NetBackend.Models.Dto.Keys;

public class IApiKeyDto
{
    public int Id { get; set; }
    public required string KeyName { get; set; }
    public required string CreatedBy { get; set; }
    public int ExpiresIn { get; set; }
    public required bool IsEnabled { get; set; }
}