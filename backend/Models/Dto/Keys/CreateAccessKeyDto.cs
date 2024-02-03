namespace NetBackend.Models.Keys.Dto;

public class CreateAccessKeyDto
{
    public required string KeyName { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}