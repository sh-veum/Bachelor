using Confluent.Kafka;

namespace MockSensors.Sensors;

public abstract class SensorBase
{
    protected readonly IProducer<string, string> _producer;
    protected readonly ILogger _logger;
    protected readonly string _topic;
    protected CancellationTokenSource? _cancellationTokenSource;

    protected SensorBase(ILogger logger, string topic, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var producerConfig = new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _topic = !string.IsNullOrWhiteSpace(topic) ? topic : throw new ArgumentException("Topic cannot be null or whitespace.", nameof(topic));
    }

    public abstract void Start();
    public abstract void Stop();
}