using NetBackend.Models.Enums;

namespace NetBackend.Services.Interfaces;

public interface ISensorService
{
    Task<(bool success, string message)> StartSensorAsync(string sensorId, SensorType sensorType, bool sendHistoricalData);
    Task<(bool success, string message)> StopSensorAsync(string sensorId, SensorType sensorType);
    Task<bool> StopAllSensorsAsync(SensorType sensorType);
    Task<(bool success, string message)> GetActiveSensors(SensorType sensorType);
}