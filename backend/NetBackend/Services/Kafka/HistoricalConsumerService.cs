using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.Kafka;
using NetBackend.Services.Interfaces.MessageHandler;

namespace NetBackend.Services.Kafka;

public class HistoricalConsumerService : BackgroundService, IHistoricalConsumerService
{
    private readonly ILogger<HistoricalConsumerService> _logger;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, SensorType> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;
    private IConsumer<Ignore, string>? _consumer;
    private volatile string _currentSessionId;
    private IAdminClient? _adminClient;

    public HistoricalConsumerService(IConfiguration configuration, ILogger<HistoricalConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;
        _activeTopics = new ConcurrentDictionary<string, SensorType>();
        _configuration = configuration;
        _currentSessionId = string.Empty;

        InitializeConsumer();
        InitializeAdminClient();
    }

    private void InitializeConsumer()
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    private void InitializeAdminClient()
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };
        _adminClient = new AdminClientBuilder(adminConfig).Build();
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
            if (_consumer == null) return;

            while (!stoppingToken.IsCancellationRequested && !_loopCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);

                        if (consumeResult != null && !consumeResult.IsPartitionEOF)
                        {
                            _logger.LogInformation($"Historical Consume Loop Sending Topic: {consumeResult.Topic}, Message: {consumeResult.Message.Value}, Offset: {consumeResult.Offset}, SessionId: {_currentSessionId}");

                            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, consumeResult.Offset, _currentSessionId);

                            HandleMessage(consumeResult.Message.Value, consumeResult.Topic, consumeResult.Offset);

                            if (IsLastOffset(consumeResult.Topic, consumeResult.Offset))
                            {
                                _logger.LogInformation($"Last message consumed for topic {consumeResult.Topic}. Unsubscribing and resetting session.");
                                _consumer.Unsubscribe();
                                _activeTopics.TryRemove(consumeResult.Topic, out _);
                                _currentSessionId = string.Empty;
                            }
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


    public void SubscribeToTopic(string newTopic, SensorType sensorType, string sessionId)
    {
        _logger.LogInformation($"Historical Consumer subscribing to topic: {newTopic} with sensor type: {sensorType} and session id: {sessionId}.");

        _currentSessionId = sessionId;
        _activeTopics.TryAdd(newTopic, sensorType);
        ResetConsumerAndRestart(newTopic);
    }

    private void ResetConsumerAndRestart(string newTopic)
    {
        if (_consumer != null)
        {
            try
            {
                _consumer.Unsubscribe();
                _consumer.Close();
                _consumer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up Kafka consumer: {ex.Message}");
            }
        }

        // Re-initialize the consumer
        InitializeConsumer();
        if (_consumer != null)
        {
            _consumer.Subscribe(newTopic);
            InterruptAndRestartConsumeLoop();
        }
        else
        {
            _logger.LogError("Failed to reinitialize the Kafka consumer.");
        }
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

    private async Task SendMessageToWebSocket(string topic, string message, long offset, string currentSessionId)
    {
        var webSocketMessage = new
        {
            topic,
            message,
            offset
        };
        var serializedMessage = JsonSerializer.Serialize(webSocketMessage);

        _logger.LogInformation($"Historical: Sending message to WebSocket: {serializedMessage}, Topic: {topic}, Offset: {offset}, SessionId: {currentSessionId}");

        await _webSocketManager.SendMessageAsync(serializedMessage, topic, currentSessionId);
    }

    private bool IsLastOffset(string topic, Offset currentOffset)
    {
        if (_adminClient != null && _consumer != null)
        {
            var metadata = _adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
            foreach (var partition in metadata.Topics[0].Partitions)
            {
                var watermarkOffsets = _consumer.QueryWatermarkOffsets(new TopicPartition(topic, new Partition(partition.PartitionId)), TimeSpan.FromSeconds(5));
                if (currentOffset == watermarkOffsets.High - 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
}