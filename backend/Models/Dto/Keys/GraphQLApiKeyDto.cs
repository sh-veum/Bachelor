using NetBackend.Models.Dto.Keys;

namespace NetBackend.Models.Keys.Dto;

public class GraphQLApiKeyDto : IApiKeyDto
{
    public required List<GraphQLAccessKeyPermissionDto> GraphQLAccessKeyPermissionDto { get; set; }
}