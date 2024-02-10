namespace NetBackend.Models.Dto;

using NetBackend.Enums;

public class UserInfoDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? AssignedDatabase { get; set; }
    public string? Role { get; set; }
}
