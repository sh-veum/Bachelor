using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.MessageHandler;

namespace NetBackend.Services.Kafka;

public class SensorConsumerService : BackgroundService, ISensorConsumerService
{
    private readonly ILogger<SensorConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, SensorType> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;

    public SensorConsumerService(IConfiguration configuration, ILogger<SensorConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
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
        _logger.LogInformation($"Stopping token: {stoppingToken}");
        _stoppingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _logger.LogInformation($"Stopping Cancel token: {_stoppingCancellationTokenSource.Token}");
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
                            // Directly handle and send the message if not processing historical data
                            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, consumeResult.Offset);
                            HandleMessage(consumeResult.Message.Value, consumeResult.Topic, consumeResult.Offset);
                        }
                    }
                    catch (ConsumeException ex) when (ex.Error.Reason.Contains("Unknown topic or partition"))
                    {
                        _logger.LogWarning($"Topic not available yet, waiting before retrying. Error: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds before retrying
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
                }
            }

            _logger.LogInformation("Closing main consumer...");
            _consumer.Close();
        }, stoppingToken);
    }

    public async Task SubscribeToTopicAsync(string newTopic, SensorType sensorType, bool sendHistoricalData = false)
    {
        _logger.LogInformation($"Subscribing to topic: {newTopic}, sendHistoricalData: {sendHistoricalData}");

        _consumer.Unsubscribe();

        var isNewSubscription = _activeTopics.TryAdd(newTopic, sensorType);

        if (isNewSubscription)
        {
            _logger.LogInformation($"activeTopics: {_activeTopics.Keys}");
            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
        }

        if (sendHistoricalData)
        {
            await ConsumeHistoricalData(newTopic);
        }

        InterruptAndRestartConsumeLoop();
    }

    // TODO: Send to a separate WebSocket endpoint for historical data
    private async Task ConsumeHistoricalData(string topic)
    {
        var partitionBuffers = new Dictionary<int, SortedList<long, ConsumeResult<Ignore, string>>>();
        var adminConfig = new AdminClientConfig { BootstrapServers = _configuration["Kafka:BootstrapServers"] };
        using (var adminClient = new AdminClientBuilder(adminConfig).Build())
        {
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(10));
            foreach (var partition in metadata.Topics[0].Partitions)
            {
                partitionBuffers[partition.PartitionId] = [];
                _consumer.Assign(new TopicPartitionOffset(topic, partition.PartitionId, Offset.Beginning));
            }
        }

        bool shouldContinue = true;
        while (shouldContinue)
        {
            var consumeResult = _consumer.Consume(100); // Adjust the timeout as needed
            if (consumeResult == null)
            {
                shouldContinue = false;
                continue;
            }

            if (consumeResult.IsPartitionEOF)
            {
                // Process messages for this partition.
                await ProcessPartitionBuffer(partitionBuffers[consumeResult.Partition.Value], consumeResult.Partition.Value);
                partitionBuffers.Remove(consumeResult.Partition.Value);
                shouldContinue = partitionBuffers.Count > 0;
                continue;
            }

            if (consumeResult.Message != null)
            {
                // Add the message to the partition's buffer.
                partitionBuffers[consumeResult.Partition.Value].Add(consumeResult.Offset.Value, consumeResult);
            }
        }

        // Ensure all buffers are processed at the end.
        foreach (var kvp in partitionBuffers)
        {
            await ProcessPartitionBuffer(kvp.Value, kvp.Key);
        }

        // _consumer.Unassign();
        _consumer.Unsubscribe();
        _consumer.Subscribe(_activeTopics.Keys);
    }

    private async Task ProcessPartitionBuffer(SortedList<long, ConsumeResult<Ignore, string>> buffer, int partitionId)
    {
        foreach (var kvp in buffer)
        {
            var consumeResult = kvp.Value;

            var currentOffset = consumeResult.Offset.Value;
            _logger.LogInformation($"Processing message for Topic: {consumeResult.Topic}, Partition: {partitionId}, Offset: {currentOffset}");

            // await HandleMessage(consumeResult.Message.Value, consumeResult.Topic);
            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, currentOffset);
        }
        buffer.Clear();
    }

    private void HandleMessage(string message, string topic, long currentOffset)
    {
        Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IMessageHandlerFactory>();

                if (_activeTopics.TryGetValue(topic, out var sensorType))
                {
                    var handler = factory.GetHandler(sensorType);
                    await handler.HandleMessageAsync(message, topic, currentOffset);
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
        await _webSocketManager.SendMessageAsync(serializedMessage);
    }
}