using Confluent.Kafka;
using NetBackend.Services.WebSocket;

namespace NetBackend.Services.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly string _topic = "key-updates";
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IAppWebSocketManager _webSocketManager;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, IAppWebSocketManager webSocketManager)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = $"{_topic}-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _logger = logger;
        _webSocketManager = webSocketManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation($"Subscribed to {_topic}");

        // Run the consuming loop in a separate task
        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(100);
                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        _logger.LogInformation($"Message received: {consumeResult.Message.Value}");
                        await _webSocketManager.SendMessageAsync(consumeResult.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error consuming Kafka message: {ex.Message}");
                }
            }

            _consumer.Close();
        }, stoppingToken);

        return Task.CompletedTask;
    }
}