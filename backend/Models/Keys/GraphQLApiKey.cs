namespace NetBackend.Models.Keys;

public class GraphQLApiKey : IApiKey
{
    public List<string>? AllowedQueries { get; set; }
}