using MockSensors.Dto;
using MockSensors.Enums;

namespace MockSensors.Sensors.Managers;

public interface ISensorManager
{
    bool TryStartSensor(string id);
    SensorStopResult TryStopSensor(string id);
    IEnumerable<SensorDto> GetActiveSensors();
    IEnumerable<SensorDto> GetAllSensorsWithStatus();
    void StopAllSensors();
}