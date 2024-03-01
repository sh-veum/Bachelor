using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class IApiKey
{
    public int Id { get; set; }
    public required string KeyName { get; set; }
    public required string UserId { get; set; }
    public required UserModel User { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExpiresIn { get; set; }
}