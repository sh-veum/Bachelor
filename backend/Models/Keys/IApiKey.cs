using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class IApiKey
{
    [Key]
    public Guid Id { get; set; }
    public required string KeyName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExpiresIn { get; set; }
    public required bool IsEnabled { get; set; }

    public AccessKey? AccessKey { get; set; }

    [ForeignKey("User")]
    public required string UserId { get; set; }
    public required UserModel User { get; set; }
}