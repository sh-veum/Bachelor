using Microsoft.AspNetCore.Identity;
using NetBackend.Models.Keys;

namespace NetBackend.Models.User;

public class UserModel : IdentityUser
{
    public string? DatabaseName { get; set; }
    public List<RestApiKey>? ApiKey { get; set; }
    public List<GraphQLApiKey>? GraphQLApiKey { get; set; }
    public List<KafkaKey>? KafkaKeys { get; set; }
    public List<Theme>? Themes { get; set; }
}