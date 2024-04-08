namespace NetBackend.Models.Dto.Keys;

public class CreateRestAccessKeyDto
{
    public required string KeyName { get; set; }
    public List<Guid>? ThemeIds { get; set; }
}