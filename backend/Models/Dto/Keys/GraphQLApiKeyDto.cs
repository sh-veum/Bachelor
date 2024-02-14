namespace NetBackend.Models.Keys.Dto;

public class GraphQLApiKeyDto : IApiKeyDto
{
    public List<string>? AllowedQueries { get; set; }
}