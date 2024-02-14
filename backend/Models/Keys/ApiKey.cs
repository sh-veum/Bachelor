namespace NetBackend.Models.Keys;

public class ApiKey : IApiKey
{
    public List<string>? AccessibleEndpoints { get; set; }
}