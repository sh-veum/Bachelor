using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetBackend.Models.Keys;

public class AccessKey
{
    [Key]
    public Guid Id { get; set; }
    public string? KeyHash { get; set; }

    public IApiKey? IApiKey { get; set; }
}