namespace NetBackend.Models.Dto.Keys;

public class KafkaTopicDto
{
    public required string SensorId { get; set; }
    public required List<string> Topics { get; set; }
}