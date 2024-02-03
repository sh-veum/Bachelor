namespace NetBackend.Models.User;

public class ApiKey
{
    public int Id { get; set; }
    public required string KeyName { get; set; }
    public required string UserId { get; set; }
    public required User User { get; set; }
    public List<string>? AccessibleEndpoints { get; set; }
}