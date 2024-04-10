namespace NetBackend.Models.Dto;

public class StartSensorRequestDto
{
    public bool SendHistoricalData { get; set; }
    public string? SessionId { get; set; }
}
