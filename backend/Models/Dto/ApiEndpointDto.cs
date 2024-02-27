namespace NetBackend.Models.Dto;

public class ApiEndpointDto
{
    public required string Path { get; set; }
    public required string Method { get; set; }
    public Type? ExpectedBodyType { get; set; }
}

public class ApiThemeDto
{
    public required string Name { get; set; }
    public required List<ApiEndpointSchema> Endpoints { get; set; }
}


public class ApiEndpointSchema
{
    public required string Path { get; set; }
    public required string Method { get; set; }
    public required List<PropertyDto> Properties { get; set; }
}

public class PropertyDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}