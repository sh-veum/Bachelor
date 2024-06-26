using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.MessageHandler;

namespace NetBackend.Services.Kafka;

public class KafkaConsumerService : BackgroundService, IKafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, SensorType> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;
        _activeTopics = new ConcurrentDictionary<string, SensorType>();
        _configuration = configuration;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        StartConsumeLoop(_stoppingCancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    private void StartConsumeLoop(CancellationToken stoppingToken)
    {
        Task.Run(async () =>
        {
            if (_consumer.Subscription.Count == 0)
            {
                _consumer.Subscribe(_activeTopics.Keys);
            }

            while (!stoppingToken.IsCancellationRequested && !_loopCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);

                        if (consumeResult != null && !consumeResult.IsPartitionEOF)
                        {
                            _logger.LogInformation($"Consume Loop Sending Topic: {consumeResult.Topic}, Message: {consumeResult.Message.Value}, Offset: {consumeResult.Offset}");

                            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, consumeResult.Offset);

                            HandleMessage(consumeResult.Message.Value, consumeResult.Topic, consumeResult.Offset);
                        }
                    }
                    catch (ConsumeException ex) when (ex.Error.Reason.Contains("Unknown topic or partition"))
                    {
                        _logger.LogWarning($"Topic not available yet, waiting before retrying. Error: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(1)); // Wait for 5 seconds before retrying
                        ResetLoopCancellationToken();
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Reconfiguring consumer subscriptions, restarting consume loop...");
                        ResetLoopCancellationToken();
                        continue; // Continue in the loop after resetting the token.
                    }
                    else
                    {
                        _logger.LogInformation("Stopping due to application shutdown.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error consuming Kafka message: {ex.Message}");
                    // await Task.Delay(TimeSpan.FromSeconds(1)); // Wait for 1 seconds before retrying
                }
            }

            _logger.LogInformation("Closing main consumer...");
            _consumer.Close();
        }, stoppingToken);
    }

    public void SubscribeToTopic(string newTopic, SensorType sensorType)
    {
        _logger.LogInformation($"Subscribing to topic: {newTopic}");

        _consumer.Unsubscribe();

        if (_activeTopics.TryAdd(newTopic, sensorType))
        {
            _logger.LogInformation($"activeTopics: {_activeTopics.Keys}");
            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
        }

        InterruptAndRestartConsumeLoop();
    }

    private void HandleMessage(string message, string topic, long currentOffset)
    {
        Task.Run(async () =>
        {
            try
            {

                if (_activeTopics.TryGetValue(topic, out var sensorType))
                {
                    if (sensorType != SensorType.none)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var factory = scope.ServiceProvider.GetRequiredService<IMessageHandlerFactory>();

                        var handler = factory.GetHandler(sensorType);
                        await handler.HandleMessage(message, topic, currentOffset);
                    }
                    else
                    {
                        _logger.LogInformation($"Sensor type is none, message not saved to database.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to store message in database. Error: {ex.Message}");
            }
        });
    }

    private void InterruptAndRestartConsumeLoop()
    {
        _loopCancellationTokenSource.Cancel();
        ResetLoopCancellationToken();
        if (_stoppingCancellationTokenSource != null)
        {
            StartConsumeLoop(_stoppingCancellationTokenSource.Token);
        }
        else
        {
            _logger.LogWarning("Stopping token source is null, cannot restart consume loop.");
        }
    }

    private void ResetLoopCancellationToken()
    {
        _loopCancellationTokenSource.Dispose();
        _loopCancellationTokenSource = new CancellationTokenSource();
    }

    private async Task SendMessageToWebSocket(string topic, string message, long offset)
    {
        var webSocketMessage = new
        {
            topic,
            message,
            offset
        };
        var serializedMessage = JsonSerializer.Serialize(webSocketMessage);

        _logger.LogInformation($"Sending message to WebSocket: {serializedMessage}, Topic: {topic}, Offset: {offset}");

        await _webSocketManager.SendMessageAsync(serializedMessage, topic);

    }
}