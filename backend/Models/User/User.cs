using Microsoft.AspNetCore.Identity;

namespace NetBackend.Models.User;

public class User : IdentityUser
{
    public string? DatabaseName { get; set; }
    public List<ApiKey>? ApiKey { get; set; }
}