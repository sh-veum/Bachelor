using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetBackend.Models.Keys;

public class AccessKeyPermission
{
    [Key]
    public Guid Id { get; set; }
    public required string QueryName { get; set; }
    public required List<string>? AllowedFields { get; set; }

    [ForeignKey("GraphQLApiKey")]
    public Guid? GraphQLApiKeyId { get; set; }
    public GraphQLApiKey? GraphQLApiKey { get; set; }
}
