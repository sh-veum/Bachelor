namespace NetBackend.Models.Dto.Keys;

public class GraphQLAccessKeyPermissionDto
{
    public required string QueryName { get; set; }
    public required List<string> AllowedFields { get; set; }
}