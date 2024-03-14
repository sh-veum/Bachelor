namespace MockSensors.Dto;

public class SensorDto
{
    public string SensorId { get; set; }
    public bool Active { get; set; }

    public SensorDto(string sensorId, bool active)
    {
        SensorId = sensorId;
        Active = active;
    }
}
