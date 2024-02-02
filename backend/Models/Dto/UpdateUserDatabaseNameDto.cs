namespace Name.Models.Dto;

using backend.Enums;

public class UpdateUserDatabaseNameDto
{
    public string? Email { get; set; }
    public DatabaseType Database { get; set; }
}
