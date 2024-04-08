using System.ComponentModel.DataAnnotations;

namespace NetBackend.Models;

public class Species
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }

    [GraphQLIgnore]
    public int? SuperSecretNumber { get; set; }
}