using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class GraphQLApiKey
{
    public int Id { get; set; }
    public string? KeyName { get; set; }
    public string? UserId { get; set; }
    public required UserModel User { get; set; }
    public List<string>? AllowedQueries { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExpiresIn { get; set; }
}