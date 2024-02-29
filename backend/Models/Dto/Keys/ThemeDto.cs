namespace NetBackend.Models.Dto.Keys;

public class ThemeDto
{
    public Guid? Id { get; set; }
    public required string ThemeName { get; set; }
    public required List<string> AccessibleEndpoints { get; set; }
}