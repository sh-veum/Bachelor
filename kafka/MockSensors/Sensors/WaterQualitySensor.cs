using Confluent.Kafka;

namespace MockSensors.Sensors;

public class WaterQualitySensor : SensorBase
{
    // Starting values for pH, turbidity, and temperature.
    private double currentPH = 7.0; // Neutral pH value as a starting point
    private double currentTurbidity = 10.0; // Arbitrary starting turbidity
    private double currentTemperature = 20.0; // Assuming a moderate starting temperature

    public WaterQualitySensor(IConfiguration configuration, string topic, ILogger<WaterQualitySensor> logger)
       : base(logger, topic, configuration)
    {
        _logger.LogInformation($"Water quality sensor initialized with starting values: pH: {currentPH}, Turbidity: {currentTurbidity} NTU, Temperature: {currentTemperature}°C");
    }

    public override void Start()
    {
        _logger.LogInformation("Starting water quality sensor: " + _topic);
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var rnd = new Random();

        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                // Simulate gradual changes
                currentPH += rnd.NextDouble() * 0.2 - 0.05; // pH changes by up to ±0.1
                currentTurbidity += rnd.NextDouble() * 5 - 2.5; // Turbidity changes by up to ±2.5 NTU
                currentTemperature += rnd.NextDouble() * 2 - 0.1; // Temperature changes by up to ±1°C

                // Ensure values remain within realistic bounds
                currentPH = Math.Clamp(currentPH, 0, 14);
                currentTurbidity = Math.Clamp(currentTurbidity, 0, 100); // Adjust max value as appropriate
                currentTemperature = Math.Clamp(currentTemperature, -5, 35);

                var waterQualityMetric = $"TimeStamp: {DateTime.Now:o}, pH: {currentPH:0.00}, Turbidity: {currentTurbidity:0.00} NTU, Temperature: {currentTemperature:0.000}C";

                _producer.Produce(_topic, new Message<string, string> { Key = DateTime.Now.ToString("o"), Value = waterQualityMetric },
                    (deliveryReport) =>
                    {
                        if (deliveryReport.Error.Code != ErrorCode.NoError)
                        {
                            Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine($"Produced event to topic {_topic}: key = {deliveryReport.Message.Key,-20} value = {deliveryReport.Message.Value}");
                        }
                    });

                _producer.Flush(TimeSpan.FromSeconds(1));

                try
                {
                    Task.Delay(5000, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, token);
    }

    public override void Stop()
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
        {
            Console.WriteLine("Sensor already stopped");
            return;
        }

        _logger.LogInformation("Stopping water quality sensor " + _topic);
        _cancellationTokenSource.Cancel();
    }
}