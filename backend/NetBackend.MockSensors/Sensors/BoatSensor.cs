using Confluent.Kafka;

namespace MockSensors.Sensors;

public class BoatSensor : SensorBase
{

    // Initial and final coordinates for the boat's journey.
    private (double Latitude, double Longitude) _currentPosition;
    private readonly (double Latitude, double Longitude) _endPosition;

    public BoatSensor(IConfiguration configuration, string topic, ILogger<BoatSensor> logger)
        : base(logger, topic, configuration)
    {
        var rnd = new Random();
        _currentPosition = GenerateRandomPosition(rnd);
        _endPosition = GenerateRandomPosition(rnd);

        _logger.LogInformation($"Boat sensor initialized with start position: {_currentPosition} and end position: {_endPosition}");
    }

    private (double Latitude, double Longitude) GenerateRandomPosition(Random rnd)
    {
        // Example range for latitude and longitude could be tailored to specific needs
        double latitude = rnd.NextDouble() * 180 - 90; // Latitude range -90 to 90
        double longitude = rnd.NextDouble() * 360 - 180; // Longitude range -180 to 180
        return (latitude, longitude);
    }

    public override void Start()
    {
        _logger.LogInformation($"Starting boat sensor: {_topic}");
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var rnd = new Random();

        Task.Run(() =>
        {
            while (!token.IsCancellationRequested && !_currentPosition.Equals(_endPosition))
            {
                // Simulate boat movement towards the end position.
                _currentPosition.Latitude += (_endPosition.Latitude - _currentPosition.Latitude) / 10 + (rnd.NextDouble() * 0.01 - 0.005);
                _currentPosition.Longitude += (_endPosition.Longitude - _currentPosition.Longitude) / 10 + (rnd.NextDouble() * 0.01 - 0.005);

                var locationUpdate = $"TimeStamp: {DateTime.Now:o}, Latitude: {_currentPosition.Latitude:0.0000}, Longitude: {_currentPosition.Longitude:0.0000}";

                _producer.Produce(_topic, new Message<string, string> { Key = DateTime.Now.ToString("o"), Value = locationUpdate },
                    (deliveryReport) =>
                    {
                        if (deliveryReport.Error.Code != ErrorCode.NoError)
                        {
                            _logger.LogError($"Failed to deliver message: {deliveryReport.Error.Reason}");
                        }
                        else
                        {
                            _logger.LogInformation($"Produced event to topic {_topic}: key = {deliveryReport.Message.Key,-20} value = {deliveryReport.Message.Value}");
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
            _logger.LogInformation("Boat sensor already stopped");
            return;
        }

        _logger.LogInformation($"Stopping boat sensor {_topic}");
        _cancellationTokenSource.Cancel();
    }
}
