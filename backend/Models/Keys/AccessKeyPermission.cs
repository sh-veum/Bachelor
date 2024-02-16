namespace NetBackend.Models.Keys;

public class AccessKeyPermission
{
    public Guid Id { get; set; }
    public required string QueryName { get; set; }
    public required List<string>? AllowedFields { get; set; }
    public int GraphQLApiKeyId { get; set; }
    public GraphQLApiKey? GraphQLApiKey { get; set; }
}
