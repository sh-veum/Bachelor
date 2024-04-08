namespace NetBackend.Models.Dto;

using NetBackend.Enums;

public class UserDatabaseNameDto
{
    public string? Email { get; set; }
    public DatabaseType Database { get; set; }
}
