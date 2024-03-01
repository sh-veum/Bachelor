namespace NetBackend.Models.Dto.Keys;

public class GraphQLApiKeyDto : IApiKeyDto
{
    public required List<GraphQLAccessKeyPermissionDto> GraphQLAccessKeyPermissionDto { get; set; }
}