using NetBackend.Models.Enums;

namespace NetBackend.Models.Dto.Keys;

public class KafkaHistoricalRequestDto
{
    public required string SessionId { get; set; }
    public SensorType? SensorType { get; set; }
}
