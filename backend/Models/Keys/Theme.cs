namespace NetBackend.Models.Keys;

public class Theme
{
    public Guid Id { get; set; }
    public required string ThemeName { get; set; }
    public required List<string> AccessibleEndpoints { get; set; }
    public int ApiKeyID { get; set; }
    public ApiKey? ApiKey { get; set; }
}
