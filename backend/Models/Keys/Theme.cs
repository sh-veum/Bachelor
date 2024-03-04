using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetBackend.Models.User;

namespace NetBackend.Models.Keys;

public class Theme
{
    [Key]
    public Guid Id { get; set; }
    public required string ThemeName { get; set; }
    public required List<string> AccessibleEndpoints { get; set; }

    [ForeignKey("ApiKey")]
    public Guid? ApiKeyID { get; set; }
    public ApiKey? ApiKey { get; set; }

    [ForeignKey("User")]
    public required string UserId { get; set; }
    public required UserModel User { get; set; }
}
