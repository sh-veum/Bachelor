namespace NetBackend.Services.Interfaces;

public interface ISensorService
{
    Task<(bool success, string message)> StartWaterQualitySensorAsync(string sensorId);
    Task<(bool success, string message)> StopWaterQualitySensorAsync(string sensorId);
    Task<bool> StopAllSensorsAsync();
}