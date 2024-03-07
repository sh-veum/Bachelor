namespace NetBackend.Models.Dto.Keys;

public class RestApiKeyDto : IApiKeyDto
{
    public List<ThemeDto>? Themes { get; set; }
}