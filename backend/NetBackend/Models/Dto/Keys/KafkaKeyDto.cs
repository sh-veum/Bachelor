namespace NetBackend.Models.Dto.Keys;

public class KafkaKeyDto : IApiKeyDto
{
    public required List<string> Topics { get; set; }
}