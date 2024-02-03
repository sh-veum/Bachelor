namespace NetBackend.Models.Dto;

public class ApiKeyDto
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}