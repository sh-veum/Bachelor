namespace NetBackend.Models.Dto.Keys;

public class ApiKeyDto : IApiKeyDto
{
    public List<ThemeDto>? Themes { get; set; }
}