namespace NetBackend.Models.Dto.Keys;

public class StartSensorRequestDto
{
    public bool SendHistoricalData { get; set; }
    public string? SessionId { get; set; }
}
