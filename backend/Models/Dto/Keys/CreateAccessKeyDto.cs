namespace NetBackend.Models.Dto.Keys;

public class CreateAccessKeyDto
{
    public required string KeyName { get; set; }
    public List<ThemeDto>? Themes { get; set; }
}