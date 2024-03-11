namespace NetBackend.Models.Dto.Keys;

public class CreateAccessKeyDto
{
    public required string KeyName { get; set; }
    public List<Guid>? ThemeIds { get; set; }
}