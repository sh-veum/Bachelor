using System.Text.Json;
using Confluent.Kafka;
using NetBackend.Services.Interfaces.Kafka;

namespace NetBackend.Services.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        _logger.LogInformation($"Kafka producer configured with bootstrap servers: {producerConfig.BootstrapServers}");
    }

    public async Task ProduceAsync<T>(string topic, T message)
    {
        try
        {
            var messageString = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = messageString });
            _logger.LogInformation($"Message produced to Kafka topic '{topic}': {messageString}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error producing to Kafka: {ex.Message}");
            throw new Exception($"Error producing to Kafka: {ex.Message}");
        }
    }
}