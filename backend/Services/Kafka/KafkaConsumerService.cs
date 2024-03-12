using System.Text.Json;
using Confluent.Kafka;
using NetBackend.Constants;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly List<string> _topics;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _topics = [
            KafkaConstants.SpeciesTopic,
            KafkaConstants.OrgTopic,
            KafkaConstants.RestKeyTopic,
            KafkaConstants.GraphQLKeyTopic
            ];
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topics);
        _logger.LogInformation($"Subscribed to topics: {string.Join(", ", _topics)}");

        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContextService = scope.ServiceProvider.GetRequiredService<DbContextService>();

                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        var webSocketMessage = new
                        {
                            topic = consumeResult.Topic,
                            message = consumeResult.Message.Value
                        };
                        var serializedMessage = JsonSerializer.Serialize(webSocketMessage);
                        await _webSocketManager.SendMessageAsync(serializedMessage);
                        _logger.LogInformation($"WebSocket message sent: {serializedMessage}");
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