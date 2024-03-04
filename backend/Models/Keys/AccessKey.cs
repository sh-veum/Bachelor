using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetBackend.Models.Keys;

public class AccessKey
{
    [Key]
    public Guid Id { get; set; }
    public string? KeyHash { get; set; }

    [ForeignKey("ApiKey")]
    public Guid? ApiKeyId { get; set; }
    public ApiKey? ApiKey { get; set; }

    [ForeignKey("GraphQLApiKey")]
    public Guid? GraphQLApiKeyId { get; set; }
    public GraphQLApiKey? GraphQLApiKey { get; set; }
}