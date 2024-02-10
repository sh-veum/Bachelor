namespace NetBackend.Models.Dto;

public class ApiEndpointDto
{
    public required string Path { get; set; }
    public required string Method { get; set; }
    public Type? ExpectedBodyType { get; set; }
}