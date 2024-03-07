namespace NetBackend.Models.Dto;

public class RestApiEndpointDto
{
    public required string Path { get; set; }
    public required string Method { get; set; }
    public Type? ExpectedBodyType { get; set; }
}

public class RestApiThemeDto
{
    public required string Name { get; set; }
    public required List<RestApiEndpointSchema> Endpoints { get; set; }
}


public class RestApiEndpointSchema
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