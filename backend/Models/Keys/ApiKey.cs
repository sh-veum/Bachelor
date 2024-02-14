using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class ApiKey
{
    public int Id { get; set; }
    public string? KeyName { get; set; }
    public string? UserId { get; set; }
    public required UserModel User { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExpiresIn { get; set; }
}