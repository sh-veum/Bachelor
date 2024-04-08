namespace NetBackend.Models.Keys;

public class GraphQLApiKey : IApiKey
{
    public required List<AccessKeyPermission> AccessKeyPermissions { get; set; }
}