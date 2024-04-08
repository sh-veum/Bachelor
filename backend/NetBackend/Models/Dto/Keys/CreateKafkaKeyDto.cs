namespace NetBackend.Models.Dto.Keys;

public class CreateKafkaKeyDto
{
    public required string KeyName { get; set; }
    public required List<string> Topics { get; set; }
}