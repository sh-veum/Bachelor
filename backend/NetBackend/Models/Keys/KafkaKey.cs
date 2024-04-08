namespace NetBackend.Models.Keys;

public class KafkaKey : IApiKey
{
    public required List<string> Topics { get; set; }
}