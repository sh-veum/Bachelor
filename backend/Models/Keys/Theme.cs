using System.ComponentModel.DataAnnotations;
using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class Theme
{
    [Key]
    public Guid Id { get; set; }
    public required string ThemeName { get; set; }
    public required List<string> AccessibleEndpoints { get; set; }
    public int? ApiKeyID { get; set; }
    public ApiKey? ApiKey { get; set; }
    public required string UserId { get; set; }
    public required UserModel User { get; set; }
}
